using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class TextureBasePostprocessorTab : IPostprocessorSetTab {
        private int selectPlatformIndex = 0;
        private readonly Styles styles;
        private readonly AssetPostprocessorHelper helper;
        private SoTexturePostprocessorBase soTexturePostprocessorBase;
        public event Action OnChanged;
        
        class Styles {
            public readonly string TextureSizeLabel = "Max Texture Size";

            public readonly string FormatLabel = "Format";

            public readonly string CompressionQualityLabel = "Compressor Quality";
        }

        public TextureBasePostprocessorTab() {
            styles = styles ?? new Styles();
            helper = helper ?? new AssetPostprocessorHelper();
            selectPlatformIndex = Array.FindIndex(AssetPostprocessorHelper.Platforms, (x) => x == AssetPostprocessorHelper.SelectPlatform);
            selectPlatformIndex = selectPlatformIndex == -1 ? 0 : selectPlatformIndex;
        }

        public void Initialize(string guid) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            soTexturePostprocessorBase = AssetDatabase.LoadAssetAtPath<SoTexturePostprocessorBase>(path);
        }

        public void OnGUI(Rect pos) {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            {
                for(int i = 0; i < AssetPostprocessorHelper.Platforms.Length; i++) {
                    if(GUILayout.Toggle(selectPlatformIndex == i, AssetPostprocessorHelper.Platforms[i], EditorStyles.toolbarButton)) {
                        selectPlatformIndex = i;
                        AssetPostprocessorHelper.SelectPlatform = AssetPostprocessorHelper.Platforms[i];
                    }
                }
            }
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            {
                var selectPlatform = AssetPostprocessorHelper.Platforms[selectPlatformIndex];
                var platformSetting = soTexturePostprocessorBase.GetPlatformSettings(selectPlatform);
                platformSetting.maxTextureSize = EditorGUILayout.IntPopup(styles.TextureSizeLabel, platformSetting.maxTextureSize, helper.TextureSizeOptionLabels, helper.TextureSizeOptions);

                var textureFormatValue = helper.GetFormatValues(platformSetting.platform);
                platformSetting.format = EditorGUILayout.IntPopup(styles.FormatLabel, platformSetting.format, textureFormatValue.FormatStrings, textureFormatValue.FormatValues);

                platformSetting.compressionQuality = (UnityEditor.TextureCompressionQuality)EditorGUILayout.EnumPopup(styles.CompressionQualityLabel, platformSetting.compressionQuality);
            }

            if(EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(soTexturePostprocessorBase);
                AssetDatabase.SaveAssets();
                OnChanged?.Invoke();
            }

            GUILayout.EndVertical();
        }
    }
}