using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Threading;

public static class GenerateConfig {
    [MenuItem("Assets/Txt 文本配置/生成 C# 配置脚本")]
    static void GenerateCSharpConfigClass() {
        var instanceIDs = Selection.instanceIDs;
        for (int i = 0; i < instanceIDs.Length; i++) {
            var instanceId = instanceIDs[i];
            var path = AssetDatabase.GetAssetPath(instanceId);
            if (path.ToLower().EndsWith(".txt")) {
                CSharpConfigGen.Generate(new FileInfo(path));
            }
        }
    }

    [MenuItem("Assets/Txt 文本配置/生成 Lua 配置脚本")]
    static void GenerateLuaConfigClass() {
        var instanceIDs = Selection.instanceIDs;
        for (int i = 0; i < instanceIDs.Length; i++) {
            var instanceId = instanceIDs[i];
            var path = AssetDatabase.GetAssetPath(instanceId);
            if (path.ToLower().EndsWith(".txt")) {
                LuaConfigGen.Generate(new FileInfo(path));
            }
        }
    }
}