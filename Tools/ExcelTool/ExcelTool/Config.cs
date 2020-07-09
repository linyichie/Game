using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace ExcelTool {

    public static class Config {
        private static readonly string xmlDocumentPath = "Config.xml";
        private static List<string> departments;
        private static int departmentColumnIndex = -1;
        private static string departmentTitleName = string.Empty;
        private static int principalColumnIndex = -1;
        private static string principalTitleName = string.Empty;
        private static string emptySign = string.Empty;
        private static Dictionary<int, int> exportColumns;
        private static Dictionary<int, string> addColumns;

        private static XmlDocument document = null;
        private static XmlDocument Document {
            get {
                if (document == null) {
                    document = new XmlDocument();
                    document.Load(xmlDocumentPath);
                }
                return document;
            }
        }

        public static List<string> Departments {
            get {
                return departments ?? (departments = new List<string>(GetElementValue("S_Form", "Departments").Split('|')));
            }
        }

        public static int DepartmentColumnIndex {
            get {
                if (departmentColumnIndex == -1) {
                    departmentColumnIndex = int.Parse(GetElementValue("S_Form", "DepartmentColumn").Split('|')[0].Split('_')[0]);
                }
                return departmentColumnIndex;
            }
        }

        public static string DepartmentTitleName {
            get {
                if (string.IsNullOrEmpty(departmentTitleName)) {
                    departmentTitleName = GetElementValue("S_Form", "DepartmentColumn").Split('|')[0].Split('_')[1];
                }
                return departmentTitleName;
            }
        }
        
        public static int PrincipalColumnIndex {
            get {
                if (principalColumnIndex == -1) {
                    principalColumnIndex = int.Parse(GetElementValue("S_Form", "DepartmentColumn").Split('|')[1].Split('_')[0]);
                }
                return principalColumnIndex;
            }
        }

        public static string PrincipalTitleName {
            get {
                if (string.IsNullOrEmpty(principalTitleName)) {
                    principalTitleName = GetElementValue("S_Form", "DepartmentColumn").Split('|')[1].Split('_')[1];
                }
                return principalTitleName;
            }
        }

        public static string EmptySign {
            get {
                if (string.IsNullOrEmpty(emptySign)) {
                    emptySign = GetElementValue("S_Form", "EmptySign");
                }
                return emptySign;
            }
        }

        public static Dictionary<int, int> ExportColumns {
            get {
                if (exportColumns == null) {
                    exportColumns = new Dictionary<int, int>();
                    var stringArray = GetElementValue("S_Form", "ExportColumns").Split('|');
                    for (int i = 0; i < stringArray.Length; i++) {
                        var columnStringArray = stringArray[i].Split('_');
                        exportColumns.Add(int.Parse(columnStringArray[0]), int.Parse(columnStringArray[1]));
                    }
                }
                return exportColumns;
            }
        }

        public static Dictionary<int, string> AddColumns {
            get {
                if (addColumns == null) {
                    addColumns = new Dictionary<int, string>();
                    var stringArray = GetElementValue("S_Form", "AddColumns").Split('|');
                    for (int i = 0; i < stringArray.Length; i++) {
                        var columnStringArray = stringArray[i].Split('_');
                        addColumns.Add(int.Parse(columnStringArray[0]), columnStringArray[1]);
                    }
                }
                return addColumns;
            }
        }

        private static string GetElementValue(string formName, string key) {
            var root = Document.SelectSingleNode("configuration");
            var nodes = root.ChildNodes;
            foreach (XmlElement element in nodes) {
                if (element.GetAttribute("Name") == formName) {
                    var subNodes = element.ChildNodes;
                    foreach (var subNode in subNodes) {
                        if (subNode is XmlElement) {
                            var subElement = subNode as XmlElement;
                            if (subElement.GetAttribute("Name") == key) {
                                return subElement.GetAttribute("Value");
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }
    }

}