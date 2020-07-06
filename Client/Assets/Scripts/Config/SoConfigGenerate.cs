using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Txt 文本配置/生成配置")]
public class SoConfigGenerate : ScriptableObject {
    [SerializeField] private string configPath = string.Empty;
    [SerializeField] private string cSharpConfigTemplatePath = string.Empty;
    [SerializeField] private string luaConfigTemplatePath = string.Empty;
    [SerializeField] private string cSharpConfigClassPath = string.Empty;
    [SerializeField] private string luaConfigClassPath = string.Empty;

    private static SoConfigGenerate instance {
        get {
            var path = "Assets/Scripts/Config/SoConfigGenerate.asset";
            var so = AssetDatabase.LoadAssetAtPath<SoConfigGenerate>(path);
            if (so == null) {
                throw new Exception(string.Format("配置丢失：{0}", path));
            }
            return so;
        }
    }

    public static string ConfigPath {
        get {
            return instance.configPath;
        }
    }

    public static string CSharpConfigTemplatePath {
        get {
            return instance.cSharpConfigTemplatePath;
        }
    }

    public static string LuaConfigTemplatePath {
        get {
            return instance.luaConfigTemplatePath;
        }
    }
    
    public static string CSharpConfigClassPath {
        get {
            return instance.cSharpConfigClassPath;
        }
    }

    public static string LuaConfigClassPath {
        get {
            return instance.luaConfigClassPath;
        }
    }
}