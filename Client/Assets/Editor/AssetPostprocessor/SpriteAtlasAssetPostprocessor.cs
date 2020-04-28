using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Funny.AssetPostprocessor {
    public static class SpriteAtlasAssetPostprocessor {
        public static void OnPostprocessSpriteAtlas(string assetPath) {
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
            if(!CustomAssetPostprocessor.IsNewCreateFile(assetPath)) {
                return;
            }

            var postprocessorFolder = SoAssetPostprocessorFolder.GetSoAssetPostprocessorFolder();
            var guid = postprocessorFolder.Get(PostprocessorAssetType.SpriteAtlas, assetPath);
            if(string.IsNullOrEmpty(guid)) {
                return;
            }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<SoSpriteAtlasPostprocessor>(path);
            if(so == null) {
                so = SoAssetPostprocessor.GetDefault(PostprocessorAssetType.SpriteAtlas) as SoSpriteAtlasPostprocessor;
            }

            SetIncludeInBuild(spriteAtlas);
            SetPackingSettings(spriteAtlas);
            SetTextureSettings(spriteAtlas);
            SetPlatformSettings(spriteAtlas, so);
            EditorUtility.SetDirty(spriteAtlas);
            AssetDatabase.SaveAssets();
        }

        public static void SetIncludeInBuild(SpriteAtlas spriteAtlas) {
            spriteAtlas.SetIncludeInBuild(true);
        }

        public static void SetPackingSettings(SpriteAtlas spriteAtlas) {
            var packSettings = spriteAtlas.GetPackingSettings();
            packSettings.enableRotation = false;
            packSettings.enableTightPacking = false;
            packSettings.padding = 2;
            spriteAtlas.SetPackingSettings(packSettings);
        }

        public static void SetTextureSettings(SpriteAtlas spriteAtlas) {
            var textureSettings = spriteAtlas.GetTextureSettings();
            textureSettings.readable = false;
            textureSettings.generateMipMaps = false;
            textureSettings.filterMode = FilterMode.Bilinear;
            spriteAtlas.SetTextureSettings(textureSettings);
        }

        public static void SetPlatformSettings(SpriteAtlas spriteAtlas, SoSpriteAtlasPostprocessor so) {
            SetPlatformSettings(Helper.PlatformStandalone, spriteAtlas, so);
            SetPlatformSettings(Helper.PlatformIPhone, spriteAtlas, so);
            SetPlatformSettings(Helper.PlatformAndroid, spriteAtlas, so);
        }

        static void SetPlatformSettings(string platform, SpriteAtlas spriteAtlas, SoSpriteAtlasPostprocessor so) {
            var soPlatformSettings = so.GetPlatformSettings(platform);
            var platformSettings = spriteAtlas.GetPlatformSettings(platform);
            platformSettings.overridden = soPlatformSettings.overridden;
            platformSettings.format = (TextureImporterFormat)soPlatformSettings.format;
            platformSettings.maxTextureSize = soPlatformSettings.maxTextureSize;
            platformSettings.compressionQuality = (int)soPlatformSettings.compressionQuality;
            spriteAtlas.SetPlatformSettings(platformSettings);
        }

        public static bool CompareSettings(SpriteAtlas spriteAtlas, SoSpriteAtlasPostprocessor so) {
            var same = ComparePlatformSetting(Helper.PlatformStandalone, so, spriteAtlas);
            same &= ComparePlatformSetting(Helper.PlatformAndroid, so, spriteAtlas);
            same &= ComparePlatformSetting(Helper.PlatformIPhone, so, spriteAtlas);
            return same;
        }

        static bool ComparePlatformSetting(string platform, SoSpriteAtlasPostprocessor texturePostprocessorBase, SpriteAtlas spriteAtlas) {
            var so = texturePostprocessorBase.GetPlatformSettings(platform);
            var texturePlatformSettings = spriteAtlas.GetPlatformSettings(platform);
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