using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public static class SpriteAtlasAssetPostprocessor {
    private static SpriteAtlas spriteAtlas;

    public static void OnPostprocessSpriteAtlas(string assetName) {
        spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetName);
        if (!CustomAssetPostprocessor.IsNewCreateFile(assetName)) {
            return;
        }
        SetIncludeInBuild();
        SetPackingSettings();
        SetPlatformSettings();
        EditorUtility.SetDirty(spriteAtlas);
        AssetDatabase.Refresh();
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
        SetPlatformSettings(AssetImporterHelper.Platform_Standalone);

        //-- iPhone
        SetPlatformSettings(AssetImporterHelper.Platform_iPhone);

        //-- Android
        SetPlatformSettings(AssetImporterHelper.Platform_Android);
    }

    static void SetPlatformSettings(string platform) {
        var soImporter = SoSpriteAtlasImporter.GetSoSpriteAtlasImporter();
        var soPlatformSettings = soImporter.GetPlatformSettings(platform);
        var platformSettings = spriteAtlas.GetPlatformSettings(platform);
        platformSettings.overridden = true;
        platformSettings.format = (TextureImporterFormat)soPlatformSettings.format;
        platformSettings.maxTextureSize = soPlatformSettings.textureMaxSize;
        if (platform != AssetImporterHelper.Platform_Standalone) {
            platformSettings.compressionQuality = soPlatformSettings.compressionQuality;
        }
        spriteAtlas.SetPlatformSettings(platformSettings);
    }
}