using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace LinChunJie.AssetPostprocessor {
    public class AssetListSpriteAtlasItem : AssetListItem {
        public AssetListSpriteAtlasItem(string path, int depth, string displayName) : base(path, depth, displayName) { }

        public override void VerifyImporterSetting(SoAssetPostprocessor so) {
            IsDirty = false;

            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(Path);

            var soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(Helper.PlatformStandalone);
            var texturePlatformSettings = spriteAtlas.GetPlatformSettings(Helper.PlatformStandalone);
            if(!CompareTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings)) {
                IsDirty = true;
            }

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(Helper.PlatformAndroid);
            texturePlatformSettings = spriteAtlas.GetPlatformSettings(Helper.PlatformAndroid);
            if(!CompareTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings)) {
                IsDirty = true;
            }

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(Helper.PlatformIPhone);
            texturePlatformSettings = spriteAtlas.GetPlatformSettings(Helper.PlatformIPhone);
            if(!CompareTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings)) {
                IsDirty = true;
            }
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(Path);

            var soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(Helper.PlatformStandalone);
            var texturePlatformSettings = spriteAtlas.GetPlatformSettings(Helper.PlatformStandalone);
            SetTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings);
            spriteAtlas.SetPlatformSettings(texturePlatformSettings);

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(Helper.PlatformAndroid);
            texturePlatformSettings = spriteAtlas.GetPlatformSettings(Helper.PlatformAndroid);
            SetTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings);
            spriteAtlas.SetPlatformSettings(texturePlatformSettings);

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(Helper.PlatformIPhone);
            texturePlatformSettings = spriteAtlas.GetPlatformSettings(Helper.PlatformIPhone);
            SetTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings);
            spriteAtlas.SetPlatformSettings(texturePlatformSettings);

            EditorUtility.SetDirty(spriteAtlas);
            AssetDatabase.SaveAssets();
            VerifyImporterSetting(so);
        }

        private void SetTexturePlatformSetting(TexturePlatformSettings so, TextureImporterPlatformSettings texturePlatformSettings) {
            texturePlatformSettings.format = (TextureImporterFormat)so.format;
            texturePlatformSettings.maxTextureSize = so.maxTextureSize;
            texturePlatformSettings.compressionQuality = (int)so.compressionQuality;
        }

        private bool CompareTexturePlatformSetting(TexturePlatformSettings so, TextureImporterPlatformSettings texturePlatformSettings) {
            var same = true;
            same &= so.format == (int)texturePlatformSettings.format;
            same &= (int)so.compressionQuality == texturePlatformSettings.compressionQuality;
            same &= so.maxTextureSize == texturePlatformSettings.maxTextureSize;
            return same;
        }
    }
}