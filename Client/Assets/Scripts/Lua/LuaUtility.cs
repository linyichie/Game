﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using XLua;

public static class LuaUtility {
    public static readonly LuaEnv luaEnv = new LuaEnv();
    private static float tickTime = 0f;
    private static float tickInterval = 1f;

    public static void Initialize() {
        luaEnv.AddLoader(CustomLoadLua);
    }

    public static void Start() {
        var textAsset = AssetLoad.Load<TextAsset>("Scripts/Game");
        luaEnv.DoString(textAsset.bytes, "Game");
    }

    private static string LoadLua(ref string path) {
        var textAsset = AssetLoad.Load<TextAsset>(path);
        return textAsset.text;
    }

    private static byte[] CustomLoadLua(ref string path) {
        return Encoding.UTF8.GetBytes(LoadLua(ref path));
    }

    public static void Update() {
        tickTime += Time.deltaTime;
        if(tickTime >= tickInterval) {
            tickTime = 0f;
            luaEnv?.Tick();
        }
    }
}