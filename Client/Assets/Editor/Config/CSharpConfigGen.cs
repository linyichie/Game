using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CSharpConfigGen {
    const string FileTemplatePath = "Assets/Editor/FileTemplates/ConfigTemplate.txt";

    public static void Generate(FileInfo fileInfo) {
        var lines = File.ReadAllLines(fileInfo.FullName);
        if (lines.Length > 2) {
            var fieldTypeLine = lines[0];
            var fieldNameLine = lines[1];
            var types = fieldTypeLine.Split('\t');
            var fields = fieldNameLine.Split('\t');
            var min = Mathf.Min(types.Length, fields.Length);

            var keyType = types[0].Trim();
            var keyField = fields[0].Trim();

            var fieldLines = new List<string>();
            var parseLines = new List<string>();
            for (int i = 0; i < min; i++) {
                var field = fields[i].Trim();
                var type = types[i].Trim();

                fieldLines.Add(GetFieldLine(field.Trim(), type.Trim()));
                parseLines.Add(GetParseLine(field.Trim(), type.Trim(), i));
            }

            var fieldLabel = string.Join("\r\n\t", fieldLines.ToArray());
            var parseLabel = string.Join("\r\n\r\n\t\t\t", parseLines.ToArray());

            GenerateConfig(new ConfigClassInfo() {
                className = StringUtility.Contact(fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.')), "Config"),
                fileName = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.')),
                keyType = keyType,
                keyField = keyField,
                fieldLabel = fieldLabel,
                parseLabel = parseLabel,
            });
        }
    }

    static string GetFieldLine(string field, string type) {
        if (string.IsNullOrEmpty(field)
            || string.IsNullOrEmpty(type)) {
            return string.Empty;
        }

        switch (type) {
            default:
                return StringUtility.Contact("public ", type, " ", field, " { get; private set; }");
        }
    }

    static string GetParseLine(string field, string type, int index) {
        if (string.IsNullOrEmpty(field)
            || string.IsNullOrEmpty(type)) {
            return string.Empty;
        }

        switch (type) {
            case "string":
                return StringUtility.Contact(field, string.Format(" = values[{0}]", index), ".Trim();");
            case "int":
                return StringUtility.Contact(field, string.Format(" = ConfigParse.ParseInt(values[{0}].Trim())", index),
                    ";");
        }

        return string.Empty;
    }

    static void GenerateConfig(ConfigClassInfo classInfo) {
        var path = StringUtility.Contact("Assets/Script/Game/Config/", classInfo.className, ".cs");
        string fullPath = Path.GetFullPath(path);

        StreamReader sr = new StreamReader(FileTemplatePath);
        var text = sr.ReadToEnd();
        sr.Close();

        text = text.Replace("#ClassName#", classInfo.className);
        text = text.Replace("#FileName#", classInfo.fileName);
        text = text.Replace("#Fileds#", classInfo.fieldLabel);
        text = text.Replace("#Parses#", classInfo.parseLabel);
        text = text.Replace("#KeyType#", classInfo.keyType);
        text = text.Replace("#KeyField#", classInfo.keyField);

        bool encoderShouldEmitUTF8Identifier = true;
        bool throwOnInvalidBytes = false;
        UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
        bool append = false;
        StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
        streamWriter.Write(text);
        streamWriter.Close();
        AssetDatabase.ImportAsset(path);
        ProjectWindowUtil.ShowCreatedAsset(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)));
    }

    public struct ConfigClassInfo {
        public string className;
        public string fileName;
        public string keyType;
        public string keyField;
        public string fieldLabel;
        public string parseLabel;
    }
}