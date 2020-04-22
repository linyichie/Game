using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AssetImporterHelper {
    public enum TextureImporterFormat_Standalone {
        RGBA32 = TextureImporterFormat.RGBA32,
    }

    public enum TextureImporterFormat_iPhone {
        RGBA32 = TextureImporterFormat.RGBA32,
        ASTC_6x6 = TextureImporterFormat.ASTC_6x6,
    }

    public enum TextureImporterFormat_Android {
        RGBA32 = TextureImporterFormat.RGBA32,
        ETC2_RGBA8 = TextureImporterFormat.ETC2_RGBA8,
        ETC2_RGB4 = TextureImporterFormat.ETC2_RGB4,
    }

    [Serializable]
    public class TexturePlatformSettings {
        public string platform;
        public int textureMaxSize = 2048;
        public int format = (int) TextureImporterFormat.Automatic;
        public int compressionQuality = 50;
    }

    public struct TextureFormatValue {
        public int[] formatValues;
        public string[] formatStrings;

        public TextureFormatValue(int[] formatValues, string[] formatStrings) {
            this.formatValues = formatValues;
            this.formatStrings = formatStrings;
        }
    }

    public const string Platform_Standalone = "Standalone";
    public const string Platform_iPhone = "iPhone";
    public const string Platform_Android = "Android";

    private string m_SelectPlatform = string.Empty;

    public string selectPlatform {
        get {
            if (string.IsNullOrEmpty(selectPlatform)) {
                selectPlatform = EditorPrefs.GetString("SoAssetImporter.Platform");
            }

            if (string.IsNullOrEmpty(selectPlatform)) {
                return Platform_Standalone;
            }

            return selectPlatform;
        }
        set {
            if (selectPlatform != value) {
                selectPlatform = value;
                EditorPrefs.SetString("SoAssetImporter.Platform", value);
            }
        }
    }

    public readonly int[] textureSizeOptions = new int[] {
        1024,
        2048,
        4096,
    };

    public readonly int[] compressionQualityOptions = new int[] {
        0,
        50,
        100,
    };

    public readonly string[] textureSizeOptionLabels = new string[] {
        "1024", "2048", "4096"
    };

    public readonly string[] compressionQualityOptionLabels = new string[] {
        "Fast", "Normal", "Best"
    };

    public readonly string[] platforms = new string[] {
        Platform_Standalone,
        Platform_iPhone,
        Platform_Android,
    };

    private Dictionary<string, TextureFormatValue> formatValues = new Dictionary<string, TextureFormatValue>();

    public static int GetDefaultTextureFormat(string platform) {
        switch (platform) {
            case Platform_Standalone:
                return (int) TextureImporterFormat_Standalone.RGBA32;
            case Platform_Android:
                return (int) TextureImporterFormat_Android.ETC2_RGBA8;
            case Platform_iPhone:
                return (int) TextureImporterFormat_iPhone.ASTC_6x6;
        }
        return (int) TextureImporterFormat.Automatic;
    }

    public TextureFormatValue GetFormatValues(string platform) {
        if (formatValues.ContainsKey(platform)) {
            return formatValues[platform];
        }

        Array enumArray = null;
        string[] enumNames = null;
        switch (platform) {
            case Platform_Standalone:
                enumArray = Enum.GetValues(typeof(TextureImporterFormat_Standalone));
                enumNames = Enum.GetNames(typeof(TextureImporterFormat_Standalone));
                break;
                ;
            case Platform_Android:
                enumArray = Enum.GetValues(typeof(TextureImporterFormat_Android));
                enumNames = Enum.GetNames(typeof(TextureImporterFormat_Android));
                break;
            case Platform_iPhone:
                enumArray = Enum.GetValues(typeof(TextureImporterFormat_iPhone));
                enumNames = Enum.GetNames(typeof(TextureImporterFormat_iPhone));
                break;
        }

        int[] enumValues = new int[enumArray.Length];
        for (int i = 0; i < enumArray.Length; i++) {
            enumValues[i] = (int) enumArray.GetValue(i);
        }

        var formatValue = new TextureFormatValue(enumValues, enumNames);
        formatValues[platform] = formatValue;
        return formatValue;
    }
}