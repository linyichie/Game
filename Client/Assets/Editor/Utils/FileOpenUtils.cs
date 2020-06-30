using System.IO;
using System.Text;
using NPOI.HSSF.UserModel;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class FileOpenUtils {
    [OnOpenAsset(1)]
    static bool OpenAssetStep1(int instanceId, int line) {
        var path = AssetDatabase.GetAssetPath(instanceId);
        if (VerifyTxtConfig(path)) {
            OpenTxtConfig(path);
            return true;
        }
        return false;
    }

    [OnOpenAsset(2)]
    static bool OpenAssetStep2(int instanceId, int line) {
        return false;
    }

    static bool VerifyTxtConfig(string path) {
        return path.Contains("Config") && path.ToLower().EndsWith(".txt");
    }

    static void OpenTxtConfig(string path) {
        var fileInfo = new FileInfo(path);
        var filePath = fileInfo.FullName;
        var cacheFilePath = StringUtil.Concat(filePath.Substring(0, filePath.Length - 4).Replace("Config", "Config~"), ".xls");
        CreateExcelFile(filePath, cacheFilePath);
        var process = new System.Diagnostics.Process();
        var startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = cacheFilePath;
        process.StartInfo = startInfo;
        process.Start();
    }

    static void CreateExcelFile(string txtFilePath, string cacheFilePath) {
        var excel = new HSSFWorkbook();
        excel.CreateSheet("sheet1");
        var sheet = excel.GetSheet("sheet1") as HSSFSheet;
        var lines = File.ReadAllLines(txtFilePath, Encoding.UTF8);
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
        var directoryPath = Path.GetDirectoryName(cacheFilePath);
        if (!Directory.Exists(directoryPath)) {
            Directory.CreateDirectory(directoryPath);
        }
        if (File.Exists(cacheFilePath)) {
            File.Delete(cacheFilePath);
        }
        using (var fs = new FileStream(cacheFilePath, FileMode.CreateNew, FileAccess.Write)) {
            excel.Write(fs);
        }
        excel.Close();
    }
}