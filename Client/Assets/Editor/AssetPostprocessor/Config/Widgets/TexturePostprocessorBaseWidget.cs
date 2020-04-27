using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class TexturePostprocessorBaseWidget : IAssetPostprocessorWidget {
        private readonly SoTexturePostprocessorBase so;
        private readonly Styles styles;
        private readonly AssetPostprocessorHelper helper;
        private readonly bool allowEdit;

        private int platformIndex = 0;

        public event Action OnChanged;

        class Styles {
            public readonly string FormatLabel = "Format";
            public readonly string TextureSizeLabel = "Max Texture Size";
            public readonly string CompressionQualityLabel = "Compressor Quality";
        }

        private TexturePostprocessorBaseWidget() {
            styles = styles ?? new Styles();
            helper = helper ?? new AssetPostprocessorHelper();
            platformIndex = Array.FindIndex(AssetPostprocessorHelper.Platforms, (x) => x == AssetPostprocessorHelper.SelectPlatform);
            platformIndex = platformIndex == -1 ? 0 : platformIndex;
        }

        public TexturePostprocessorBaseWidget(string guid, bool allowEdit) : this() {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            this.so = AssetDatabase.LoadAssetAtPath<SoTexturePostprocessorBase>(path);
            this.allowEdit = allowEdit;
        }

        public TexturePostprocessorBaseWidget(SoTexturePostprocessorBase so, bool allowEdit) : this() {
            this.so = so;
            this.allowEdit = allowEdit;
        }

        public void OnGUI() {
            if (this.so == null) {
                return;
            }
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            {
                for (int i = 0; i < AssetPostprocessorHelper.Platforms.Length; i++) {
                    if (GUILayout.Toggle(platformIndex == i, AssetPostprocessorHelper.Platforms[i], EditorStyles.toolbarButton)) {
                        platformIndex = i;
                        AssetPostprocessorHelper.SelectPlatform = AssetPostprocessorHelper.Platforms[i];
                    }
                }
            }
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            using (new EditorGUI.DisabledScope(!allowEdit)) {
                var selectPlatform = AssetPostprocessorHelper.Platforms[platformIndex];
                var platformSetting = so.GetPlatformSettings(selectPlatform);
                platformSetting.maxTextureSize = EditorGUILayout.IntPopup(styles.TextureSizeLabel, platformSetting.maxTextureSize, helper.TextureSizeOptionLabels, helper.TextureSizeOptions);

                var textureFormatValue = helper.GetFormatValues(platformSetting.platform);
                platformSetting.format = EditorGUILayout.IntPopup(styles.FormatLabel, platformSetting.format, textureFormatValue.FormatStrings, textureFormatValue.FormatValues);

                platformSetting.compressionQuality = (UnityEditor.TextureCompressionQuality) EditorGUILayout.EnumPopup(styles.CompressionQualityLabel, platformSetting.compressionQuality);
            }

            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(so);
                AssetDatabase.SaveAssets();
                OnChanged?.Invoke();
            }

            GUILayout.EndVertical();
        }
    }
}