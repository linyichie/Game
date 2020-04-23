using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

public class SoSpriteImporter : SoTextureBaseImporter {

    [MenuItem("Tools/资源导入规范/Sprite")]
    static void Create() {
        var so = AssetDatabase.LoadAssetAtPath<SoSpriteImporter>(SoPath.Instance["SpriteImporter"]);
        if (so == null) {
            so = ScriptableObject.CreateInstance<SoSpriteImporter>();
            AssetDatabase.CreateAsset(so, SoPath.Instance["SpriteImporter"]);
            AssetDatabase.Refresh();
            Selection.activeObject = so;
        }
    }

    public static SoSpriteImporter GetSoSpriteImporter() {
        var so = AssetDatabase.LoadAssetAtPath<SoSpriteImporter>(SoPath.Instance["SpriteImporter"]);
        return so;
    }
}

[CustomEditor(typeof(SoSpriteImporter))]
public class SoSpriteImpoterInspector : SoTextureBaseImporterInspector {
}