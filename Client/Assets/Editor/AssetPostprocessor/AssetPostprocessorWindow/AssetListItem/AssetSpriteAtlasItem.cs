using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Funny.AssetPostprocessor {
    public sealed class AssetListSpriteAtlasItem : AssetListItem {
        private SpriteAtlas spriteAtlas;
        public AssetListSpriteAtlasItem(string path, int depth, string displayName) : base(path, depth, displayName) { }

        public override void VerifyAssetState(SoAssetPostprocessor so) {
            changeLogic.SetValue(false);

            var texturePostprocessorBase = so as SoSpriteAtlasPostprocessor;
            string message;
            if(!SpriteAtlasAssetPostprocessor.CompareSettings(GetSpriteAltas(), texturePostprocessorBase, out message)) {
                changeLogic.SetValue(true);
                changeLogic.SetMessage(message.TrimStart('\n'));
            }
        }

        public override void VerifyAssetError(SoAssetPostprocessor so) {
            errorLogic.SetValue(false);
        }

        private SpriteAtlas GetSpriteAltas() {
            if(spriteAtlas == null) {
                spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(Path);
            }

            return spriteAtlas;
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            SpriteAtlasAssetPostprocessor.SetPlatformSettings(GetSpriteAltas(), texturePostprocessorBase as SoSpriteAtlasPostprocessor);
            EditorUtility.SetDirty(GetSpriteAltas());
        }
    }
}