using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public abstract class AssetListItem : TreeViewItem {
        public readonly string Path;
        public bool IsChanged { get; protected set; } = false;
        public bool IsError { get; protected set; } = false;
        public bool IsDirty { get; protected set; } = true;
        public bool IsErrorDirty { get; protected set; } = true;

        protected AssetImporter importer;

        protected AssetListItem(string path, int depth, string displayName) : base(path.GetHashCode(), depth, displayName) {
            this.Path = path;
        }

        public abstract void VerifyAssetState(SoAssetPostprocessor so);

        public abstract void VerifyAssetError(SoAssetPostprocessor so);

        protected T GetAssetImporter<T>() where T : AssetImporter {
            if(importer == null) {
                importer = AssetImporter.GetAtPath(Path);
            }

            return importer as T;
        }

        public virtual void SetDirty() {
            IsDirty = true;
        }

        public abstract void FixAndReimport(SoAssetPostprocessor so);

        public static AssetListItem Get(PostprocessorAssetType assetType, string path, int depth, string displayName) {
            switch(assetType) {
                case PostprocessorAssetType.SpriteAtlas:
                    return new AssetListSpriteAtlasItem(path, depth, displayName);
                case PostprocessorAssetType.Sprite:
                    return new AssetSpriteItem(path, depth, displayName);
                case PostprocessorAssetType.Texture:
                    return new AssetTextureItem(path, depth, displayName);
                case PostprocessorAssetType.Model:
                    return new AssetModelItem(path, depth, displayName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
            }

            return null;
        }
    }
}