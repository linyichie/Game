using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Funny.AssetPostprocessor {
    public static class TextureAssetPostprocessor {
        public static void OnPreprocessTexture(TextureImporter importer) {
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
            
            var path = AssetDatabase.GUIDToAssetPath(guid);
            SoTexturePostprocessorBase soPostprocessor = null;
            if(!string.IsNullOrEmpty(path)) {
                soPostprocessor = AssetDatabase.LoadAssetAtPath<SoTexturePostprocessorBase>(path);
            }

            if(soPostprocessor == null) {
                soPostprocessor = SoAssetPostprocessor.GetDefault(assetType) as SoTexturePostprocessorBase;
            }

            importer.textureType = assetType == PostprocessorAssetType.Sprite ? TextureImporterType.Sprite : TextureImporterType.Default;
            SetDefaultSettings(importer);
            SetPlatformSettings(importer, soPostprocessor);
        }

        public static void SetDefaultSettings(TextureImporter importer) {
            importer.alphaIsTransparency = importer.DoesSourceTextureHaveAlpha();
            importer.sRGBTexture = true;
            importer.isReadable = false;
            importer.mipmapEnabled = false;
            importer.streamingMipmaps = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
        }

        public static void SetPlatformSettings(TextureImporter importer, SoTexturePostprocessorBase so) {
            SetPlatformSettings(Helper.PlatformStandalone, importer, so);
            SetPlatformSettings(Helper.PlatformIPhone, importer, so);
            SetPlatformSettings(Helper.PlatformAndroid, importer, so);
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

        public static bool CompareSettings(TextureImporter importer, SoTexturePostprocessorBase soPostprocessor) {
            var same = ComparePlatformSetting(Helper.PlatformStandalone, soPostprocessor, importer);
            same &= ComparePlatformSetting(Helper.PlatformAndroid, soPostprocessor, importer);
            same &= ComparePlatformSetting(Helper.PlatformIPhone, soPostprocessor, importer);
            return same;
        }
        
        static bool ComparePlatformSetting(string platform, SoTexturePostprocessorBase texturePostprocessorBase, TextureImporter importer) {
            var so = texturePostprocessorBase.GetPlatformSettings(platform);
            var texturePlatformSettings = importer.GetPlatformTextureSettings(platform);
            var same = true;
            same &= so.overridden == texturePlatformSettings.overridden;
            if(so.overridden && texturePlatformSettings.overridden) {
                same &= so.format == (int)texturePlatformSettings.format;
                same &= (int)so.compressionQuality == texturePlatformSettings.compressionQuality;
                same &= so.maxTextureSize == texturePlatformSettings.maxTextureSize;
            }

            return same;
        }
    }
}