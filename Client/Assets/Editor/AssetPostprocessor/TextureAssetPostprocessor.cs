using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public static class TextureAssetPostprocessor {
        public static void OnPostprocessTexture(TextureImporter importer) {
            if (importer.assetPath.Contains("UI/Sprite")) {
                importer.textureType = TextureImporterType.Sprite;
            } else if (importer.assetPath.Contains("UI/Texture")) {
                importer.textureType = TextureImporterType.Default;
            } else {
                return;
            }

            var haveAlphaChannel = importer.DoesSourceTextureHaveAlpha();
            importer.sRGBTexture = true;
            importer.alphaSource = haveAlphaChannel ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
            importer.alphaIsTransparency = haveAlphaChannel;
            importer.isReadable = false;
            importer.mipmapEnabled = false;
            importer.streamingMipmaps = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            SetPlatformSettings(importer);
            
            Debug.Log("OnPostprocessTexture : " + importer.assetPath);
        }

        static void SetPlatformSettings(TextureImporter importer) {
            SoTexturePostprocessorBase soPostprocessor = null;
            switch (importer.textureType) {
                case TextureImporterType.Default:
                    soPostprocessor = SoTexturePostprocessor.GetDefaultSoPostprocessor();
                    break;
                case TextureImporterType.Sprite:
                    soPostprocessor = SoSpritePostprocessor.GetDefaultSoPostprocessor();
                    break;
            }

            if (soPostprocessor != null) {
                SetPlatformSettings(AssetPostprocessorHelper.PlatformStandalone, importer, soPostprocessor);
                SetPlatformSettings(AssetPostprocessorHelper.PlatformIPhone, importer, soPostprocessor);
                SetPlatformSettings(AssetPostprocessorHelper.PlatformAndroid, importer, soPostprocessor);
            }
        }

        static void SetPlatformSettings(string platform, TextureImporter importer, SoTexturePostprocessorBase soPostprocessor) {
            var soPlatformSettings = soPostprocessor.GetPlatformSettings(platform);
            var platformSettings = importer.GetPlatformTextureSettings(platform);
            platformSettings.overridden = true;
            platformSettings.format = (TextureImporterFormat) soPlatformSettings.format;
            platformSettings.maxTextureSize = soPlatformSettings.maxTextureSize;
            if (platform != AssetPostprocessorHelper.PlatformStandalone) {
                platformSettings.compressionQuality = (int) soPlatformSettings.compressionQuality;
            }

            importer.SetPlatformTextureSettings(platformSettings);
        }
    }
}