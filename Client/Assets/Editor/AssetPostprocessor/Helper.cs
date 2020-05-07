using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Funny.AssetPostprocessor {
    public enum PostprocessorAssetType {
        SpriteAtlas = 0,
        Sprite = 1,
        Texture = 2,
        Model = 3,
    }

    public enum TextureImporterFormatStandalone {
        RGBA32 = TextureImporterFormat.RGBA32,
    }

    public enum TextureImporterFormatIPhone {
        RGB24 = TextureImporterFormat.RGB24,
        RGBA32 = TextureImporterFormat.RGBA32,
        ASTC_RGB_4x4 = TextureImporterFormat.ASTC_RGB_4x4,
        ASTC_RGB_6x6 = TextureImporterFormat.ASTC_RGB_6x6,
        ASTC_RGBA_4x4 = TextureImporterFormat.ASTC_RGBA_4x4,
        ASTC_RGBA_6x6 = TextureImporterFormat.ASTC_RGBA_6x6,
        PVRTC_RGB4 = TextureImporterFormat.PVRTC_RGB4,
        PVRTC_RGBA4 = TextureImporterFormat.PVRTC_RGBA4
    }

    public enum TextureImporterFormatAndroid {
        RGBA32 = TextureImporterFormat.RGBA32,
        ETC_RGB4 = TextureImporterFormat.ETC_RGB4,
        ETC2_RGB4 = TextureImporterFormat.ETC2_RGB4,
        ETC2_RGBA8 = TextureImporterFormat.ETC2_RGBA8,
        ASTC_RGB_4x4 = TextureImporterFormat.ASTC_RGB_4x4,
        ASTC_RGB_6x6 = TextureImporterFormat.ASTC_RGB_6x6,
        ASTC_RGBA_4x4 = TextureImporterFormat.ASTC_RGBA_4x4,
        ASTC_RGBA_6x6 = TextureImporterFormat.ASTC_RGBA_6x6,
    }

    [Serializable]
    public class TexturePlatformSettings {
        public string platform;
        public bool overridden = true;
        public int maxTextureSize = 2048;
        public int format = (int)TextureImporterFormat.Automatic;
        public UnityEditor.TextureCompressionQuality compressionQuality = UnityEditor.TextureCompressionQuality.Normal;
    }

    public static class Helper {
        public readonly struct TextureFormatValue {
            public readonly int[] FormatValues;
            public readonly string[] FormatStrings;

            public TextureFormatValue(int[] formatValues, string[] formatStrings) {
                this.FormatValues = formatValues;
                this.FormatStrings = formatStrings;
            }
        }

        public const string PlatformStandalone = "Standalone";
        public const string PlatformIPhone = "iPhone";
        public const string PlatformAndroid = "Android";

        private static string selectPlatform = string.Empty;

        public static string SelectPlatform {
            get {
                if(string.IsNullOrEmpty(selectPlatform)) {
                    selectPlatform = EditorPrefs.GetString("SoAssetImporter.Platform");
                }

                if(string.IsNullOrEmpty(selectPlatform)) {
                    return PlatformStandalone;
                }

                return selectPlatform;
            }
            set {
                if(selectPlatform != value) {
                    selectPlatform = value;
                    EditorPrefs.SetString("SoAssetImporter.Platform", value);
                }
            }
        }

        public static readonly int[] TextureSizeOptions = new int[] {
            1024,
            2048,
        };

        public static readonly string[] TextureSizeOptionLabels = new string[] {
            "1024", "2048"
        };

        public static readonly string[] Platforms = new string[] {
            PlatformStandalone,
            PlatformIPhone,
            PlatformAndroid,
        };

        private static readonly Dictionary<string, TextureFormatValue> formatValues = new Dictionary<string, TextureFormatValue>();

        public static int GetDefaultTextureFormat(string platform) {
            switch(platform) {
                case PlatformStandalone:
                    return (int)TextureImporterFormatStandalone.RGBA32;
                case PlatformAndroid:
                    return (int)TextureImporterFormatAndroid.ETC2_RGBA8;
                case PlatformIPhone:
                    return (int)TextureImporterFormatIPhone.ASTC_RGBA_6x6;
            }

            return (int)TextureImporterFormat.Automatic;
        }

        public static TextureFormatValue GetFormatValues(string platform) {
            if(formatValues.ContainsKey(platform)) {
                return formatValues[platform];
            }

            Array enumArray = null;
            string[] enumNames = null;
            switch(platform) {
                case PlatformStandalone:
                    enumArray = Enum.GetValues(typeof(TextureImporterFormatStandalone));
                    enumNames = Enum.GetNames(typeof(TextureImporterFormatStandalone));
                    break;
                case PlatformAndroid:
                    enumArray = Enum.GetValues(typeof(TextureImporterFormatAndroid));
                    enumNames = Enum.GetNames(typeof(TextureImporterFormatAndroid));
                    break;
                case PlatformIPhone:
                    enumArray = Enum.GetValues(typeof(TextureImporterFormatIPhone));
                    enumNames = Enum.GetNames(typeof(TextureImporterFormatIPhone));
                    break;
            }

            if(enumArray != null) {
                int[] enumValues = new int[enumArray.Length];
                for(int i = 0; i < enumArray.Length; i++) {
                    enumValues[i] = (int)enumArray.GetValue(i);
                }

                var formatValue = new TextureFormatValue(enumValues, enumNames);
                formatValues[platform] = formatValue;
                return formatValue;
            }

            return default;
        }

        public static string GetSoAssetPostprocessorFolder(PostprocessorAssetType assetType) {
            switch(assetType) {
                case PostprocessorAssetType.SpriteAtlas:
                    return "Assets/Editor/AssetPostprocessor/Config/SpriteAtlas";
                case PostprocessorAssetType.Sprite:
                    return "Assets/Editor/AssetPostprocessor/Config/Sprite";
                case PostprocessorAssetType.Texture:
                    return "Assets/Editor/AssetPostprocessor/Config/Texture";
                case PostprocessorAssetType.Model:
                    return "Assets/Editor/AssetPostprocessor/Config/Model";
                default:
                    throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
            }
        }

        public static string GetAssetSearchFilterByAssetType(PostprocessorAssetType assetType) {
            switch(assetType) {
                case PostprocessorAssetType.SpriteAtlas:
                    return "t:spriteatlas";
                case PostprocessorAssetType.Sprite:
                    return "t:sprite";
                case PostprocessorAssetType.Texture:
                    return "t:texture";
                case PostprocessorAssetType.Model:
                    return "t:model";
            }

            return string.Empty;
        }

        public static bool IsDragFolders(string[] paths) {
            var selectFolder = true;
            if(paths != null && paths.Length > 0) {
                for(int i = 0; i < paths.Length; i++) {
                    if(!Directory.Exists(paths[i])) {
                        selectFolder = false;
                    }
                }
            } else {
                selectFolder = false;
            }

            return selectFolder;
        }

        public static bool IsValuePowerOf2(int value) {
            if(value < 2) {
                return false;
            }

            if((value & value - 1) == 0) {
                return true;
            }

            return false;
        }
    }
}