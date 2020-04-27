using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class TexturePostprocessorBaseWidget : IAssetPostprocessorWidget {
        private readonly SoTexturePostprocessorBase so;
        private readonly Styles styles;
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
            platformIndex = Array.FindIndex(Helper.Platforms, (x) => x == Helper.SelectPlatform);
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
                for (int i = 0; i < Helper.Platforms.Length; i++) {
                    if (GUILayout.Toggle(platformIndex == i, Helper.Platforms[i], EditorStyles.toolbarButton)) {
                        platformIndex = i;
                        Helper.SelectPlatform = Helper.Platforms[i];
                    }
                }
            }
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            using (new EditorGUI.DisabledScope(!allowEdit)) {
                var selectPlatform = Helper.Platforms[platformIndex];
                var platformSetting = so.GetPlatformSettings(selectPlatform);
                platformSetting.overridden = EditorGUILayout.Toggle(StringUtility.Contact("Override for ", platformSetting.platform), platformSetting.overridden);
                using(new EditorGUI.DisabledScope(!platformSetting.overridden)) {
                    platformSetting.maxTextureSize = EditorGUILayout.IntPopup(styles.TextureSizeLabel, platformSetting.maxTextureSize, Helper.TextureSizeOptionLabels, Helper.TextureSizeOptions);

                    var textureFormatValue = Helper.GetFormatValues(platformSetting.platform);
                    platformSetting.format = EditorGUILayout.IntPopup(styles.FormatLabel, platformSetting.format, textureFormatValue.FormatStrings, textureFormatValue.FormatValues);

                    platformSetting.compressionQuality = (UnityEditor.TextureCompressionQuality) EditorGUILayout.EnumPopup(styles.CompressionQualityLabel, platformSetting.compressionQuality);
                }
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