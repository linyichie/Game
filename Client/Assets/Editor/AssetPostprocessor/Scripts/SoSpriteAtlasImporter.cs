using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SoSpriteAtlasImporter : SoTextureBaseImporter {
    [MenuItem("Tools/资源导入规范/SpriteAtlas")]
    static void Create() {
        var so = AssetDatabase.LoadAssetAtPath<SoSpriteAtlasImporter>(SoPath.Instance["SpriteAtlasImporter"]);
        if (so == null) {
            so = ScriptableObject.CreateInstance<SoSpriteAtlasImporter>();
            AssetDatabase.CreateAsset(so, SoPath.Instance["SpriteAtlasImporter"]);
            AssetDatabase.Refresh();
            Selection.activeObject = so;
        }
    }

    public static SoSpriteAtlasImporter GetSoSpriteAtlasImporter() {
        var so = AssetDatabase.LoadAssetAtPath<SoSpriteAtlasImporter>(SoPath.Instance["SpriteAtlasImporter"]);
        return so;
    }
}

[CustomEditor(typeof(SoSpriteAtlasImporter))]
public class SoSpriteAtlasImpoterInspector : SoTextureBaseImporterInspector {
}