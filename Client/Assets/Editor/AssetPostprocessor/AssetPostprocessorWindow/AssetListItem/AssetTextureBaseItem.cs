using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Funny.AssetPostprocessor {
    public class AssetTextureBaseItem : AssetListItem {
        protected static MethodInfo getTextureSizeMethod;
        protected static object[] textureSizeArray;

        protected AssetTextureBaseItem(string path, int depth, string displayName) : base(path, depth, displayName) {
            var type = typeof(TextureImporter);
            getTextureSizeMethod = getTextureSizeMethod ?? type.GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            textureSizeArray = textureSizeArray ?? new object[2];
        }

        public override void VerifyAssetState(SoAssetPostprocessor so) {
            WarnLogic.SetValue(false);

            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var importer = GetAssetImporter<TextureImporter>();
            string message;
            if(!TextureAssetPostprocessor.CompareSettings(GetAssetImporter<TextureImporter>(), texturePostprocessorBase, out message)) {
                WarnLogic.SetValue(true);
            }

            WarnLogic.SetMessage(message.TrimStart('\n'));
        }

        public override void VerifyAssetError(SoAssetPostprocessor so) {
            var (width, height) = GetTextureSize(GetAssetImporter<TextureImporter>());
            var message = string.Empty;
            if(width != height) {
                ErrorLogic.SetValue(true);
                message = "The width of the texture should be the same as the height";
            }

            if(!Helper.IsValuePowerOf2(width) || !Helper.IsValuePowerOf2(height)) {
                ErrorLogic.SetValue(true);
                message = StringUtil.Concat(message, "\n", "The size of the texture should be a power of 2");
            }

            ErrorLogic.SetMessage(message.TrimStart('\n'));
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            TextureAssetPostprocessor.SetPlatformSettings(GetAssetImporter<TextureImporter>(), texturePostprocessorBase);
            EditorUtility.SetDirty(GetAssetImporter<TextureImporter>());
        }

        protected (int, int) GetTextureSize(TextureImporter importer) {
            getTextureSizeMethod.Invoke(importer, textureSizeArray);
            return ((int)textureSizeArray[0], (int)textureSizeArray[1]);
        }
    }

    public class AssetSpriteItem : AssetTextureBaseItem {
        private static Dictionary<string, string> spriteAtlasFile = new Dictionary<string, string>();

        public AssetSpriteItem(string path, int depth, string displayName) : base(path, depth, displayName) { }

        public override void VerifyAssetError(SoAssetPostprocessor so) {
            ErrorLogic.SetValue(false);
            var inSpriteAtlas = false;
            var folder = System.IO.Path.GetDirectoryName(Path);
            var spriteAtlasAssetPath = StringUtil.Concat(folder.Replace("Sprite", "Atlas"), ".spriteatlas");
            if(File.Exists(spriteAtlasAssetPath)) {
                string allText;
                if(spriteAtlasFile.ContainsKey(spriteAtlasAssetPath)) {
                    allText = spriteAtlasFile[spriteAtlasAssetPath];
                } else {
                    allText = File.ReadAllText(spriteAtlasAssetPath);
                    spriteAtlasFile[spriteAtlasAssetPath] = allText;
                }

                if(allText.Contains(AssetDatabase.AssetPathToGUID(Path))) {
                    inSpriteAtlas = true;
                }
            }

            if(!inSpriteAtlas) {
                base.VerifyAssetError(so);
            }
        }

        public static void ClearSpriteAtlasFile() {
            spriteAtlasFile.Clear();
        }
    }

    public class AssetTextureItem : AssetTextureBaseItem {
        public AssetTextureItem(string path, int depth, string displayName) : base(path, depth, displayName) { }

        public override void VerifyAssetError(SoAssetPostprocessor so) {
            ErrorLogic.SetValue(false);
            var textureImporter = GetAssetImporter<TextureImporter>();
            if(textureImporter.npotScale == TextureImporterNPOTScale.None) {
                base.VerifyAssetError(so);
            }
        }
    }
}