using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class AssetPostprocessorHelper {
        public enum TextureImporterFormatStandalone {
            RGBA32 = TextureImporterFormat.RGBA32,
        }

        public enum TextureImporterFormatIPhone {
            RGBA32 = TextureImporterFormat.RGBA32,
            Astc6X6 = TextureImporterFormat.ASTC_6x6,
            PVRTC_RGB4 = TextureImporterFormat.PVRTC_RGB4,
        }

        public enum TextureImporterFormatAndroid {
            RGBA32 = TextureImporterFormat.RGBA32,
            ETC2_RGBA8 = TextureImporterFormat.ETC2_RGBA8,
            ETC2_RGB4 = TextureImporterFormat.ETC2_RGB4,
        }

        public enum PostprocessorAssetType {
            SpriteAtlas = 0,
            Sprite = 1,
            Texture = 2,
            Model = 3,
        }

        [Serializable]
        public class TexturePlatformSettings {
            public string platform;
            public int maxTextureSize = 2048;
            public int format = (int) TextureImporterFormat.Automatic;
            public UnityEditor.TextureCompressionQuality compressionQuality = UnityEditor.TextureCompressionQuality.Normal;
        }

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
                if (string.IsNullOrEmpty(selectPlatform)) {
                    selectPlatform = EditorPrefs.GetString("SoAssetImporter.Platform");
                }

                if (string.IsNullOrEmpty(selectPlatform)) {
                    return PlatformStandalone;
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

        public readonly int[] TextureSizeOptions = new int[] {
            1024,
            2048,
            4096,
        };

        public readonly string[] TextureSizeOptionLabels = new string[] {
            "1024", "2048", "4096"
        };

        public static readonly string[] Platforms = new string[] {
            PlatformStandalone,
            PlatformIPhone,
            PlatformAndroid,
        };

        private readonly Dictionary<string, TextureFormatValue> formatValues = new Dictionary<string, TextureFormatValue>();

        public static int GetDefaultTextureFormat(string platform) {
            switch (platform) {
                case PlatformStandalone:
                    return (int) TextureImporterFormatStandalone.RGBA32;
                case PlatformAndroid:
                    return (int) TextureImporterFormatAndroid.ETC2_RGBA8;
                case PlatformIPhone:
                    return (int) TextureImporterFormatIPhone.Astc6X6;
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

            if (enumArray != null) {
                int[] enumValues = new int[enumArray.Length];
                for (int i = 0; i < enumArray.Length; i++) {
                    enumValues[i] = (int) enumArray.GetValue(i);
                }

                var formatValue = new TextureFormatValue(enumValues, enumNames);
                formatValues[platform] = formatValue;
                return formatValue;
            }

            return default;
        }

        public static string GetSoAssetPostprocessorFolder(PostprocessorAssetType assetType) {
            switch (assetType) {
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

        public static bool IsDragFolders(string[] paths) {
            var selectFolder = true;
            if (paths != null && paths.Length > 0) {
                for (int i = 0; i < paths.Length; i++) {
                    if (!Directory.Exists(paths[i])) {
                        selectFolder = false;
                    }
                }
            } else {
                selectFolder = false;
            }

            return selectFolder;
        }
    }
}