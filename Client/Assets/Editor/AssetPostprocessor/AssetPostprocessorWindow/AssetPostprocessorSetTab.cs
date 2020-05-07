using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Hardware;
using UnityEngine;

namespace Funny.AssetPostprocessor {
    public class AssetPostprocessorSetTab {
        private IAssetPostprocessorWidget postprocessorWidget;
        private PostprocessorAssetType assetType;
        private string guid = string.Empty;
        public event Action<string> OnChanged;

        public void OnGUI(Rect pos) {
            GUI.Box(pos, string.Empty);
            GUILayout.BeginArea(new Rect(pos.x + 3, pos.y + 3, pos.width - 6, pos.height - 6));
            postprocessorWidget?.OnGUI();
            GUILayout.EndArea();
        }

        public void SetPostprocessor(PostprocessorAssetType assetType, string guid) {
            if (this.assetType != assetType || this.guid != guid) {
                this.assetType = assetType;
                this.guid = guid;
                postprocessorWidget = null;
                switch (assetType) {
                    case PostprocessorAssetType.SpriteAtlas:
                    case PostprocessorAssetType.Sprite:
                    case PostprocessorAssetType.Texture:
                        postprocessorWidget = new TexturePostprocessorBaseWidget(this.guid, true);
                        break;
                    case PostprocessorAssetType.Model:
                        postprocessorWidget = new ModelPostprocessorWidget(this.guid, true);
                        break;
                }

                if (postprocessorWidget != null) {
                    postprocessorWidget.OnChanged += HandleOnChanged;
                }
            }
        }

        private void HandleOnChanged() {
            OnChanged?.Invoke(this.guid);
        }
    }
}