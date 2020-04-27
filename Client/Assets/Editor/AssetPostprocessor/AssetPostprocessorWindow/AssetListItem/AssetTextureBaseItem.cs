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

            var soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(Helper.PlatformStandalone);
            var texturePlatformSettings = importer.GetPlatformTextureSettings(Helper.PlatformStandalone);
            if(!CompareTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings)) {
                IsDirty = true;
            }

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(Helper.PlatformAndroid);
            texturePlatformSettings = importer.GetPlatformTextureSettings(Helper.PlatformAndroid);
            if(!CompareTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings)) {
                IsDirty = true;
            }

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(Helper.PlatformIPhone);
            texturePlatformSettings = importer.GetPlatformTextureSettings(Helper.PlatformIPhone);
            if(!CompareTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings)) {
                IsDirty = true;
            }
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var importer = AssetImporter.GetAtPath(Path) as TextureImporter;

            var soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(Helper.PlatformStandalone);
            var texturePlatformSettings = importer.GetPlatformTextureSettings(Helper.PlatformStandalone);
            SetTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings);
            importer.SetPlatformTextureSettings(texturePlatformSettings);

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(Helper.PlatformAndroid);
            texturePlatformSettings = importer.GetPlatformTextureSettings(Helper.PlatformAndroid);
            SetTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings);
            importer.SetPlatformTextureSettings(texturePlatformSettings);

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(Helper.PlatformIPhone);
            texturePlatformSettings = importer.GetPlatformTextureSettings(Helper.PlatformIPhone);
            SetTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings);
            importer.SetPlatformTextureSettings(texturePlatformSettings);

            importer.SaveAndReimport();
            // -- Todo 更新对应 Inspector 显示
            VerifyImporterSetting(so);
        }

        private void SetTexturePlatformSetting(TexturePlatformSettings so, TextureImporterPlatformSettings texturePlatformSettings) {
            texturePlatformSettings.format = (TextureImporterFormat)so.format;
            texturePlatformSettings.maxTextureSize = so.maxTextureSize;
            texturePlatformSettings.compressionQuality = (int)so.compressionQuality;
        }

        private bool CompareTexturePlatformSetting(TexturePlatformSettings so, TextureImporterPlatformSettings texturePlatformSettings) {
            if((int)so.format != (int)texturePlatformSettings.format || (int)so.compressionQuality != texturePlatformSettings.compressionQuality || so.maxTextureSize != texturePlatformSettings.maxTextureSize) {
                return false;
            }

            return true;
        }
    }
    
    
    public class AssetSpriteItem : AssetTextureBaseItem {
        public AssetSpriteItem(string path, int depth, string displayName) : base(path, depth, displayName) { }
    }

    public class AssetTextureItem : AssetTextureBaseItem {
        public AssetTextureItem(string path, int depth, string displayName) : base(path, depth, displayName) { }
    }
}