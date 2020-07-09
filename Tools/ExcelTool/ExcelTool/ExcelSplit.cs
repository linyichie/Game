using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ExcelTool {

    public static class ExcelSplit {
        public struct ExcelSplitInfo {
            public string filePath;
            public string sheetName;
        }

        private static Dictionary<int, string> titles;
        public static Dictionary<int, string> Departments { get; private set; }

        public static void Split(ExcelSplitInfo splitInfo) {
            using (var fs = new FileStream(splitInfo.filePath, FileMode.Open, FileAccess.Read)) {
                IWorkbook workbook = null;
                var extension = Path.GetExtension(splitInfo.filePath).ToLower();
                if (extension.EndsWith("xlsx")) {
                    workbook = new XSSFWorkbook(fs);
                } else if (extension.EndsWith("xls")) {
                    workbook = new HSSFWorkbook(fs);
                }
                if (workbook != null) {
                    var sheet = workbook.GetSheet(splitInfo.sheetName);
                    if (sheet == null) {
                        Console.Write($"未找到分页：{splitInfo.sheetName}");
                    } else {
                        InitializeTitles(sheet);
                        Departments = ExcelUtils.GetDepartments(sheet);
                        var formDatas = new List<SFormData>();
                        var firstRowIndex = sheet.FirstRowNum;
                        var lastRowIndex = sheet.LastRowNum;
                        if (lastRowIndex > 0) {
                            for (int i = 1; i <= lastRowIndex; i++) {
                                var row = sheet.GetRow(i);
                                if (row == null) {
                                    continue;
                                }
                                SFormData formData = new SFormData(sheet, i);
                                formDatas.Add(formData);
                            }
                        }
                        Split(formDatas);
                    }
                }
            }
        }

        private static void InitializeTitles(ISheet sheet) {
            var row = sheet.GetRow(0);
            titles = new Dictionary<int, string>();
            var exportColumns = Config.ExportColumns;
            foreach (var columnIndex in exportColumns.Keys) {
                titles.Add(exportColumns[columnIndex], row.GetCell(columnIndex).StringCellValue);
            }
            titles.Add(Config.DepartmentColumnIndex, Config.DepartmentTitleName);
            titles.Add(Config.PrincipalColumnIndex, Config.PrincipalTitleName);
            foreach (var columnIndex in Config.AddColumns.Keys) {
                titles.Add(columnIndex, Config.AddColumns[columnIndex]);
            }
        }

        private static int CreateFirstRow(ISheet sheet) {
            var row = sheet.CreateRow(0);
            var columnCount = -1;
            foreach (var columnIndex in titles.Keys) {
                if (columnIndex >= columnCount) {
                    columnCount = columnIndex;
                }
            }
            columnCount = columnCount + 1;
            if (columnCount > 0) {
                var sheetCells = new XSSFCell[columnCount];
                for (int i = 0; i < columnCount; i++) {
                    sheetCells[i] = row.CreateCell(i) as XSSFCell;
                    sheetCells[i].SetCellValue(titles.ContainsKey(i) ? titles[i] : string.Empty);
                }
            }
            return columnCount;
        }

        private static void Split(List<SFormData> formDatas) {
            if (formDatas.Count == 0) {
                return;
            }
            var departmentNames = Config.Departments;
            foreach (var departmentName in departmentNames) {
                var workbook = new XSSFWorkbook();
                workbook.CreateSheet("sheet1");
                var sheet = workbook.GetSheet("sheet1") as XSSFSheet;
                var columnCount = CreateFirstRow(sheet);
                var line = 1;
                for (int i = 0; i < formDatas.Count; i++) {
                    var formData = formDatas[i];
                    if (formData.departments.ContainsKey(departmentName)) {
                        var row = sheet.CreateRow(line);
                        var sheetCells = new XSSFCell[columnCount];
                        for (int j = 0; j < columnCount; j++) {
                            sheetCells[j] = row.CreateCell(j) as XSSFCell;
                            var formDataIndex = -1;
                            foreach (var columnIndex in Config.ExportColumns.Keys) {
                                if (Config.ExportColumns[columnIndex] == j) {
                                    formDataIndex = columnIndex;
                                    break;
                                }
                            }
                            if (j == Config.DepartmentColumnIndex) {
                                sheetCells[j].SetCellValue(departmentName);
                            }else if (j == Config.PrincipalColumnIndex) {
                                sheetCells[j].SetCellValue(formData.departments[departmentName]);
                            }
                            else if (formDataIndex!=-1) {
                                sheetCells[j].SetCellValue(formData.datas[formDataIndex]);
                            } else {
                                sheetCells[j].SetCellValue(string.Empty);
                            }
                        }
                        line += 1;
                    }
                }
                var directoryPath = System.Environment.CurrentDirectory;
                var filePath = Path.Combine(directoryPath, $"{departmentName}.xlsx");
                if (File.Exists(filePath)) {
                    File.Delete(filePath);
                }
                using (var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write)) {
                    workbook.Write(fs);
                }
                workbook.Close();
            }
        }
    }

}