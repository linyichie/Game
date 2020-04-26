using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class SoAssetPostprocessor : ScriptableObject {
        public static SoAssetPostprocessor Create(PostprocessorAssetType assetType) {
            switch (assetType) {
                case PostprocessorAssetType.SpriteAtlas:
                    return SoSpriteAtlasPostprocessor.Create();
                case PostprocessorAssetType.Sprite:
                    return SoSpritePostprocessor.Create();
                case PostprocessorAssetType.Texture:
                    return SoTexturePostprocessor.Create();
                case PostprocessorAssetType.Model:
                    break;
            }

            return null;
        }

        public static SoAssetPostprocessor GetDefault(PostprocessorAssetType assetType) {
            switch (assetType) {
                case PostprocessorAssetType.SpriteAtlas:
                    return SoSpriteAtlasPostprocessor.GetDefaultSoPostprocessor();
                case PostprocessorAssetType.Sprite:
                    return SoSpritePostprocessor.GetDefaultSoPostprocessor();
                case PostprocessorAssetType.Texture:
                    return SoTexturePostprocessor.GetDefaultSoPostprocessor();
                case PostprocessorAssetType.Model:
                    break;
            }

            return null;
        }
    }
}