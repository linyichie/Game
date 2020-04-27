using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public static class TextureAssetPostprocessor {
        public static void OnPostprocessTexture(TextureImporter importer) {
            var assetType = (PostprocessorAssetType)(-1);
            if(importer.assetPath.Contains("Sprite")) {
                assetType = PostprocessorAssetType.Sprite;
            } else if(importer.assetPath.Contains("Texture")) {
                assetType = PostprocessorAssetType.Texture;
            }

            if(assetType != PostprocessorAssetType.Sprite && assetType != PostprocessorAssetType.Texture) {
                return;
            }

            var postprocessorFolder = SoAssetPostprocessorFolder.GetSoAssetPostprocessorFolder();
            var guid = postprocessorFolder.Get(assetType, importer.assetPath);
            if(string.IsNullOrEmpty(guid)) {
                return;
            }

            importer.textureType = assetType == PostprocessorAssetType.Sprite ? TextureImporterType.Sprite : TextureImporterType.Default;
            importer.alphaIsTransparency = importer.DoesSourceTextureHaveAlpha();
            importer.sRGBTexture = true;
            importer.isReadable = false;
            importer.mipmapEnabled = false;
            importer.streamingMipmaps = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            SetPlatformSettings(importer, assetType, guid);
            Reimport(importer);
        }

        static async Task Reimport(TextureImporter importer) {
            await Task.Delay(1);
            importer.SaveAndReimport();
        }

        static void SetPlatformSettings(TextureImporter importer, PostprocessorAssetType assetType, string guid) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            SoTexturePostprocessorBase soPostprocessor = null;
            if(!string.IsNullOrEmpty(path)) {
                soPostprocessor = AssetDatabase.LoadAssetAtPath<SoTexturePostprocessorBase>(path);
            }

            if(soPostprocessor == null) {
                soPostprocessor = SoAssetPostprocessor.GetDefault(assetType) as SoTexturePostprocessorBase;
            }

            if(soPostprocessor != null) {
                SetPlatformSettings(Helper.PlatformStandalone, importer, soPostprocessor);
                SetPlatformSettings(Helper.PlatformIPhone, importer, soPostprocessor);
                SetPlatformSettings(Helper.PlatformAndroid, importer, soPostprocessor);
            }
        }

        static void SetPlatformSettings(string platform, TextureImporter importer, SoTexturePostprocessorBase soPostprocessor) {
            var soPlatformSettings = soPostprocessor.GetPlatformSettings(platform);
            var platformSettings = importer.GetPlatformTextureSettings(platform);
            platformSettings.overridden = soPlatformSettings.overridden;
            if(soPlatformSettings.overridden) {
                platformSettings.format = (TextureImporterFormat)soPlatformSettings.format;
                platformSettings.maxTextureSize = soPlatformSettings.maxTextureSize;
                platformSettings.compressionQuality = (int)soPlatformSettings.compressionQuality;
            }

            importer.SetPlatformTextureSettings(platformSettings);
        }
    }
}