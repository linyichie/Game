using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class AssetPostprocessorFolderTab : TreeView {
        private readonly EditorWindow parent;
        private AssetPostprocessorHelper.PostprocessorAssetType assetType = (AssetPostprocessorHelper.PostprocessorAssetType)(-1);
        private List<string> paths = new List<string>();
        private bool dirty = false;
        
        private readonly SoAssetPostprocessorFolder soAssetPostprocessorFolder;

        private static Styles styles;

        class Styles {
            public readonly GUIStyle dragDropLabelStyle;

            public Styles() {
                dragDropLabelStyle = new GUIStyle();
                dragDropLabelStyle.normal.textColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.5f);
                dragDropLabelStyle.fontSize = 14;
                dragDropLabelStyle.alignment = TextAnchor.MiddleCenter;
                dragDropLabelStyle.clipping = TextClipping.Clip;
            }
        }

        public static AssetPostprocessorFolderTab Get(EditorWindow parent) {
            var treeView = new AssetPostprocessorFolderTab(parent, new TreeViewState());
            treeView.Reload();
            treeView.showAlternatingRowBackgrounds = true;
            return treeView;
        }

        private AssetPostprocessorFolderTab(EditorWindow parent, TreeViewState state) : base(state) {
            this.parent = parent;
            soAssetPostprocessorFolder = SoAssetPostprocessorFolder.GetSoAssetPostprocessorFolder();
        }

        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();
            if (paths != null) {
                foreach (var path in paths) {
                    root.AddChild(new AssetPostprocessorFolderTreeItem(path, 0, path));
                }
            }

            return root;
        }

        public override void OnGUI(Rect pos) {
            styles = styles ?? new Styles();
            if (CheckDragAndDrop(parent.position)) {
                CheckDragPerform(pos);
            } else {
                GUI.Box(pos, string.Empty, GUI.skin.box);
                if (!rootItem.hasChildren) {
                    GUI.Label(pos, "拖动文件夹至此添加配置", styles.dragDropLabelStyle);
                } else {
                    base.OnGUI(new Rect(pos.x + 1, pos.y + 1, pos.width - 2, pos.height - 2));
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && pos.Contains(Event.current.mousePosition)) {
                        SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
                    }
                }
            }

            if (dirty) {
                Reload();
            }
        }

        protected override void RowGUI(RowGUIArgs args) {
            var cellRect = args.rowRect;
            var item = args.item;
            DefaultGUI.BoldLabel(new Rect(cellRect.x, cellRect.y, cellRect.width, cellRect.height), item.displayName, args.selected, args.focused);
        }

        protected override void ContextClickedItem(int id) {
            var selectIds = GetSelection();

            if (selectIds.Count > 0) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Remove"), false, OnRemoveFolder, selectIds);
                if (menu.GetItemCount() > 0) {
                    menu.ShowAsContext();
                }
            }
        }

        private void OnRemoveFolder(object @object) {
            var selectIds = @object as IList<int>;
            for (int i = 0; i < selectIds.Count; i++) {
                var item = FindItem(selectIds[i], rootItem) as AssetPostprocessorFolderTreeItem;
                if (paths.Contains(item.Path)) {
                    paths.Remove(item.Path);
                }
                soAssetPostprocessorFolder.Remove(this.assetType, item.Path);
            }
            AssetDatabase.SaveAssets();

            dirty = true;
        }

        private bool CheckDragAndDrop(Rect parentRect) {
            parentRect = new Rect(0, 0, parentRect.width, parentRect.height);
            if (parentRect.Contains(Event.current.mousePosition) && (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)) {
                return AssetPostprocessorHelper.IsDragFolders(DragAndDrop.paths);
            }

            return false;
        }

        private void CheckDragPerform(Rect pos) {
            if (pos.Contains(Event.current.mousePosition)) {
                if (Event.current.type == EventType.DragUpdated) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                } else if (Event.current.type == EventType.DragPerform) {
                    DragAndDrop.AcceptDrag();
                    Event.current.Use();
                    TryAddFolder(DragAndDrop.paths);
                }
            }
        }

        private void TryAddFolder(string[] paths) {
            var defaultImporter = SoSpriteAtlasPostprocessor.GetDefaultSoPostprocessor();
            var assetPath = AssetDatabase.GetAssetPath(defaultImporter);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            for (int i = 0; i < paths.Length; i++) {
                var path = paths[i];
                if (!this.paths.Contains(path)) {
                    this.paths.Add(path);
                    soAssetPostprocessorFolder.Set(this.assetType, path, guid);
                    dirty = true;
                }
            }
        }

        public string GetSelectPostprocessorFolder() {
            var selectIds = GetSelection();
            if(selectIds != null && selectIds.Count > 0) {
                var item = FindItem(selectIds[0], rootItem) as AssetPostprocessorFolderTreeItem;
                if(item != null) {
                    return item.Path;
                }
            }

            return string.Empty;
        }

        public void SetAssetType(AssetPostprocessorHelper.PostprocessorAssetType assetType) {
            if (this.assetType != assetType) {
                paths?.Clear();
                this.assetType = assetType;
                paths = soAssetPostprocessorFolder.GetPaths(this.assetType);
                Reload();
            }
        }
    }

    public class AssetPostprocessorFolderTreeItem : TreeViewItem {
        public readonly string Path;

        public AssetPostprocessorFolderTreeItem(string path, int depth, string displayName) : base(path.GetHashCode(), depth, displayName) {
            this.Path = path;
        }
    }
}