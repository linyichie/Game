using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using TextureCompressionQuality = UnityEditor.TextureCompressionQuality;

namespace LinChunJie.AssetPostprocessor {
    public class SoTexturePostprocessorBase : SoAssetPostprocessor {
        public List<TexturePlatformSettings> platformSettings = new List<TexturePlatformSettings>();

        private void OnEnable() {
            if (platformSettings.Count != Helper.Platforms.Length) {
                for (int i = 0; i < Helper.Platforms.Length; i++) {
                    GetPlatformSettings(Helper.Platforms[i]);
                }

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        public TexturePlatformSettings GetPlatformSettings(string platform) {
            var index = platformSettings.FindIndex((x) => { return x.platform == platform; });
            if (index == -1) {
                var platformSetting = new TexturePlatformSettings() {
                    platform = platform,
                    format = Helper.GetDefaultTextureFormat(platform),
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