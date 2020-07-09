using System.Collections.Generic;
using System.Drawing;
using NPOI.SS.UserModel;

namespace ExcelTool {

    public static class ExcelUtils {
        public static bool IsMergeCell(ISheet sheet, int rowIndex, int colIndex, out Point start, out Point end) {
            start = Point.Empty;
            end = Point.Empty;
            if (rowIndex < 0 || colIndex < 0) {
                return false;
            }
            bool result = false;
            int regionsCount = sheet.NumMergedRegions;
            for (int i = 0; i < regionsCount; i++) {
                var range = sheet.GetMergedRegion(i);
                if (rowIndex >= range.FirstRow && rowIndex <= range.LastRow && colIndex >= range.FirstColumn && colIndex <= range.LastColumn) {
                    start = new Point(range.FirstRow, range.FirstColumn);
                    end = new Point(range.LastRow, range.LastColumn);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public static Dictionary<int, string> GetDepartments(ISheet sheet) {
            var row = sheet.GetRow(0);
            var columnCount = row.LastCellNum;
            var departmentNames = Config.Departments;
            var departments = new Dictionary<int, string>();
            for (int i = 0; i < columnCount; i++) {
                var value = row.GetCell(i).StringCellValue;
                if (departmentNames.Contains(value)) {
                    departments.Add(i, value);
                }
            }
            return departments;
        }
    }

}