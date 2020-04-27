using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace LinChunJie.AssetPostprocessor {
    public class AssetListTab : TreeView {
        private PostprocessorAssetType assetType = (PostprocessorAssetType) (-1);
        private string folder = string.Empty;
        private bool contextOnItem = false;

        private readonly SoAssetPostprocessorFolder postprocessorFolder;
        private readonly List<string> paths = new List<string>();
        private readonly Texture2D warnIcon;

        public static AssetListTab Get() {
            var treeView = new AssetListTab(new TreeViewState());
            treeView.Reload();
            return treeView;
        }

        private AssetListTab(TreeViewState state) : base(state) {
            showAlternatingRowBackgrounds = true;
            postprocessorFolder = SoAssetPostprocessorFolder.GetSoAssetPostprocessorFolder();
            warnIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/AssetPostprocessor/Texture/Warn.png");
        }

        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();
            if (paths.Count > 0) {
                var guid = postprocessorFolder.Get(this.assetType, this.folder);
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<SoAssetPostprocessor>(assetPath);
                foreach (var path in paths) {
                    var treeViewItem = AssetListItem.Get(this.assetType, path, 0, Path.GetFileNameWithoutExtension(path));
                    treeViewItem.VerifyImporterSetting(so);
                    root.AddChild(treeViewItem);
                }
            }

            return root;
        }

        public override void OnGUI(Rect pos) {
            GUI.Box(pos, string.Empty, GUI.skin.box);
            base.OnGUI(new Rect(pos.x + 1, pos.y + 1, pos.width - 2, pos.height - 2));
        }

        protected override void RowGUI(RowGUIArgs args) {
            var rect = args.rowRect;
            var item = args.item as AssetListItem;
            var iconRect = new Rect(rect.x + 1, rect.y + 1, rect.height - 2, rect.height - 2);
            GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
            var labelRect = new Rect(rect.x + iconRect.xMax + 1, rect.y, rect.width - iconRect.width - 1, rect.height);
            if (item.IsDirty) {
                var warnRect = new Rect(rect.width - rect.height, rect.y + 1, rect.height - 2, rect.height - 2);
                GUI.DrawTexture(warnRect, warnIcon, ScaleMode.ScaleToFit);

                labelRect.width = labelRect.width - warnRect.width - 2;
            }

            DefaultGUI.BoldLabel(labelRect, item.displayName, args.selected, args.focused);
        }

        protected override void DoubleClickedItem(int id) {
            var item = FindItem(id, rootItem) as AssetListItem;
            if (item != null) {
                var o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item.Path);
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }

        protected override void ContextClicked() {
            if (contextOnItem) {
                contextOnItem = false;
                return;
            }
        }

        protected override void ContextClickedItem(int id) {
            contextOnItem = true;

            var selectIds = GetSelection();

            if (selectIds.Count > 0) {
                GenericMenu menu = new GenericMenu();

                var isDirty = false;
                for (int i = 0; i < selectIds.Count; i++) {
                    var item = FindItem(selectIds[i], rootItem) as AssetListItem;
                    if (item.IsDirty) {
                        isDirty = true;
                        break;
                    }
                }

                if (isDirty) {
                    menu.AddItem(new GUIContent("FixAndReimport"), false, OnFixAndReimport, selectIds);
                } else {
                    menu.AddItem(new GUIContent("FixAndReimport"), false, null);
                }

                if (menu.GetItemCount() > 0) {
                    menu.ShowAsContext();
                }
            }
        }

        private void OnFixAndReimport(object o) {
            var selectIds = o as IList<int>;
            if (selectIds != null) {
                var guid = postprocessorFolder.Get(this.assetType, this.folder);
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<SoAssetPostprocessor>(assetPath);
                for (int i = 0; i < selectIds.Count; i++) {
                    var item = FindItem(selectIds[i], rootItem) as AssetListItem;
                    item.FixAndReimport(so);
                }
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                Repaint();
            }
        }

        public void SetFolder(PostprocessorAssetType assetType, string path) {
            if (this.assetType != assetType || this.folder != path) {
                this.assetType = assetType;
                this.folder = path;
                Refresh();
            }
        }

        public void Refresh() {
            paths.Clear();
            if (!string.IsNullOrEmpty(this.folder)) {
                var guids = AssetDatabase.FindAssets(AssetPostprocessorHelper.GetAssetSearchFilterByAssetType(this.assetType), new string[] {
                    this.folder
                });
                if (guids != null) {
                    List<string> directories = new List<string>();
                    for (int i = 0; i < guids.Length; i++) {
                        var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        GetParentDirectories(this.folder, path, ref directories);
                        if (directories.Count == 0 || !postprocessorFolder.ContainsOneOfFolders(this.assetType, directories)) {
                            paths.Add(path);
                        }
                    }
                }
            }

            Reload();
        }

        public void SoPostprocessorChanged(string guid) {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<SoAssetPostprocessor>(assetPath);
            for (int i = 0; i < rootItem.children.Count; i++) {
                var item = rootItem.children[i] as AssetListItem;
                item.VerifyImporterSetting(so);
            }
        }

        private void GetParentDirectories(string rootPath, string path, ref List<string> directories) {
            directories.Clear();
            path = Path.GetDirectoryName(path);
            while (path.Length > rootPath.Length) {
                path = path.Replace('\\', '/');
                directories.Add(path);
                path = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal));
            }
        }

        public bool IsAnyOfAssetDirty() {
            for (int i = 0; i < rootItem.children.Count; i++) {
                var item = rootItem.children[i] as AssetListItem;
                if (item.IsDirty) {
                    return true;
                }
            }

            return false;
        }

        public void FixAllDirty() {
            var guid = postprocessorFolder.Get(this.assetType, this.folder);
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<SoAssetPostprocessor>(assetPath);
            for (int i = 0; i < rootItem.children.Count; i++) {
                var item = rootItem.children[i] as AssetListItem;
                if (item.IsDirty) {
                    item.FixAndReimport(so);
                }
            }
        }
    }

    public abstract class AssetListItem : TreeViewItem {
        public readonly string Path;
        public bool IsDirty { get; protected set; }

        protected AssetListItem(string path, int depth, string displayName) : base(path.GetHashCode(), depth, displayName) {
            this.Path = path;
            icon = AssetDatabase.GetCachedIcon(path) as Texture2D;
        }

        public abstract void VerifyImporterSetting(SoAssetPostprocessor so);

        public abstract void FixAndReimport(SoAssetPostprocessor so);

        public static AssetListItem Get(PostprocessorAssetType assetType, string path, int depth, string displayName) {
            switch (assetType) {
                case PostprocessorAssetType.SpriteAtlas:
                    return new AssetListSpriteAtlasItem(path, depth, displayName);
                case PostprocessorAssetType.Sprite:
                    return new AssetListSpriteItem(path, depth, displayName);
                case PostprocessorAssetType.Texture:
                    return new AssetListTextureItem(path, depth, displayName);
                case PostprocessorAssetType.Model:
                    return new AssetListModelItem(path, depth, displayName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
            }

            return null;
        }
    }

    public class AssetListTextureBaseItem : AssetListItem {
        protected AssetListTextureBaseItem(string path, int depth, string displayName) : base(path, depth, displayName) {
        }

        public override void VerifyImporterSetting(SoAssetPostprocessor so) {
            IsDirty = false;

            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var importer = AssetImporter.GetAtPath(Path) as TextureImporter;

            var soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(AssetPostprocessorHelper.PlatformStandalone);
            var texturePlatformSettings = importer.GetPlatformTextureSettings(AssetPostprocessorHelper.PlatformStandalone);
            if (!CompareTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings)) {
                IsDirty = true;
            }

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(AssetPostprocessorHelper.PlatformAndroid);
            texturePlatformSettings = importer.GetPlatformTextureSettings(AssetPostprocessorHelper.PlatformAndroid);
            if (!CompareTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings)) {
                IsDirty = true;
            }

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(AssetPostprocessorHelper.PlatformIPhone);
            texturePlatformSettings = importer.GetPlatformTextureSettings(AssetPostprocessorHelper.PlatformIPhone);
            if (!CompareTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings)) {
                IsDirty = true;
            }
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var importer = AssetImporter.GetAtPath(Path) as TextureImporter;

            var soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(AssetPostprocessorHelper.PlatformStandalone);
            var texturePlatformSettings = importer.GetPlatformTextureSettings(AssetPostprocessorHelper.PlatformStandalone);
            SetTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings);
            importer.SetPlatformTextureSettings(texturePlatformSettings);

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(AssetPostprocessorHelper.PlatformAndroid);
            texturePlatformSettings = importer.GetPlatformTextureSettings(AssetPostprocessorHelper.PlatformAndroid);
            SetTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings);
            importer.SetPlatformTextureSettings(texturePlatformSettings);

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(AssetPostprocessorHelper.PlatformIPhone);
            texturePlatformSettings = importer.GetPlatformTextureSettings(AssetPostprocessorHelper.PlatformIPhone);
            SetTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings);
            importer.SetPlatformTextureSettings(texturePlatformSettings);

            importer.SaveAndReimport();
            // -- Todo 更新对应 Inspector 显示
            VerifyImporterSetting(so);
        }

        private void SetTexturePlatformSetting(AssetPostprocessorHelper.TexturePlatformSettings so, TextureImporterPlatformSettings texturePlatformSettings) {
            texturePlatformSettings.format = (TextureImporterFormat) so.format;
            texturePlatformSettings.maxTextureSize = so.maxTextureSize;
            texturePlatformSettings.compressionQuality = (int) so.compressionQuality;
        }

        private bool CompareTexturePlatformSetting(AssetPostprocessorHelper.TexturePlatformSettings so, TextureImporterPlatformSettings texturePlatformSettings) {
            if ((int) so.format != (int) texturePlatformSettings.format || (int) so.compressionQuality != texturePlatformSettings.compressionQuality || so.maxTextureSize != texturePlatformSettings.maxTextureSize) {
                return false;
            }

            return true;
        }
    }

    public class AssetListSpriteAtlasItem : AssetListItem {
        public AssetListSpriteAtlasItem(string path, int depth, string displayName) : base(path, depth, displayName) {
        }

        public override void VerifyImporterSetting(SoAssetPostprocessor so) {
            IsDirty = false;

            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(Path);

            var soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(AssetPostprocessorHelper.PlatformStandalone);
            var texturePlatformSettings = spriteAtlas.GetPlatformSettings(AssetPostprocessorHelper.PlatformStandalone);
            if (!CompareTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings)) {
                IsDirty = true;
            }

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(AssetPostprocessorHelper.PlatformAndroid);
            texturePlatformSettings = spriteAtlas.GetPlatformSettings(AssetPostprocessorHelper.PlatformAndroid);
            if (!CompareTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings)) {
                IsDirty = true;
            }

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(AssetPostprocessorHelper.PlatformIPhone);
            texturePlatformSettings = spriteAtlas.GetPlatformSettings(AssetPostprocessorHelper.PlatformIPhone);
            if (!CompareTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings)) {
                IsDirty = true;
            }
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
            var texturePostprocessorBase = so as SoTexturePostprocessorBase;
            var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(Path);

            var soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(AssetPostprocessorHelper.PlatformStandalone);
            var texturePlatformSettings = spriteAtlas.GetPlatformSettings(AssetPostprocessorHelper.PlatformStandalone);
            SetTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings);
            spriteAtlas.SetPlatformSettings(texturePlatformSettings);

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(AssetPostprocessorHelper.PlatformAndroid);
            texturePlatformSettings = spriteAtlas.GetPlatformSettings(AssetPostprocessorHelper.PlatformAndroid);
            SetTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings);
            spriteAtlas.SetPlatformSettings(texturePlatformSettings);

            soTexturePlatformSettings = texturePostprocessorBase.GetPlatformSettings(AssetPostprocessorHelper.PlatformIPhone);
            texturePlatformSettings = spriteAtlas.GetPlatformSettings(AssetPostprocessorHelper.PlatformIPhone);
            SetTexturePlatformSetting(soTexturePlatformSettings, texturePlatformSettings);
            spriteAtlas.SetPlatformSettings(texturePlatformSettings);

            EditorUtility.SetDirty(spriteAtlas);
            AssetDatabase.SaveAssets();
            VerifyImporterSetting(so);
        }

        private void SetTexturePlatformSetting(AssetPostprocessorHelper.TexturePlatformSettings so, TextureImporterPlatformSettings texturePlatformSettings) {
            texturePlatformSettings.format = (TextureImporterFormat) so.format;
            texturePlatformSettings.maxTextureSize = so.maxTextureSize;
            texturePlatformSettings.compressionQuality = (int) so.compressionQuality;
        }

        private bool CompareTexturePlatformSetting(AssetPostprocessorHelper.TexturePlatformSettings so, TextureImporterPlatformSettings texturePlatformSettings) {
            var same = true;
            same &= so.format == (int) texturePlatformSettings.format;
            same &= (int) so.compressionQuality == texturePlatformSettings.compressionQuality;
            same &= so.maxTextureSize == texturePlatformSettings.maxTextureSize;
            return same;
        }
    }

    public class AssetListSpriteItem : AssetListTextureBaseItem {
        public AssetListSpriteItem(string path, int depth, string displayName) : base(path, depth, displayName) {
        }
    }

    public class AssetListTextureItem : AssetListTextureBaseItem {
        public AssetListTextureItem(string path, int depth, string displayName) : base(path, depth, displayName) {
        }
    }

    public class AssetListModelItem : AssetListItem {
        public AssetListModelItem(string path, int depth, string displayName) : base(path, depth, displayName) {
        }

        public override void VerifyImporterSetting(SoAssetPostprocessor so) {
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
        }
    }
}