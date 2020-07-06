using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class CSharpConfigGen {
    private const string retract1 = "\t";
    private const string retract2 = "\t\t";
    private const string retract3 = "\t\t\t";
    private const string retract4 = "\t\t\t\t";
    private const string retract5 = "\t\t\t\t\t";
    
    private static string fieldContent = string.Empty;
    private static string readContent = string.Empty;

    public static void Generate(FileInfo fileInfo) {
        var lines = File.ReadAllLines(fileInfo.FullName);
        if (lines.Length > 2) {
            var typeLine = lines[0];
            var fieldLine = lines[1];
            var types = typeLine.Split('\t');
            var fields = fieldLine.Split('\t');
            var min = Mathf.Min(types.Length, fields.Length);
            var fieldLines = new List<string>();
            var readLines = new List<string>();

            int index = 0;
            for (int j = 0; j < min; j++) {
                var type = types[j];
                var field = fields[j];
                var fieldstring = GetFieldLine(type, field);
                var readString = GetReadLine(type, field, index);
                if (!string.IsNullOrEmpty(fieldstring)) {
                    fieldLines.Add(fieldstring);
                }

                if (!string.IsNullOrEmpty(readString)) {
                    index++;
                    readLines.Add(readString);
                }
            }

            fieldContent = string.Join("\r\n\t", fieldLines.ToArray());
            readContent = string.Join("\r\n\t\t\t", readLines.ToArray());
            GenerateConfig(fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.')));
        }
    }

    private static string GetFieldLine(string field, string type) {
        field = field.Replace(" ", "");
        if (type.Contains("int[]")) {
            return StringUtil.Concat("public readonly int[] ", field.Trim(), ";");
        }
        if (type.Contains("float[]")) {
            return StringUtil.Concat("public readonly float[] ", field.Trim(), ";");
        }
        if (type.Contains("string[]")) {
            return StringUtil.Concat("public readonly string[] ", field.Trim(), ";");
        }
        if (type.Contains("Vector3[]")) {
            return StringUtil.Concat("public readonly Vector3[] ", field.Trim(), ";");
        }
        if (type.Contains("int")) {
            return StringUtil.Concat("public readonly int ", field.Trim(), ";");
        }
        if (type.Contains("float")) {
            return StringUtil.Concat("public readonly float ", field.Trim(), ";");
        }
        if (type.Contains("string")) {
            return StringUtil.Concat("public readonly string ", field, ";");
        }
        if (type.Contains("Vector3")) {
            return StringUtil.Concat("public readonly Vector3 ", field.Trim(), ";");
        }
        if (type.Contains("bool")) {
            return StringUtil.Concat("public readonly bool ", field.Trim(), ";");
        }
        return string.Empty;
    }

    static string GetReadLine(string field, string type, int index) {
        field = field.Replace(" ", "");
        if (type.Contains("int[]")) {
            var line1 = StringUtil.Concat("string[] ", field, "StringArray", " = ", "tables", "[", index, "]", ".Trim().Split(StringUtil.splitSeparator,StringSplitOptions.RemoveEmptyEntries);", "\n");
            var line2 = StringUtil.Concat(retract3, field, " = ", "new int", "[", field, "StringArray.Length]", ";", "\n");
            var line3 = StringUtil.Concat(retract3, "for (int i = 0; i <", field, "StringArray", ".Length", ";", "i++", ")", "\n");
            var line4 = StringUtil.Concat(retract3, "{\n");
            var line5 = StringUtil.Concat(retract4, " int.TryParse(", field, "StringArray", "[i]", ",", "out ", field, "[i]", ")", ";", "\n");
            var line6 = StringUtil.Concat(retract3, "}");

            return StringUtil.Concat(line1, line2, line3, line4, line5, line6);
        }
        if (type.Contains("float[]")) {
            var line1 = StringUtil.Concat("string[] ", field, "StringArray", " = ", "tables", "[", index, "]", ".Trim().Split(StringUtil.splitSeparator,StringSplitOptions.RemoveEmptyEntries);", "\n");
            var line2 = StringUtil.Concat(retract3, field, " = ", "new float", "[", field, "StringArray.Length", "]", ";", "\n");
            var line3 = StringUtil.Concat(retract3, "for (int i = 0; i <", field, "StringArray", ".Length", ";", "i++", ")", "\n");
            var line4 = StringUtil.Concat(retract3, "{\n");
            var line5 = StringUtil.Concat(retract4, " float.TryParse(", field, "StringArray", "[i]", ",", "out ", field, "[i]", ")", ";", "\n");
            var line6 = StringUtil.Concat(retract3, "}");

            return StringUtil.Concat(line1, line2, line3, line4, line5, line6);
        }
        if (type.Contains("string[]")) {
            var line1 = StringUtil.Concat(field, " = ", "tables", "[", index, "]", ".Trim().Split(StringUtil.splitSeparator,StringSplitOptions.RemoveEmptyEntries);");
            return line1;
        }
        if (type.Contains("Vector3[]")) {
            var line1 = StringUtil.Concat("string[] ", field, "StringArray", " = ", "tables", "[", index, "]", ".Trim().Split(StringUtil.splitSeparator,StringSplitOptions.RemoveEmptyEntries);", "\n");
            var line2 = StringUtil.Concat(retract3, field, " = ", "new Vector3", "[", field, "StringArray.Length", "]", ";", "\n");
            var line3 = StringUtil.Concat(retract3, "for (int i = 0; i <", field, "StringArray", ".Length", ";", "i++", ")", "\n");
            var line4 = StringUtil.Concat(retract3, "{\n");
            var line5 = StringUtil.Concat(retract4, field, "[i]", "=", field, "StringArray", "[i]", ".Vector3Parse()", ";", "\n");
            var line6 = StringUtil.Concat(retract3, "}");

            return StringUtil.Concat(line1, line2, line3, line4, line5, line6);
        }
        if (type.Contains("int")) {
            return StringUtil.Concat("int.TryParse(tables", "[", index, "]", ",", "out ", field, ")", "; ");
        }
        if (type.Contains("float")) {
            return StringUtil.Concat("float.TryParse(tables", "[", index, "]", ",", "out ", field, ")", "; ");
        }
        if (type.Contains("string")) {
            return StringUtil.Concat(field, " = ", "tables", "[", index, "]", ";");
        }
        if (type.Contains("Vector3")) {
            return StringUtil.Concat(field, "=", "tables", "[", index, "]", ".Vector3Parse()", ";");
        }
        if (type.Contains("bool")) {
            var line1 = StringUtil.Concat("var ", field, "Temp", " = 0", ";", "\n");
            var line2 = StringUtil.Concat(retract3, "int.TryParse(tables", "[", index, "]", ",", "out ", field, "Temp", ")", "; ", "\n");
            var line3 = StringUtil.Concat(retract3, field, "=", field, "Temp", "!=0", ";");
            return StringUtil.Concat(line1, line2, line3);
        }
        return string.Empty;
    }

    private  static void GenerateConfig(string name) {
        var newConfigPath = SoConfigGenerate.CSharpConfigClassPath + string.Format("/{0}Config.cs", name);
        AssetDatabase.DeleteAsset(newConfigPath);
        UnityEngine.Object o = CreateScriptAssetFromTemplate(newConfigPath, SoConfigGenerate.CSharpConfigTemplatePath);
        ProjectWindowUtil.ShowCreatedAsset(o);
    }
    
    private static UnityEngine.Object CreateScriptAssetFromTemplate(string pathName, string resourceFile) {
        var fullPath = Path.GetFullPath(pathName);

        var streamReader = new StreamReader(resourceFile);
        var text = streamReader.ReadToEnd();
        streamReader.Close();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
        text = Regex.Replace(text, "#ClassName*#", fileNameWithoutExtension);
        text = Regex.Replace(text, "#DateTime#", System.DateTime.Now.ToLongDateString());
        text = Regex.Replace(text, "#Field#", fieldContent);
        text = Regex.Replace(text, "#Read#", readContent);
        text = Regex.Replace(text, "#FileName#", fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - 6));

        var encoderShouldEmitUTF8Identifier = true;
        var throwOnInvalidBytes = false;
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
        var append = false;
        var streamWriter = new StreamWriter(fullPath, append, encoding);
        streamWriter.Write(text);
        streamWriter.Close();
        AssetDatabase.ImportAsset(pathName);
        return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
    }
}