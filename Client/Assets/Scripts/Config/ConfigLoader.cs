using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ConfigLoader {
    public static string[] GetConfigRawDatas(string fileName) {
        var content = ReadAllText(fileName);
        var lines = content.Split(new string[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);
        return lines;
    }

    private static string ReadAllText(string fileName) {
        var relativePath = fileName + ".txt";

        var path = string.Empty;
        if (Application.isEditor) {
            path = Path.Combine(TxtExportConfig.ConfigPath, relativePath);
            return File.ReadAllText(path);
        }
        
        path = Path.Combine(Application.persistentDataPath, relativePath);
        if (File.Exists(path)) {
            return File.ReadAllText(path);
        }

        path = Path.Combine(Application.streamingAssetsPath, relativePath);
        if (Application.isMobilePlatform && Application.platform == RuntimePlatform.Android) {
            var www = new WWW(path);
            while (!www.isDone) {
            }
            return www.text;
        }
        return File.ReadAllText(path);
    }
}