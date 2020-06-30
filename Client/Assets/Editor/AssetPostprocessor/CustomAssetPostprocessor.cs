using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Funny.AssetPostprocessor {
    public class CustomAssetPostprocessor : UnityEditor.AssetPostprocessor {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            for (int i = 0; i < importedAssets.Length; i++) {
                OnPostprocessWithExtension(importedAssets[i]);
            }
        }

        private void OnPostprocessModel(GameObject g) {
            if (IsNewCreateFile(assetImporter.assetPath)) {
                var importer = assetImporter as ModelImporter;
                ModelAssetPostprocessor.OnPostprocessModel(importer);
            }
        }

        private void OnPreprocessTexture() {
            if (IsNewCreateFile(assetImporter.assetPath)) {
                var importer = assetImporter as TextureImporter;
                TextureAssetPostprocessor.OnPreprocessTexture(importer);
            }
        }

        private static void OnPostprocessWithExtension(string assetName) {
            if (assetName.EndsWith(".spriteatlas")) {
                SpriteAtlasAssetPostprocessor.OnPostprocessSpriteAtlas(assetName);
            }
        }

        public static bool IsNewCreateFile(string assetPath) {
            var metaFilePath = StringUtil.Concat(assetPath, ".meta");
            if (!File.Exists(metaFilePath)) {
                return true;
            }

            return false;
        }
    }
}