using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Threading;

public static class GenerateConfig {
    private const string Root_Path = "Assets/AddressableAssets/Config";

    [MenuItem("Assets/配置/生成 C# 配置脚本")]
    static void TxtToCSharp() {
        var instanceIDs = Selection.instanceIDs;
        for (int i = 0; i < instanceIDs.Length; i++) {
            var instanceId = instanceIDs[i];
            var path = AssetDatabase.GetAssetPath(instanceId);
            CSharpConfigGen.Generate(new FileInfo(path));
        }
    }

    [MenuItem("Assets/配置/生成 Lua 配置脚本")]
    static void TxtToLua() {
        var instanceIDs = Selection.instanceIDs;
        for (int i = 0; i < instanceIDs.Length; i++) {
            var instanceId = instanceIDs[i];
            var path = AssetDatabase.GetAssetPath(instanceId);
            //LuaConfigGen.Generate(path, "");
        }
    }

    [MenuItem("Assets/配置/生成 C# 配置脚本", true)]
    static bool TxtToCSharpValidateFunc() {
        return IsSelectConfigFile();
    }

    [MenuItem("Assets/配置/生成 Lua 配置脚本", true)]
    static bool TxtToLuaValidateFunc() {
        return IsSelectConfigFile();
    }

    static bool IsSelectConfigFile() {
        var instanceIDs = Selection.instanceIDs;
        if (instanceIDs == null || instanceIDs.Length == 0) {
            return false;
        }

        for (int i = 0; i < instanceIDs.Length; i++) {
            var instanceId = instanceIDs[i];
            var path = AssetDatabase.GetAssetPath(instanceId);
            if (!path.StartsWith(Root_Path) || !path.EndsWith(".txt")) {
                return false;
            }
        }

        return true;
    }
}