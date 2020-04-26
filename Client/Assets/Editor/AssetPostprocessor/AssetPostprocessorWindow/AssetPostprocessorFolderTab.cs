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
        private readonly List<Folder> folders = new List<Folder>();
        private readonly SoAssetPostprocessorFolder soAssetPostprocessorFolder;
        private PostprocessorAssetType assetType = (PostprocessorAssetType) (-1);
        private List<string> paths = new List<string>();
        private bool dirty = false;
        private bool contextOnItem = false;

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

        class Folder {
            public string path;
            public Folder parent;
            public List<Folder> childs;
        }

        public static AssetPostprocessorFolderTab Get(EditorWindow parent) {
            var treeView = new AssetPostprocessorFolderTab(parent, new TreeViewState());
            treeView.Reload();
            return treeView;
        }

        private AssetPostprocessorFolderTab(EditorWindow parent, TreeViewState state) : base(state) {
            this.parent = parent;
            showAlternatingRowBackgrounds = true;
            soAssetPostprocessorFolder = SoAssetPostprocessorFolder.GetSoAssetPostprocessorFolder();
        }

        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem(-1, -1) {children = new List<TreeViewItem>()};
            folders.Clear();
            paths.Sort((lhs, rhs) => lhs.Length.CompareTo(rhs.Length));
            foreach (var path in paths) {
                if (!SetFolder(folders, path)) {
                    folders.Add(new Folder() {
                        path = path
                    });
                }
            }

            foreach (var folder in folders) {
                CreateTreeItem(root, folder);
            }

            return root;
        }

        private bool SetFolder(List<Folder> folders, string path) {
            foreach (var folder in folders) {
                if (path.StartsWith(folder.path + "/")) {
                    if (folder.childs != null) {
                        if (SetFolder(folder.childs, path)) {
                            return true;
                        }
                    }

                    folder.childs = folder.childs ?? new List<Folder>();
                    folder.childs.Add(new Folder() {
                        parent = folder,
                        path = path,
                    });
                    return true;
                }
            }

            return false;
        }

        private void CreateTreeItem(TreeViewItem root, Folder folder) {
            var displayName = folder.path;
            if (folder.parent != null) {
                displayName = folder.path.Remove(0, folder.parent.path.Length + 1);
            }

            var treeItem = new AssetPostprocessorFolderTreeItem(folder.path, root.depth + 1, displayName);
            root.AddChild(treeItem);
            if (folder.childs == null) {
                return;
            }

            foreach (var item in folder.childs) {
                CreateTreeItem(treeItem, item);
            }
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

        protected override void DoubleClickedItem(int id) {
            var item = FindItem(id, rootItem) as AssetPostprocessorFolderTreeItem;
            if (item != null) {
                var o = AssetDatabase.LoadAssetAtPath<Object>(item.Path);
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }

        protected override void ContextClicked() {
            if (contextOnItem) {
                contextOnItem = false;
                return;
            }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("拖拽文件夹添加配置"), false, null);

            if (menu.GetItemCount() > 0) {
                menu.ShowAsContext();
            }
        }

        protected override void ContextClickedItem(int id) {
            contextOnItem = true;
            
            var selectIds = GetSelection();

            if (selectIds.Count > 0) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Remove"), false, OnRemoveFolder, selectIds);
                if (menu.GetItemCount() > 0) {
                    menu.ShowAsContext();
                }
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item) {
            return false;
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
                    AddFolder(DragAndDrop.paths);
                }
            }
        }

        private void AddFolder(string[] paths) {
            var defaultSo = SoAssetPostprocessor.GetDefault(this.assetType);
            ;
            var assetPath = AssetDatabase.GetAssetPath(defaultSo);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            for (var i = 0; i < paths.Length; i++) {
                var path = paths[i];
                if (this.paths.Contains(path)) {
                    continue;
                }

                this.paths.Add(path);
                soAssetPostprocessorFolder.Set(this.assetType, path, guid);
                dirty = true;
            }
        }

        public string GetSelectPostprocessorFolder() {
            var selectIds = GetSelection();
            if (selectIds != null && selectIds.Count > 0) {
                var item = FindItem(selectIds[0], rootItem) as AssetPostprocessorFolderTreeItem;
                if (item != null) {
                    return item.Path;
                }
            }

            return string.Empty;
        }

        public void SetAssetType(PostprocessorAssetType assetType) {
            if (this.assetType == assetType) {
                return;
            }

            this.assetType = assetType;
            Refresh();
        }

        public void Refresh() {
            paths?.Clear();
            paths = soAssetPostprocessorFolder.GetPaths(this.assetType);
            Reload();
        }
    }

    public class AssetPostprocessorFolderTreeItem : TreeViewItem {
        public readonly string Path;

        public AssetPostprocessorFolderTreeItem(string path, int depth, string displayName) : base(path.GetHashCode(), depth, displayName) {
            this.Path = path;
        }
    }
}