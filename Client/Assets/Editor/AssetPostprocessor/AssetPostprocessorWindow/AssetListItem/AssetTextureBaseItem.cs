using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class AssetTextureBaseItem : AssetListItem {
        protected AssetTextureBaseItem(string path, int depth, string displayName) : base(path, depth, displayName) { }

        public override void VerifyImporterSetting(SoAssetPostprocessor so) {
            IsChanged = false;

            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var importer = AssetImporter.GetAtPath(Path) as TextureImporter;

            if(!TextureAssetPostprocessor.CompareSettings(importer, texturePostprocessorBase)) {
                IsChanged = true;
            }

            IsDirty = false;
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var importer = AssetImporter.GetAtPath(Path) as TextureImporter;

            TextureAssetPostprocessor.SetPlatformSettings(importer, texturePostprocessorBase);

            EditorUtility.SetDirty(importer);
        }
    }

    public class AssetSpriteItem : AssetTextureBaseItem {
        public AssetSpriteItem(string path, int depth, string displayName) : base(path, depth, displayName) { }
    }

    public class AssetTextureItem : AssetTextureBaseItem {
        public AssetTextureItem(string path, int depth, string displayName) : base(path, depth, displayName) { }
    }
}