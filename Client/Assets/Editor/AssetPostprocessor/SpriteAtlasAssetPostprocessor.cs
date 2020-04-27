using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace LinChunJie.AssetPostprocessor {
    public static class SpriteAtlasAssetPostprocessor {
        private static SpriteAtlas spriteAtlas;

        public static void OnPostprocessSpriteAtlas(string assetPath) {
            spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
            if (!CustomAssetPostprocessor.IsNewCreateFile(assetPath)) {
                return;
            }
            
            var postprocessorFolder = SoAssetPostprocessorFolder.GetSoAssetPostprocessorFolder();
            var guid = postprocessorFolder.Get(PostprocessorAssetType.SpriteAtlas, assetPath);
            if (string.IsNullOrEmpty(guid)) {
                return;
            }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<SoSpritePostprocessor>(path);
            if (so == null) {
                so = SoAssetPostprocessor.GetDefault(PostprocessorAssetType.SpriteAtlas) as SoSpritePostprocessor;
            }

            SetIncludeInBuild();
            SetPackingSettings();
            SetTextureSettings();
            SetPlatformSettings(so);
            EditorUtility.SetDirty(spriteAtlas);
            AssetDatabase.SaveAssets();
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

        static void SetPlatformSettings(SoSpritePostprocessor so) {
            //-- Standalone
            SetPlatformSettings(Helper.PlatformStandalone, so);

            //-- iPhone
            SetPlatformSettings(Helper.PlatformIPhone, so);

            //-- Android
            SetPlatformSettings(Helper.PlatformAndroid, so);
        }

        static void SetPlatformSettings(string platform, SoSpritePostprocessor so) {
            var soPlatformSettings = so.GetPlatformSettings(platform);
            var platformSettings = spriteAtlas.GetPlatformSettings(platform);
            platformSettings.overridden = true;
            platformSettings.format = (TextureImporterFormat) soPlatformSettings.format;
            platformSettings.maxTextureSize = soPlatformSettings.maxTextureSize;
            if (platform != Helper.PlatformStandalone) {
                platformSettings.compressionQuality = (int) soPlatformSettings.compressionQuality;
            }

            spriteAtlas.SetPlatformSettings(platformSettings);
        }
    }
}