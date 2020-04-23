using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

public class SoTextureImporter : SoTextureBaseImporter {

    [MenuItem("Tools/资源导入规范/Texture")]
    static void Create() {
        var so = AssetDatabase.LoadAssetAtPath<SoTextureImporter>(SoPath.Instance["TextureImporter"]);
        if (so == null) {
            so = ScriptableObject.CreateInstance<SoTextureImporter>();
            AssetDatabase.CreateAsset(so, SoPath.Instance["TextureImporter"]);
            AssetDatabase.Refresh();
            Selection.activeObject = so;
        }
    }

    public static SoTextureImporter GetSoTextureImporter() {
        var so = AssetDatabase.LoadAssetAtPath<SoTextureImporter>(SoPath.Instance["TextureImporter"]);
        return so;
    }
}

[CustomEditor(typeof(SoTextureImporter))]
public class SoTextureImporterInspector : SoTextureBaseImporterInspector {
}