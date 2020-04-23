using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class TextureAssetPostprocessor {
    public static void OnPostprocessTexture(TextureImporter importer) {
        if (importer.assetPath.Contains("Sprite")) {
            importer.textureType = TextureImporterType.Sprite;
        }
        else if (importer.assetPath.Contains("Texture")) {
            importer.textureType = TextureImporterType.Default;
        }

        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.isReadable = false;
        importer.mipmapEnabled = false;
        importer.streamingMipmaps = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        SetPlatformSettings(importer);
        EditorUtility.SetDirty(importer);
        AssetDatabase.SaveAssets();
    }

    static void SetPlatformSettings(TextureImporter importer) {
        SoTextureBaseImporter soImporter = null;
        switch (importer.textureType) {
            case TextureImporterType.Default:
                soImporter = SoTextureImporter.GetSoTextureImporter();
                break;
            case TextureImporterType.Sprite:
                soImporter = SoSpriteImporter.GetSoSpriteImporter();
                break;
        }

        if (soImporter != null) {
            SetPlatformSettings(AssetImporterHelper.PlatformStandalone, importer, soImporter);
            SetPlatformSettings(AssetImporterHelper.PlatformIPhone, importer, soImporter);
            SetPlatformSettings(AssetImporterHelper.PlatformAndroid, importer, soImporter);
        }
    }

    static void SetPlatformSettings(string platform, TextureImporter importer, SoTextureBaseImporter soImporter) {
        var soPlatformSettings = soImporter.GetPlatformSettings(platform);
        var platformSettings = importer.GetPlatformTextureSettings(platform);
        platformSettings.overridden = true;
        platformSettings.format = (TextureImporterFormat) soPlatformSettings.format;
        platformSettings.maxTextureSize = soPlatformSettings.maxTextureSize;
        if (platform != AssetImporterHelper.PlatformStandalone) {
            platformSettings.compressionQuality = (int) soPlatformSettings.compressionQuality;
        }

        importer.SetPlatformTextureSettings(platformSettings);
    }
}