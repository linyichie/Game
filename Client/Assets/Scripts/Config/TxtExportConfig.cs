using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Txt 文本配置/生成配置")]
public class TxtExportConfig : ScriptableObject {
    [SerializeField] private string configPath = string.Empty;
    [SerializeField] private string cSharpConfigTemplatePath = string.Empty;
    [SerializeField] private string luaConfigTemplatePath = string.Empty;
    [SerializeField] private string cSharpConfigClassPath = string.Empty;
    [SerializeField] private string luaConfigClassPath = string.Empty;

    private static string Guid {
        get {
            if (!EditorPrefs.HasKey("SoConfigGenerateGUID")) {
                return string.Empty;
            }
            var guid = EditorPrefs.GetString("SoConfigGenerateGUID");
            return guid;
        }
        set {
            EditorPrefs.SetString("SoConfigGenerateGUID", value);
        }
    }

    private static TxtExportConfig Instance {
        get {
            TxtExportConfig so = null;
            if (!string.IsNullOrEmpty(Guid)) {
                var path = AssetDatabase.GUIDToAssetPath(Guid);
                so = AssetDatabase.LoadAssetAtPath<TxtExportConfig>(path);
            }
            if (so == null) {
                var path = "Assets/ConfigTool/TxtExportConfig.asset";
                so = AssetDatabase.LoadAssetAtPath<TxtExportConfig>(path);
                if (so == null) {
                    throw new Exception($"配置丢失：{path}");
                }
                Guid = AssetDatabase.AssetPathToGUID(path);
            }
            return so;
        }
    }

    public static string ConfigPath {
        get {
            return Instance.configPath;
        }
    }

    public static string CSharpConfigTemplatePath {
        get {
            return Instance.cSharpConfigTemplatePath;
        }
    }

    public static string LuaConfigTemplatePath {
        get {
            return Instance.luaConfigTemplatePath;
        }
    }
    
    public static string CSharpConfigClassPath {
        get {
            return Instance.cSharpConfigClassPath;
        }
    }

    public static string LuaConfigClassPath {
        get {
            return Instance.luaConfigClassPath;
        }
    }
}