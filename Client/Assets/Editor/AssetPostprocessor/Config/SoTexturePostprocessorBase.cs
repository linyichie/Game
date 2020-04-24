using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using TextureCompressionQuality = UnityEditor.TextureCompressionQuality;

namespace LinChunJie.AssetPostprocessor {
    public class SoTexturePostprocessorBase : ScriptableObject {
        public List<AssetPostprocessorHelper.TexturePlatformSettings> platformSettings = new List<AssetPostprocessorHelper.TexturePlatformSettings>();

        public List<string> folderGuids = new List<string>();

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
        class Styles {
            public readonly string TextureSizeLabel = "Max Texture Size";

            public readonly string FormatLabel = "Format";

            public readonly string CompressionQualityLabel = "Compressor Quality";
        }

        private int selectPlatformIndex = 0;

        private Styles styles;
        private AssetPostprocessorHelper helper;

        private void OnEnable() {
            styles = styles ?? new Styles();
            helper = helper ?? new AssetPostprocessorHelper();
            selectPlatformIndex = Array.FindIndex(AssetPostprocessorHelper.Platforms, (x) => { return x == AssetPostprocessorHelper.SelectPlatform; });
            selectPlatformIndex = selectPlatformIndex == -1 ? 0 : selectPlatformIndex;
        }

        private void OnDisable() {
        }

        public override void OnInspectorGUI() {
            var config = target as SoTexturePostprocessorBase;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            {
                for (int i = 0; i < AssetPostprocessorHelper.Platforms.Length; i++) {
                    if (GUILayout.Toggle(selectPlatformIndex == i, AssetPostprocessorHelper.Platforms[i], EditorStyles.toolbarButton)) {
                        selectPlatformIndex = i;
                        AssetPostprocessorHelper.SelectPlatform = AssetPostprocessorHelper.Platforms[i];
                    }
                }
            }
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            {
                var selectPlatform = AssetPostprocessorHelper.Platforms[selectPlatformIndex];
                var platformSetting = config.GetPlatformSettings(selectPlatform);
                platformSetting.maxTextureSize = EditorGUILayout.IntPopup(styles.TextureSizeLabel, platformSetting.maxTextureSize, helper.TextureSizeOptionLabels, helper.TextureSizeOptions);

                var textureFormatValue = helper.GetFormatValues(platformSetting.platform);
                platformSetting.format = EditorGUILayout.IntPopup(styles.FormatLabel, platformSetting.format, textureFormatValue.FormatStrings, textureFormatValue.FormatValues);

                platformSetting.compressionQuality = (UnityEditor.TextureCompressionQuality) EditorGUILayout.EnumPopup(styles.CompressionQualityLabel, platformSetting.compressionQuality);
            }

            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }

            GUILayout.EndVertical();
        }
    }
}