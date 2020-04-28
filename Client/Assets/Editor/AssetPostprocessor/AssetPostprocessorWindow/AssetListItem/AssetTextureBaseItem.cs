using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace LinChunJie.AssetPostprocessor {
    public class AssetTextureBaseItem : AssetListItem {
        protected static MethodInfo getTextureSizeMethod;
        protected static object[] textureSizeArray;

        protected AssetTextureBaseItem(string path, int depth, string displayName) : base(path, depth, displayName) {
            var type = typeof(TextureImporter);
            getTextureSizeMethod = getTextureSizeMethod ?? type.GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            textureSizeArray = textureSizeArray ?? new object[2];
        }

        public override void VerifyAssetState(SoAssetPostprocessor so) {
            IsChanged = false;

            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            if(!TextureAssetPostprocessor.CompareSettings(GetAssetImporter<TextureImporter>(), texturePostprocessorBase)) {
                IsChanged = true;
            }

            IsDirty = false;
        }

        public override void VerifyAssetError(SoAssetPostprocessor so) {
            IsErrorDirty = false;
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
        public AssetSpriteItem(string path, int depth, string displayName) : base(path, depth, displayName) { }

        public override void VerifyAssetError(SoAssetPostprocessor so) {
            IsError = false;
            var folder = System.IO.Path.GetDirectoryName(Path);
            var spriteAtlasAssetPath = StringUtility.Contact(folder.Replace("Sprite", "Atlas"), ".spriteatlas");
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(spriteAtlasAssetPath);
            if(spriteAtlas != null) {
                if(spriteAtlas.GetSprite(displayName) == null) {
                    var (width, height) = GetTextureSize(GetAssetImporter<TextureImporter>());
                    if(width != height) {
                        IsError = true;
                    }

                    if(!Helper.IsValuePowerOf2(width) || !Helper.IsValuePowerOf2(height)) {
                        IsError = true;
                    }
                }
            }

            base.VerifyAssetError(so);
        }
    }

    public class AssetTextureItem : AssetTextureBaseItem {
        public AssetTextureItem(string path, int depth, string displayName) : base(path, depth, displayName) { }

        public override void VerifyAssetError(SoAssetPostprocessor so) {
            IsError = false;
            var textureImporter = GetAssetImporter<TextureImporter>();
            if(textureImporter.npotScale == TextureImporterNPOTScale.None) {
                var (width, height) = GetTextureSize(textureImporter);
                if(width != height) {
                    IsError = true;
                }

                if(!Helper.IsValuePowerOf2(width) || !Helper.IsValuePowerOf2(height)) {
                    IsError = true;
                }
            }

            base.VerifyAssetError(so);
        }
    }
}