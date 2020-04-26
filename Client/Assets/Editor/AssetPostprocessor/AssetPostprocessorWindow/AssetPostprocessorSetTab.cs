using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Hardware;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class AssetPostprocessorSetTab {
        private IPostprocessorSetTab postprocessorSetTab;
        private PostprocessorAssetType assetType;
        private string guid = string.Empty;
        public event Action<string> OnChanged;

        public void OnGUI(Rect pos) {
            GUI.Box(pos, string.Empty);
            GUILayout.BeginArea(new Rect(pos.x + 3, pos.y + 3, pos.width - 6, pos.height - 6));
            postprocessorSetTab?.OnGUI(pos);
            GUILayout.EndArea();
        }

        public void SetPostprocessor(PostprocessorAssetType assetType, string guid) {
            if (this.assetType != assetType || this.guid != guid) {
                this.assetType = assetType;
                this.guid = guid;
                switch (assetType) {
                    case PostprocessorAssetType.SpriteAtlas:
                    case PostprocessorAssetType.Sprite:
                    case PostprocessorAssetType.Texture:
                        if (!string.IsNullOrEmpty(guid)) {
                            postprocessorSetTab = new TextureBasePostprocessorTab();
                            postprocessorSetTab.Initialize(guid);
                            postprocessorSetTab.OnChanged += HandleOnChanged;
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

        private void HandleOnChanged() {
            OnChanged?.Invoke(this.guid);
        }
    }
}