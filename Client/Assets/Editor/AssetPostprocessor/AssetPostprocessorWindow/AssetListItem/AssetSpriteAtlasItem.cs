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
            IsChanged = false;

            var texturePostprocessorBase = so as SoSpriteAtlasPostprocessor;
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(Path);

            if(!SpriteAtlasAssetPostprocessor.CompareSettings(spriteAtlas, texturePostprocessorBase)) {
                IsChanged = true;
            }

            IsDirty = false;
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(Path);
            
            SpriteAtlasAssetPostprocessor.SetPlatformSettings(spriteAtlas, texturePostprocessorBase as SoSpriteAtlasPostprocessor);

            EditorUtility.SetDirty(spriteAtlas);
        }
    }
}