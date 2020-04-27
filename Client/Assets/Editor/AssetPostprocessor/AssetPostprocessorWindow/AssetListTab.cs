using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.U2D;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LinChunJie.AssetPostprocessor {
    public class AssetListTab : TreeView {
        private PostprocessorAssetType assetType = (PostprocessorAssetType)(-1);
        private string folder = string.Empty;
        private bool contextOnItem = false;

        private readonly SoAssetPostprocessorFolder postprocessorFolder;
        private readonly Texture2D warnIcon;
        private readonly List<AssetListItem> listItems = new List<AssetListItem>();

        private SoAssetPostprocessor soAssetPostprocessor;

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

        public void OnLostFocus() { }

        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();
            if(listItems.Count > 0) {
                foreach(var item in listItems) {
                    root.AddChild(item);
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
            //item.icon = item.icon ? item.icon : (AssetDatabase.GetCachedIcon(item.Path) as Texture2D);
            //GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
            var labelRect = new Rect(rect.x + iconRect.xMax + 1, rect.y, rect.width - iconRect.width - 1, rect.height);
            if(item.IsDirty) {
                var warnRect = new Rect(rect.width - rect.height, rect.y + 1, rect.height - 2, rect.height - 2);
                GUI.DrawTexture(warnRect, warnIcon, ScaleMode.ScaleToFit);

                labelRect.width = labelRect.width - warnRect.width - 2;
            }

            DefaultGUI.BoldLabel(labelRect, item.displayName, args.selected, args.focused);
        }

        protected override void DoubleClickedItem(int id) {
            var item = FindItem(id, rootItem) as AssetListItem;
            if(item != null) {
                var o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item.Path);
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }

        protected override void ContextClicked() {
            if(contextOnItem) {
                contextOnItem = false;
                return;
            }
        }

        protected override void ContextClickedItem(int id) {
            contextOnItem = true;

            var selectIds = GetSelection();

            if(selectIds.Count > 0) {
                GenericMenu menu = new GenericMenu();

                var isDirty = false;
                for(int i = 0; i < selectIds.Count; i++) {
                    var item = FindItem(selectIds[i], rootItem) as AssetListItem;
                    if(item.IsDirty) {
                        isDirty = true;
                        break;
                    }
                }

                if(isDirty) {
                    menu.AddItem(new GUIContent("FixAndReimport"), false, OnFixAndReimport, selectIds);
                } else {
                    menu.AddItem(new GUIContent("FixAndReimport"), false, null);
                }

                if(menu.GetItemCount() > 0) {
                    menu.ShowAsContext();
                }
            }
        }

        private void OnFixAndReimport(object o) {
            var selectIds = o as IList<int>;
            if(selectIds != null) {
                for(int i = 0; i < selectIds.Count; i++) {
                    var item = FindItem(selectIds[i], rootItem) as AssetListItem;
                    item.FixAndReimport(soAssetPostprocessor);
                }

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                Repaint();
            }
        }

        public void SetFolder(PostprocessorAssetType assetType, string path) {
            if(this.assetType != assetType || this.folder != path) {
                this.assetType = assetType;
                this.folder = path;
                var guid = postprocessorFolder.Get(this.assetType, this.folder);
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                soAssetPostprocessor = AssetDatabase.LoadAssetAtPath<SoAssetPostprocessor>(assetPath);
                Refresh();
            }
        }

        public void Refresh() {
            listItems.Clear();
            if(!string.IsNullOrEmpty(this.folder)) {
                var guids = AssetDatabase.FindAssets(Helper.GetAssetSearchFilterByAssetType(this.assetType), new string[] {
                    this.folder
                });
                if(guids != null) {
                    var index = 0;
                    var directories = new List<string>();
                    List<Task> tasks = new List<Task>();
                    var folders = postprocessorFolder.GetPaths(this.assetType);
                    for(int i = 0; i < guids.Length; i++) {
                        var path = AssetDatabase.GUIDToAssetPath(guids[index]);
                        tasks.Add(new Task(guids[i], this.folder, this.assetType, folders));
                    }

                    foreach(var task in tasks) {
                        ThreadPool.QueueUserWorkItem(x => { task.Begin(); });
                    }

                    var completeCount = 0;
                    var taskTotal = tasks.Count;
                    while(completeCount < taskTotal) {
                        completeCount = 0;
                        foreach(var task in tasks) {
                            completeCount += task.IsDone ? 1 : 0;
                        }
                        EditorUtility.DisplayProgressBar("", StringUtility.Contact(completeCount, "/", taskTotal), completeCount / (float)taskTotal);
                    }

                    foreach(var task in tasks) {
                        if(task.Item != null) {
                            var item = task.Item;
                            //item.VerifyImporterSetting(soAssetPostprocessor);
                            listItems.Add(task.Item);
                        }
                    }

                    EditorUtility.ClearProgressBar();
                }
            }

            Reload();
        }

        public void SoPostprocessorChanged(string guid) {
            for(int i = 0; i < rootItem.children.Count; i++) {
                var item = rootItem.children[i] as AssetListItem;
                item.VerifyImporterSetting(soAssetPostprocessor);
            }
        }

        public bool IsAnyOfAssetDirty() {
            for(int i = 0; i < rootItem.children.Count; i++) {
                var item = rootItem.children[i] as AssetListItem;
                if(item.IsDirty) {
                    return true;
                }
            }

            return false;
        }

        public void FixAllDirty() {
            for(int i = 0; i < rootItem.children.Count; i++) {
                var item = rootItem.children[i] as AssetListItem;
                if(item.IsDirty) {
                    item.FixAndReimport(soAssetPostprocessor);
                }
            }
        }

        public class Task {
            public bool IsDone = false;
            public AssetListItem Item;

            private readonly string guid;
            private readonly string path;
            private readonly string folder;
            private readonly List<string> folders;
            private readonly PostprocessorAssetType assetType;

            public Task(string guid, string folder, PostprocessorAssetType assetType, List<string> folders) {
                this.guid = guid;
                this.path = AssetDatabase.GUIDToAssetPath(this.guid);
                this.folder = folder;
                this.assetType = assetType;
                this.folders = folders;
            }

            private List<string> GetParentDirectories(string rootPath, string path) {
                var directories = new List<string>();
                path = Path.GetDirectoryName(path);
                while(path.Length > rootPath.Length) {
                    path = path.Replace('\\', '/');
                    directories.Add(path);
                    path = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal));
                }

                return directories;
            }

            public void Begin() {
                var directories = GetParentDirectories(folder, path);
                var contains = false;
                for(int i = 0; i < directories.Count; i++) {
                    if(folders.Contains(directories[i])) {
                        contains = true;
                        break;
                    }
                }

                if(directories.Count == 0 || !contains) {
                    Item = AssetListItem.Get(assetType, path, 0, Path.GetFileNameWithoutExtension(path));
                }

                IsDone = true;
            }
        }
    }
}