using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SoSpriteImporter : SoAssetImporter {
    public List<AssetImporterHelper.TexturePlatformSettings> platformSettings =
        new List<AssetImporterHelper.TexturePlatformSettings>();

    public AssetImporterHelper.TexturePlatformSettings GetPlatformSettings(string platform) {
        var index = platformSettings.FindIndex((x) => { return x.platform == platform; });
        if (index == -1) {
            var platformSetting = new AssetImporterHelper.TexturePlatformSettings() {
                platform = platform,
                maxTextureSize = 2048,
                compressionQuality = 50,
                format = AssetImporterHelper.GetDefaultTextureFormat(platform),
            };
            platformSettings.Add(platformSetting);
            return platformSetting;
        }

        return platformSettings[index];
    }

    [MenuItem("Tools/资源导入规范/Sprite")]
    static void Create() {
        var so = AssetDatabase.LoadAssetAtPath<SoSpriteAtlasImporter>(SoPath.Instance["SpriteImporter"]);
        if (so == null) {
            so = ScriptableObject.CreateInstance<SoSpriteAtlasImporter>();
            AssetDatabase.CreateAsset(so, SoPath.Instance["SpriteImporter"]);
            AssetDatabase.Refresh();
        }
    }

    public static SoSpriteAtlasImporter GetSoSpriteAtlasImporter() {
        var so = AssetDatabase.LoadAssetAtPath<SoSpriteAtlasImporter>(SoPath.Instance["SpriteImporter"]);
        return so;
    }
}

[CustomEditor(typeof(SoSpriteImporter))]
public class SoSpriteImpoterInspector : Editor {
    class Styles {
        public readonly string textureSizeLabel = "Max Texture Size";

        public readonly string formatLabel = "Format";

        public readonly string compressionQualityLabel = "Compressor Quality";
    }

    private int selectPlatformIndex = 0;

    private Styles styles;
    private AssetImporterHelper helper;

    private void OnEnable() {
        styles = styles ?? new Styles();
        helper = helper ?? new AssetImporterHelper();
    }

    private void OnDisable() {
    }

    public override void OnInspectorGUI() {
        var soImporter = target as SoSpriteAtlasImporter;
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        {
            for (int i = 0; i < helper.platforms.Length; i++) {
                if (GUILayout.Toggle(selectPlatformIndex == i, helper.platforms[i],
                    EditorStyles.toolbarButton)) {
                    selectPlatformIndex = i;
                }
            }
        }
        GUILayout.EndHorizontal();

        var selectPlatform = helper.platforms[selectPlatformIndex];
        var platformSetting = soImporter.GetPlatformSettings(selectPlatform);
        platformSetting.maxTextureSize = EditorGUILayout.IntPopup(styles.textureSizeLabel,
            platformSetting.maxTextureSize,
            helper.textureSizeOptionLabels,
            helper.textureSizeOptions);

        var textureFormatValue = helper.GetFormatValues(platformSetting.platform);
        platformSetting.format = EditorGUILayout.IntPopup(styles.formatLabel, platformSetting.format,
            textureFormatValue.formatStrings,
            textureFormatValue.formatValues);

        platformSetting.compressionQuality = EditorGUILayout.IntPopup(styles.compressionQualityLabel,
            platformSetting.compressionQuality
            , helper.compressionQualityOptionLabels,
            helper.compressionQualityOptions);

        GUILayout.EndVertical();
    }
}