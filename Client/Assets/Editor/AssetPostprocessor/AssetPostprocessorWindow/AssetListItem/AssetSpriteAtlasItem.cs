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

            if(!CompareTexturePlatformSetting(Helper.PlatformStandalone, texturePostprocessorBase, spriteAtlas)) {
                IsDirty = true;
            }

            if(!CompareTexturePlatformSetting(Helper.PlatformAndroid, texturePostprocessorBase, spriteAtlas)) {
                IsDirty = true;
            }

            if(!CompareTexturePlatformSetting(Helper.PlatformIPhone, texturePostprocessorBase, spriteAtlas)) {
                IsDirty = true;
            }
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(Path);
            
            SetTexturePlatformSetting(Helper.PlatformStandalone, texturePostprocessorBase, spriteAtlas);
            SetTexturePlatformSetting(Helper.PlatformAndroid, texturePostprocessorBase, spriteAtlas);
            SetTexturePlatformSetting(Helper.PlatformIPhone, texturePostprocessorBase, spriteAtlas);
            
            EditorUtility.SetDirty(spriteAtlas);
        }

        private void SetTexturePlatformSetting(string platform, SoTexturePostprocessorBase texturePostprocessorBase, SpriteAtlas spriteAtlas) {
            var so = texturePostprocessorBase.GetPlatformSettings(platform);
            var texturePlatformSettings = spriteAtlas.GetPlatformSettings(platform);
            texturePlatformSettings.overridden = so.overridden;
            texturePlatformSettings.format = (TextureImporterFormat)so.format;
            texturePlatformSettings.maxTextureSize = so.maxTextureSize;
            texturePlatformSettings.compressionQuality = (int)so.compressionQuality;
            spriteAtlas.SetPlatformSettings(texturePlatformSettings);
        }

        private bool CompareTexturePlatformSetting(string platform, SoTexturePostprocessorBase texturePostprocessorBase, SpriteAtlas spriteAtlas) {
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