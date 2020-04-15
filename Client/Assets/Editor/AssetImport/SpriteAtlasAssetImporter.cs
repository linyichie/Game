using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public static class SpriteAtlasAssetImporter {
    private static SpriteAtlas spriteAtlas;

    public static void OnPostprocessSpriteAtlas(string assetName) {
        spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetName);
        SetIncludeInBuild();
        SetPackingSettings();
        SetPlatformSettings();
        EditorUtility.SetDirty(spriteAtlas);
    }

    static void SetIncludeInBuild() {
        spriteAtlas.SetIncludeInBuild(true);
    }

    static void SetPackingSettings() {
        var packSettings = spriteAtlas.GetPackingSettings();
        spriteAtlas.SetPackingSettings(packSettings);
    }

    static void SetTextureSettings() {
        var textureSettings = spriteAtlas.GetTextureSettings();
        spriteAtlas.SetTextureSettings(textureSettings);
    }

    static void SetPlatformSettings() {
        //-- Standalone
        var platformSettings = spriteAtlas.GetPlatformSettings(CustomAssetImport.Platform_Standalone);
        spriteAtlas.SetPlatformSettings(platformSettings);

        //-- iPhone
        platformSettings = spriteAtlas.GetPlatformSettings(CustomAssetImport.Platform_iPhone);
        spriteAtlas.SetPlatformSettings(platformSettings);

        //-- Android
        platformSettings = spriteAtlas.GetPlatformSettings(CustomAssetImport.Platform_Android);
        platformSettings.overridden = true;
        spriteAtlas.SetPlatformSettings(platformSettings);
    }
}