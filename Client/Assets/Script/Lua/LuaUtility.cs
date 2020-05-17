using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using XLua;

public static class LuaUtility {
    public static readonly LuaEnv luaEnv = new LuaEnv();

    public static void Initialize() {
        luaEnv.AddLoader(CustomLoadLua);
    }

    private static string LoadLua(ref string path) {
        path = "Assets/Examples/LuaInjection/Scripts/LuaDebug.lua.txt";
        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        return textAsset.text;
    }

    private static byte[] CustomLoadLua(ref string path) {
        return Encoding.UTF8.GetBytes(LoadLua(ref path));
    }
}