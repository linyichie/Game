using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using TextureCompressionQuality = UnityEditor.TextureCompressionQuality;

public class SoTextureBaseImporter : ScriptableObject {
    public List<AssetImporterHelper.TexturePlatformSettings> platformSettings =
        new List<AssetImporterHelper.TexturePlatformSettings>();
    
    public List<string> folderGuids = new List<string>();

    private void OnEnable() {
        if (platformSettings.Count != AssetImporterHelper.Platforms.Length) {
            for (int i = 0; i < AssetImporterHelper.Platforms.Length; i++) {
                GetPlatformSettings(AssetImporterHelper.Platforms[i]);
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }

    public AssetImporterHelper.TexturePlatformSettings GetPlatformSettings(string platform) {
        var index = platformSettings.FindIndex((x) => { return x.platform == platform; });
        if (index == -1) {
            var platformSetting = new AssetImporterHelper.TexturePlatformSettings() {
                platform = platform,
                format = AssetImporterHelper.GetDefaultTextureFormat(platform),
            };
            platformSettings.Add(platformSetting);
            return platformSetting;
        }

        return platformSettings[index];
    }
}

[CustomEditor(typeof(SoTextureBaseImporter))]
public class SoTextureBaseImporterInspector : Editor {
    class Styles {
        public readonly string TextureSizeLabel = "Max Texture Size";

        public readonly string FormatLabel = "Format";

        public readonly string CompressionQualityLabel = "Compressor Quality";
    }

    private int selectPlatformIndex = 0;

    private Styles styles;
    private AssetImporterHelper helper;

    private void OnEnable() {
        styles = styles ?? new Styles();
        helper = helper ?? new AssetImporterHelper();
        selectPlatformIndex =
            Array.FindIndex(AssetImporterHelper.Platforms, (x) => { return x == AssetImporterHelper.SelectPlatform; });
        selectPlatformIndex = selectPlatformIndex == -1 ? 0 : selectPlatformIndex;
    }

    private void OnDisable() {
    }

    public override void OnInspectorGUI() {
        var config = target as SoTextureBaseImporter;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        {
            for (int i = 0; i < AssetImporterHelper.Platforms.Length; i++) {
                if (GUILayout.Toggle(selectPlatformIndex == i, AssetImporterHelper.Platforms[i],
                    EditorStyles.toolbarButton)) {
                    selectPlatformIndex = i;
                    AssetImporterHelper.SelectPlatform = AssetImporterHelper.Platforms[i];
                }
            }
        }
        GUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        {
            var selectPlatform = AssetImporterHelper.Platforms[selectPlatformIndex];
            var platformSetting = config.GetPlatformSettings(selectPlatform);
            platformSetting.maxTextureSize = EditorGUILayout.IntPopup(styles.TextureSizeLabel,
                platformSetting.maxTextureSize,
                helper.TextureSizeOptionLabels,
                helper.TextureSizeOptions);

            var textureFormatValue = helper.GetFormatValues(platformSetting.platform);
            platformSetting.format = EditorGUILayout.IntPopup(styles.FormatLabel, platformSetting.format,
                textureFormatValue.FormatStrings,
                textureFormatValue.FormatValues);

            platformSetting.compressionQuality = (UnityEditor.TextureCompressionQuality) EditorGUILayout.EnumPopup(
                styles.CompressionQualityLabel,
                platformSetting.compressionQuality);
        }

        if (EditorGUI.EndChangeCheck()) {
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        GUILayout.EndVertical();
    }
}