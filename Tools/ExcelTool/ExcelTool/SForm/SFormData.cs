using System.Collections.Generic;
using System.Drawing;
using NPOI.SS.UserModel;
using Org.BouncyCastle.Utilities.IO;

namespace ExcelTool {

    public class SFormData {
        public readonly Dictionary<int, string> datas = new Dictionary<int, string>();
        public readonly Dictionary<string, string> departments = new Dictionary<string, string>();

        public SFormData(ISheet sheet, int rowIndex) {
            var row = sheet.GetRow(rowIndex);
            var columnCount = sheet.GetRow(0).LastCellNum;
            for (int i = 0; i < columnCount; i++) {
                if (ExcelSplit.Departments.ContainsKey(i)) {
                    var emptySign = Config.EmptySign;
                    var value = row.GetCell(i).StringCellValue;
                    if (string.IsNullOrEmpty(value) || emptySign == value) {
                        continue;
                    }
                    departments.Add(ExcelSplit.Departments[i], value);
                } else if (Config.ExportColumns.ContainsKey(i)) {
                    if (ExcelUtils.IsMergeCell(sheet, rowIndex, i, out var start, out var end)) {
                        datas.Add(i, sheet.GetRow(start.X).GetCell(start.Y).StringCellValue);
                    } else {
                        datas.Add(i, row.GetCell(i).StringCellValue);
                    }
                }
            }
        }
    }

}