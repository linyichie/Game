using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace LinChunJie.AssetPostprocessor {
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

        static void SetPlatformSettings() {
            //-- Standalone
            SetPlatformSettings(AssetPostprocessorHelper.PlatformStandalone);

            //-- iPhone
            SetPlatformSettings(AssetPostprocessorHelper.PlatformIPhone);

            //-- Android
            SetPlatformSettings(AssetPostprocessorHelper.PlatformAndroid);
        }

        static void SetPlatformSettings(string platform) {
            var soImporter = SoSpriteAtlasPostprocessor.GetDefaultSoPostprocessor();
            var soPlatformSettings = soImporter.GetPlatformSettings(platform);
            var platformSettings = spriteAtlas.GetPlatformSettings(platform);
            platformSettings.overridden = true;
            platformSettings.format = (TextureImporterFormat) soPlatformSettings.format;
            platformSettings.maxTextureSize = soPlatformSettings.maxTextureSize;
            if (platform != AssetPostprocessorHelper.PlatformStandalone) {
                platformSettings.compressionQuality = (int) soPlatformSettings.compressionQuality;
            }

            spriteAtlas.SetPlatformSettings(platformSettings);
        }
    }
}