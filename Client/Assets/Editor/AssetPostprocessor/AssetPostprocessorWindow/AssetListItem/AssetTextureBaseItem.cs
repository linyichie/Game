using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class AssetTextureBaseItem : AssetListItem {
        protected AssetTextureBaseItem(string path, int depth, string displayName) : base(path, depth, displayName) { }

        public override void VerifyImporterSetting(SoAssetPostprocessor so) {
            IsDirty = false;

            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var importer = AssetImporter.GetAtPath(Path) as TextureImporter;

            if(!CompareTexturePlatformSetting(Helper.PlatformStandalone, texturePostprocessorBase, importer)) {
                IsDirty = true;
            }

            if(!CompareTexturePlatformSetting(Helper.PlatformAndroid, texturePostprocessorBase, importer)) {
                IsDirty = true;
            }

            if(!CompareTexturePlatformSetting(Helper.PlatformIPhone, texturePostprocessorBase, importer)) {
                IsDirty = true;
            }
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var importer = AssetImporter.GetAtPath(Path) as TextureImporter;

            SetTexturePlatformSetting(Helper.PlatformStandalone, texturePostprocessorBase, importer);
            SetTexturePlatformSetting(Helper.PlatformAndroid, texturePostprocessorBase, importer);
            SetTexturePlatformSetting(Helper.PlatformIPhone, texturePostprocessorBase, importer);

            EditorUtility.SetDirty(importer);
        }

        private void SetTexturePlatformSetting(string platform, SoTexturePostprocessorBase texturePostprocessorBase, TextureImporter importer) {
            var so = texturePostprocessorBase.GetPlatformSettings(platform);
            var texturePlatformSettings = importer.GetPlatformTextureSettings(platform);
            texturePlatformSettings.overridden = so.overridden;
            texturePlatformSettings.format = (TextureImporterFormat)so.format;
            texturePlatformSettings.maxTextureSize = so.maxTextureSize;
            texturePlatformSettings.compressionQuality = (int)so.compressionQuality;
            importer.SetPlatformTextureSettings(texturePlatformSettings);
        }

        private bool CompareTexturePlatformSetting(string platform, SoTexturePostprocessorBase texturePostprocessorBase, TextureImporter importer) {
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

    public class AssetSpriteItem : AssetTextureBaseItem {
        public AssetSpriteItem(string path, int depth, string displayName) : base(path, depth, displayName) { }
    }

    public class AssetTextureItem : AssetTextureBaseItem {
        public AssetTextureItem(string path, int depth, string displayName) : base(path, depth, displayName) { }
    }
}