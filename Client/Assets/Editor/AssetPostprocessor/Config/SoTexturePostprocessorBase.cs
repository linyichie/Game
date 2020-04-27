using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using TextureCompressionQuality = UnityEditor.TextureCompressionQuality;

namespace LinChunJie.AssetPostprocessor {
    public class SoTexturePostprocessorBase : SoAssetPostprocessor {
        public List<AssetPostprocessorHelper.TexturePlatformSettings> platformSettings = new List<AssetPostprocessorHelper.TexturePlatformSettings>();

        private void OnEnable() {
            if (platformSettings.Count != AssetPostprocessorHelper.Platforms.Length) {
                for (int i = 0; i < AssetPostprocessorHelper.Platforms.Length; i++) {
                    GetPlatformSettings(AssetPostprocessorHelper.Platforms[i]);
                }

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        public AssetPostprocessorHelper.TexturePlatformSettings GetPlatformSettings(string platform) {
            var index = platformSettings.FindIndex((x) => { return x.platform == platform; });
            if (index == -1) {
                var platformSetting = new AssetPostprocessorHelper.TexturePlatformSettings() {
                    platform = platform,
                    format = AssetPostprocessorHelper.GetDefaultTextureFormat(platform),
                };
                platformSettings.Add(platformSetting);
                return platformSetting;
            }

            return platformSettings[index];
        }
    }

    [CustomEditor(typeof(SoTexturePostprocessorBase))]
    public class SoTexturePostprocessorBaseInspector : Editor {
        private IAssetPostprocessorWidget postprocessorWidget;

        private void OnEnable() {
            postprocessorWidget = postprocessorWidget ?? new TexturePostprocessorBaseWidget(this.target as SoTexturePostprocessorBase, false);
        }

        public override void OnInspectorGUI() {
            postprocessorWidget.OnGUI();
        }
    }
}