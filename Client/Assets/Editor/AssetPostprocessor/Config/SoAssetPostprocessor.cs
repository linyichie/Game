using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class SoAssetPostprocessor : ScriptableObject {
        [SerializeField] protected bool isDefault = false;
        public static SoAssetPostprocessor Create(AssetPostprocessorHelper.PostprocessorAssetType assetType) {
            switch(assetType) {
                case AssetPostprocessorHelper.PostprocessorAssetType.SpriteAtlas:
                    return SoSpriteAtlasPostprocessor.Create();
                case AssetPostprocessorHelper.PostprocessorAssetType.Sprite:
                    return SoSpritePostprocessor.Create();
                case AssetPostprocessorHelper.PostprocessorAssetType.Texture:
                    return SoTexturePostprocessor.Create();
                case AssetPostprocessorHelper.PostprocessorAssetType.Model:
                    break;
            }

            return null;
        }
    }
}