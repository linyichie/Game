using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public abstract class AssetListItem : TreeViewItem {
        public readonly string Path;
        public bool IsDirty { get; protected set; }

        protected AssetListItem(string path, int depth, string displayName) : base(path.GetHashCode(), depth, displayName) {
            this.Path = path;
        }

        public abstract void VerifyImporterSetting(SoAssetPostprocessor so);

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