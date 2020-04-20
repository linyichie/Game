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
        if (!CustomAssetImport.IsNewCreateFile(assetName)) {
            return;
        }
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
        packSettings.enableRotation = false;
        packSettings.enableTightPacking = false;
        packSettings.padding = 2;
        spriteAtlas.SetPackingSettings(packSettings);
    }

    static void SetTextureSettings() {
        var textureSettings = spriteAtlas.GetTextureSettings();
        textureSettings.readable = false;
        textureSettings.generateMipMaps = false;
        textureSettings.filterMode = FilterMode.Bilinear;
        spriteAtlas.SetTextureSettings(textureSettings);
    }

    static void SetPlatformSettings() {
        //-- Standalone
        var platformSettings = spriteAtlas.GetPlatformSettings(CustomAssetImport.Platform_Standalone);
        platformSettings.overridden = true;
        platformSettings.format = TextureImporterFormat.RGBA32;
        spriteAtlas.SetPlatformSettings(platformSettings);

        //-- iPhone
        platformSettings = spriteAtlas.GetPlatformSettings(CustomAssetImport.Platform_iPhone);
        platformSettings.overridden = true;
        platformSettings.format = TextureImporterFormat.ASTC_6x6;
        platformSettings.maxTextureSize = 2048;
        platformSettings.compressionQuality = 50;
        spriteAtlas.SetPlatformSettings(platformSettings);

        //-- Android
        platformSettings = spriteAtlas.GetPlatformSettings(CustomAssetImport.Platform_Android);
        platformSettings.overridden = true;
        platformSettings.format = TextureImporterFormat.ETC2_RGBA8;
        platformSettings.maxTextureSize = 2048;
        platformSettings.compressionQuality = 50;
        spriteAtlas.SetPlatformSettings(platformSettings);
    }

    static bool IsNewCreate(SpriteAtlas spriteAtlas) {
        var platformSettings = spriteAtlas.GetPlatformSettings(CustomAssetImport.Platform_iPhone);
        return platformSettings.format == TextureImporterFormat.Automatic;
    }
}