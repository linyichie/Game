using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using UnityEditor;
using Debug = UnityEngine.Debug;

public static class ConfigFileOpenUtils {
    public static bool VerifyConfigFile(string path) {
        return path.Contains("Config") && path.ToLower().EndsWith(".txt");
    }

    public static void StartOpenConfigFile(string path) {
        var fileInfo = new FileInfo(path);
        var filePath = fileInfo.FullName;
        var excelFilePath = StringUtil.Concat(filePath.Substring(0, filePath.Length - 4).Replace("Config", "Config~"), ".xls");
        CreateExcelFile(filePath, excelFilePath);
        OpenExcelFile(excelFilePath, () => { ApplyConfigChange(filePath, excelFilePath); });
    }

    static void CreateExcelFile(string filePath, string excelFilePath) {
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
        var directoryPath = Path.GetDirectoryName(excelFilePath);
        if (!Directory.Exists(directoryPath)) {
            Directory.CreateDirectory(directoryPath);
        }
        if (File.Exists(excelFilePath)) {
            File.Delete(excelFilePath);
        }
        using (var fs = new FileStream(excelFilePath, FileMode.CreateNew, FileAccess.Write)) {
            excel.Write(fs);
        }
        excel.Close();
    }

    static void OpenExcelFile(string path, Action callback) {
        ThreadPool.QueueUserWorkItem((@object) => {
            using (var process = new System.Diagnostics.Process()) {
                var startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = path;
                process.StartInfo = startInfo;
                process.Start();
                while (!process.HasExited) {
                }
                Debug.Log(111);
                callback?.Invoke();
            }
        });
    }

    static void ApplyConfigChange(string filePath, string excelFilePath) {
        var dataTable = GetExcelData(excelFilePath);
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
                var line = string.Join("\t", values);
                sw.WriteLine(line);
            }
        }

        if (File.Exists(excelFilePath)) {
            File.Delete(excelFilePath);
        }
    }

    static DataTable GetExcelData(string excelFilePath) {
        using (var fs = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read)) {
            var dataTable = new DataTable();
            var excel = new HSSFWorkbook(fs);
            var sheet = excel.GetSheetAt(0);
            if (sheet == null) {
                return null;
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
            return dataTable;
        }
    }
}