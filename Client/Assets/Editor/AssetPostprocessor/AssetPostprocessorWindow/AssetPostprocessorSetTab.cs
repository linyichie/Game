using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class AssetPostprocessorSetTab {
        private IPostprocessorSetTab postprocessorSetTab;
        private AssetPostprocessorHelper.PostprocessorAssetType assetType;
        private string soPostprocessorGuid = string.Empty;

        public void OnGUI(Rect pos) {
            GUI.Box(pos, string.Empty);
            GUILayout.BeginArea(new Rect(pos.x + 3, pos.y + 3, pos.width - 6, pos.height - 6));
            postprocessorSetTab?.OnGUI(pos);
            GUILayout.EndArea();
        }

        public void SetPostprocessor(AssetPostprocessorHelper.PostprocessorAssetType assetType, string guid) {
            if (this.assetType != assetType || soPostprocessorGuid != guid) {
                this.assetType = assetType;
                this.soPostprocessorGuid = guid;
                switch (assetType) {
                    case AssetPostprocessorHelper.PostprocessorAssetType.SpriteAtlas:
                    case AssetPostprocessorHelper.PostprocessorAssetType.Sprite:
                    case AssetPostprocessorHelper.PostprocessorAssetType.Texture:
                        if (!string.IsNullOrEmpty(guid)) {
                            postprocessorSetTab = new TextureBasePostprocessorTab();
                            postprocessorSetTab.Initialize(guid);
                        } else {
                            postprocessorSetTab = null;
                        }
                        break;
                    default:
                        postprocessorSetTab = null;
                        break;
                }
            }
        }
    }
}