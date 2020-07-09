using System;

namespace ExcelTool {

    internal static class Program {
        public static void Main(string[] args) {
            ExcelSplit.Split(new ExcelSplit.ExcelSplitInfo() {
                filePath = "2020年1月_12月T表+S表.xlsx",
                sheetName = "V29-S表"
            });
        }
    }

}