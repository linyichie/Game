using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class TxtFileOpenUtils {
    public static bool VerifyConfigFile(string path) {
        return path.StartsWith(TxtExportConfig.ConfigPath) && path.ToLower().EndsWith(".txt");
    }

    public static void StartOpenConfigFile(string path) {
        var fileInfo = new FileInfo(path);
        var filePath = fileInfo.FullName;
        var excelFilePath = StringUtil.Concat(Application.dataPath.Substring(0, Application.dataPath.Length - 6), "/Temp~/CacheTxtConfig/", Path.GetFileNameWithoutExtension(path), ".xls");
        if (File.Exists(excelFilePath) && IsFileOpened(excelFilePath)) {
            EditorUtility.DisplayDialog("警告", "文件已打开，请先关闭相应的 Excel 文件", "确认");
            return;
        }
        CreateExcelFile(filePath, excelFilePath);
        OpenExcelFile(excelFilePath, () => { ApplyConfigChange(filePath, excelFilePath); });
    }

    private static void CreateExcelFile(string filePath, string excelFilePath) {
        var excel = new HSSFWorkbook();
        excel.CreateSheet("sheet1");
        var sheet = excel.GetSheet("sheet1") as HSSFSheet;
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        for (int i = 0; i < lines.Length; i++) {
            sheet.CreateRow(i);
            var line = lines[i];
            var values = line.Split('\t');
            var sheetRow = sheet.GetRow(i) as HSSFRow;
            var sheetCells = new HSSFCell[values.Length];
            for (int j = 0; j < values.Length; ++j) {
                sheetCells[j] = sheetRow.CreateCell(j) as HSSFCell;
                sheetCells[j].SetCellValue(values[j]);
            }
        }
        if (File.Exists(excelFilePath)) {
            File.Delete(excelFilePath);
        }
        var directoryPath = Path.GetDirectoryName(excelFilePath);
        if (!Directory.Exists(directoryPath)) {
            Directory.CreateDirectory(directoryPath);
        }
        using (var fs = new FileStream(excelFilePath, FileMode.CreateNew, FileAccess.Write)) {
            excel.Write(fs);
        }
        excel.Close();
    }

    private static void OpenExcelFile(string path, Action callback) {
        ThreadPool.QueueUserWorkItem((@object) => {
            try {
                using (var process = new Process()) {
                    var startInfo = new ProcessStartInfo();
                    startInfo.FileName = path;
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitUtilFileExit(path);
                    if (!process.HasExited) {
                        process.CloseMainWindow();
                    }
                    callback?.Invoke();
                }
            } catch (Exception e) {
                Debug.LogError(e);
            }
        });
    }

    private static void ApplyConfigChange(string filePath, string excelFilePath) {
        var dataTable = GetExcelData(excelFilePath);
        if (dataTable != null) {
            var col = dataTable.Columns.Count;
            var row = dataTable.Rows.Count;

            var values = new List<string>(col);
            using (var fs = new FileStream(filePath, FileMode.Create))
            using (var sw = new StreamWriter(fs, Encoding.UTF8)) {
                for (int r = 0; r < row; r++) {
                    values.Clear();
                    for (int c = 0; c < col; c++) {
                        var value = dataTable.Rows[r][c] == null ? string.Empty : dataTable.Rows[r][c].ToString();
                        values.Add(value);
                    }
                    var emptyLength = values.FindAll(x => { return string.IsNullOrEmpty(x); })?.Count;
                    if (!emptyLength.HasValue || emptyLength.Value < values.Count) {
                        var line = string.Join("\t", values);
                        sw.WriteLine(line);
                    }
                }
            }
        }

        if (File.Exists(excelFilePath)) {
            File.Delete(excelFilePath);
        }
        
        ReloadRuntimeConfig(filePath);
    }

    private static void ReloadRuntimeConfig(string filePath) {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var fullName = StringUtil.Concat(fileName, "Config");
        var assembly = typeof(ConfigLoader).Assembly;
        var type = assembly.GetType(fullName);
        if (type == null) {
            return;
        }
        var property = type.GetProperty("Inited", BindingFlags.Static | BindingFlags.Public);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        var inited = (bool)property.GetValue(null);
        if (inited) {
            var rawDatasField = type.GetField("rawDatas", BindingFlags.Static | BindingFlags.NonPublic);
            var rawDatasDictionary = rawDatasField.GetValue(null) as System.Collections.IDictionary;
            var lines = File.ReadAllLines(filePath);
            for (var i = 3; i < lines.Length; i++) {
                var line = lines[i];
                var index = line.IndexOf("\t");
                var id = line.Substring(0, index);
                rawDatasDictionary[id] = line;
            }
            var configsField = type.GetField("configs", BindingFlags.Static | BindingFlags.NonPublic);
            var configsDictionary = configsField.GetValue(null) as System.Collections.IDictionary;
            foreach (var key in configsDictionary.Keys) {
                var config = configsDictionary[key];
                var line = rawDatasDictionary[key] as string;
                if (line != null) {
                    var values = line.Split('\t');
                    if (values.Length < fields.Length) {
                        Debug.LogError(fileName + ":" + key);
                        continue;
                    }
                    for (int i = 0; i < fields.Length; i++) {
                        SetConfigFieldValue(config, fields[i], values[i]);
                    }
                }
            }
        }
    }

    private static void SetConfigFieldValue(object instance, FieldInfo field, string value) {
        var fieldType = field.FieldType;
        switch (fieldType.Name.ToLower()) {
            case "string":
                field.SetValue(instance, value);
                break;
            case "string[]":
                field.SetValue(instance, value.Trim().Split(StringUtil.splitSeparator, StringSplitOptions.RemoveEmptyEntries));
                break;
            case "int32":
                int.TryParse(value, out var intValue);
                field.SetValue(instance, intValue);
                break;
            case "int32[]":
                var intStringArray = value.Trim().Split(StringUtil.splitSeparator, StringSplitOptions.RemoveEmptyEntries);
                var intArray = new int[intStringArray.Length];
                for (int i = 0; i < intStringArray.Length; i++) {
                    int.TryParse(intStringArray[i], out intArray[i]);
                }
                field.SetValue(instance, intArray);
                break;
            case "int64":
                long.TryParse(value, out var longValue);
                field.SetValue(instance, longValue);
                break;
            case "single":
                float.TryParse(value, out var floatValue);
                field.SetValue(instance, floatValue);
                break;
            case "single[]":
                var floatStringArray = value.Trim().Split(StringUtil.splitSeparator, StringSplitOptions.RemoveEmptyEntries);
                var floatArray = new float[floatStringArray.Length];
                for (int i = 0; i < floatStringArray.Length; i++) {
                    float.TryParse(floatStringArray[i], out floatArray[i]);
                }
                field.SetValue(instance, floatArray);
                break;
            case "boolean":
                int.TryParse(value, out var boolValue);
                field.SetValue(instance, boolValue != 0);
                break;
            case "vector3":
                field.SetValue(instance, value.Vector3Parse());
                break;
            case "vector3[]":
                var vectorStringArray = value.Trim().Split(StringUtil.splitSeparator, StringSplitOptions.RemoveEmptyEntries);
                var vectorArray = new Vector3[vectorStringArray.Length];
                for (int i = 0; i < vectorStringArray.Length; i++) {
                    vectorArray[i] = vectorStringArray[i].Vector3Parse();
                }
                field.SetValue(instance, vectorArray);
                break;
        }
    }

    private static DataTable GetExcelData(string excelFilePath) {
        var dataTable = new DataTable();
        using (var fs = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read)) {
            var excel = new HSSFWorkbook(fs);
            var sheet = excel.GetSheetAt(0);
            if (sheet?.GetRow(0) == null) {
                fs.Close();
                excel.Close();
                return dataTable;
            }
            var colCount = sheet.GetRow(0).LastCellNum;
            for (int i = 0; i < colCount; i++) {
                var column = new DataColumn();
                dataTable.Columns.Add(column);
            }

            var startRow = sheet.FirstRowNum;
            var lastRow = sheet.LastRowNum;
            for (int i = startRow; i <= lastRow; i++) {
                var row = sheet.GetRow(i);
                if (row == null) {
                    continue;
                }
                var dataRow = dataTable.NewRow();
                for (int j = row.FirstCellNum; j < colCount; ++j) {
                    var cell = row.GetCell(j);
                    if (cell == null) {
                        continue;
                    }
                    switch (cell.CellType) {
                        case CellType.Numeric:
                            dataRow[j] = cell.NumericCellValue;
                            break;
                        case CellType.Blank:
                            dataRow[j] = string.Empty;
                            break;
                        case CellType.Formula:
                            if (cell.CachedFormulaResultType == CellType.String) {
                                dataRow[j] = cell.StringCellValue;
                            } else {
                                dataRow[j] = cell.NumericCellValue;
                            }
                            break;
                        default:
                            dataRow[j] = cell.StringCellValue;
                            break;
                    }
                }
                dataTable.Rows.Add(dataRow);
            }
            excel.Close();
        }
        return dataTable;
    }

    private static void WaitUtilFileExit(this Process process, string path) {
        if (process != null) {
            while (true) {
                Thread.Sleep(1000);
                if (!IsFileOpened(path)) {
                    break;
                }
            }
        }
    }

    private static bool IsFileOpened(string path) {
        FileStream fileStream = null;
        var isOpened = false;
        try {
            fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
        } catch (IOException ioEx) {
            isOpened = true;
        } catch (Exception ex) {
            isOpened = true;
        } finally {
            fileStream?.Close();
        }
        return isOpened;
    }
}