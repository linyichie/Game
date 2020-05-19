using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "lua")]
public class LuaScriptImpoter : ScriptedImporter {
    public override void OnImportAsset(AssetImportContext ctx) {
        var text = File.ReadAllText(ctx.assetPath);
        var textAsset = new TextAsset(text);
        ctx.AddObjectToAsset("main obj", textAsset);
        ctx.SetMainObject(textAsset);
    }
}