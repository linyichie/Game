using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = System.Object;

namespace Funny.AssetPostprocessor {
    public class AssetPostprocessorConfigTab : TreeView {
        private readonly List<string> paths = new List<string>();
        private readonly SoAssetPostprocessorUtils soAssetPostprocessorUtils;
        
        private PostprocessorAssetType assetType = (PostprocessorAssetType) (-1);
        private Styles styles;
        private string folderPath;
        private string postprocessorConfigGuid = string.Empty;
        private bool contextOnItem = false;

        public event Action<string> DeleteSoAssetPostprocessor;
        public event Action<string> OnChanged;

        class Styles {
            public readonly GUIStyle itemSelectedStyle;
            public readonly GUIStyle itemNormalStyle;

            public Styles() {
                itemNormalStyle = new GUIStyle() {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Bold,
                };
                itemSelectedStyle = new GUIStyle(itemNormalStyle);
                itemSelectedStyle.normal.textColor = Color.white;
            }
        }

        public static AssetPostprocessorConfigTab Get() {
            var treeView = new AssetPostprocessorConfigTab(new TreeViewState());
            treeView.Reload();
            return treeView;
        }

        private AssetPostprocessorConfigTab(TreeViewState state) : base(state) {
            soAssetPostprocessorUtils = SoAssetPostprocessorUtils.GetSoAssetPostprocessorUtils();
            showAlternatingRowBackgrounds = true;
            rowHeight = 25;
        }

        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem(-1, -1);
            var defaultSo = SoAssetPostprocessor.GetDefault(this.assetType);
            var defaultGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(defaultSo));
            root.children = new List<TreeViewItem>();
            foreach (var path in paths) {
                var item = new AssetPostprocessorConfigItem(path, 0, Path.GetFileNameWithoutExtension(path), defaultGuid);
                root.AddChild(item);
            }
                        
            root.children.Sort((lhs, rhs) => {
                var lhsItem = lhs as AssetPostprocessorConfigItem;
                var rhsItem = rhs as AssetPostprocessorConfigItem;
                return -lhsItem.IsDefault.CompareTo(rhsItem.IsDefault);
            });

            return root;
        }

        public override void OnGUI(Rect pos) {
            styles = styles ?? new Styles();
            GUI.Box(pos, string.Empty);
            base.OnGUI(new Rect(pos.x + 1, pos.y + 1, pos.width - 2, pos.height - 2));
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && pos.Contains(Event.current.mousePosition)) {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }

        protected override void RowGUI(RowGUIArgs args) {
            var cellRect = args.rowRect;
            var item = args.item as AssetPostprocessorConfigItem;
            var iconRect = new Rect(cellRect.x + 5, cellRect.y + 5, cellRect.height - 10, cellRect.height - 10);

            var style = styles.itemNormalStyle;

            if (args.selected) {
                style = styles.itemSelectedStyle;
            }

            using (new EditorGUI.DisabledScope(item.IsDefault)) {
                var labelRect = new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width, cellRect.height);
                GUI.Label(labelRect, args.isRenaming ? string.Empty : item.displayName, style);
                if (item.Guid == this.postprocessorConfigGuid) {
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 5, cellRect.height), Color.yellow);
                }
            }
        }

        protected override void ContextClicked() {
            if (contextOnItem) {
                contextOnItem = false;
                return;
            }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Create"), false, OnCreate, null);

            if (menu.GetItemCount() > 0) {
                menu.ShowAsContext();
            }
        }

        protected override void ContextClickedItem(int id) {
            contextOnItem = true;

            var selectIds = GetSelection();

            if (selectIds.Count > 0) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Duplicate"), false, OnDuplicate, selectIds);

                var item = FindItem(selectIds[0], rootItem) as AssetPostprocessorConfigItem;
                if (!string.IsNullOrEmpty(postprocessorConfigGuid)) {
                    if(item.Guid == postprocessorConfigGuid) {
                        menu.AddItem(new GUIContent("Selected"), false, null);
                    } else {
                        menu.AddItem(new GUIContent("Select"), false, OnChangeConfig, selectIds);
                    }
                }
                if (!item.IsDefault) {
                    menu.AddItem(new GUIContent("Delete"), false, OnDeleteConfig, selectIds);
                    menu.AddItem(new GUIContent("Rename"), false, OnRename, selectIds);
                }

                if (menu.GetItemCount() > 0) {
                    menu.ShowAsContext();
                }
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item) {
            return false;
        }

        protected override void DoubleClickedItem(int id) {
            var item = FindItem(id, rootItem) as AssetPostprocessorConfigItem;
            if (item != null) {
                var o = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item.Path);
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }

        protected override bool CanRename(TreeViewItem item) {
            var configItem = item as AssetPostprocessorConfigItem;
            return !configItem.IsDefault;
        }

        protected override void RenameEnded(RenameEndedArgs args) {
            base.RenameEnded(args);
            if (args.newName.Length > 0 && args.newName != args.originalName) {
                args.acceptedRename = true;
                var items = GetRows();
                if (items != null) {
                    for (int i = 0; i < items.Count; i++) {
                        var item = items[i] as AssetPostprocessorConfigItem;
                        if (item.displayName == args.newName) {
                            args.acceptedRename = false;
                            break;
                        }
                    }
                }

                if (args.acceptedRename) {
                    var item = FindItem(args.itemID, rootItem) as AssetPostprocessorConfigItem;
                    AssetDatabase.RenameAsset(item.Path, args.newName);
                    AssetDatabase.Refresh();
                    Refresh();
                }
            } else {
                args.acceptedRename = false;
            }
        }

        private void OnDuplicate(object o) {
            var selectIds = o as IList<int>;
            if (selectIds != null && selectIds.Count == 1) {
                var item = FindItem(selectIds[0], rootItem) as AssetPostprocessorConfigItem;
                var newPath = AssetDatabase.GenerateUniqueAssetPath(item.Path);
                if (newPath.Length != 0) {
                    var success = AssetDatabase.CopyAsset(item.Path, newPath);
                    if (success) {
                        AssetDatabase.SaveAssets();
                        paths.Add(newPath);
                        Reload();
                    }
                }
            }
        }

        private void OnCreate(object o) {
            var so = SoAssetPostprocessor.Create(this.assetType);
            var path = AssetDatabase.GetAssetPath(so);
            var hashCode = path.GetHashCode();
            paths.Add(path);
            Reload();
            BeginRename(FindItem(hashCode, rootItem), 0.25f);
        }

        private void OnRename(object o) {
            var selectIds = o as IList<int>;
            if (selectIds != null && selectIds.Count > 0) {
                BeginRename(FindItem(selectIds[0], rootItem));
            }
        }

        private void OnChangeConfig(object o) {
            var selectIds = o as IList<int>;
            if (selectIds != null && selectIds.Count == 1) {
                var item = FindItem(selectIds[0], rootItem) as AssetPostprocessorConfigItem;
                soAssetPostprocessorUtils.Set(this.assetType, folderPath, item.Guid);
                postprocessorConfigGuid = item.Guid;
                OnChanged?.Invoke(postprocessorConfigGuid);
            }
        }

        private void OnDeleteConfig(object @object) {
            var selectIds = @object as IList<int>;
            if (selectIds != null && selectIds.Count == 1) {
                var item = FindItem(selectIds[0], rootItem) as AssetPostprocessorConfigItem;
                if (postprocessorConfigGuid == item.Guid) {
                    var defaultConfig = SoAssetPostprocessor.GetDefault(this.assetType);
                    var path = AssetDatabase.GetAssetPath(defaultConfig);
                    var guid = AssetDatabase.AssetPathToGUID(path);
                    soAssetPostprocessorUtils.Set(this.assetType, this.folderPath, guid);
                    postprocessorConfigGuid = guid;
                }

                paths.Remove(item.Path);
                DeleteSoAssetPostprocessor?.Invoke(item.Guid);
                AssetDatabase.DeleteAsset(item.Path);
                AssetDatabase.Refresh();
            }

            Reload();
        }

        public void SetAssetType(PostprocessorAssetType assetType) {
            if (this.assetType != assetType) {
                this.assetType = assetType;
                Refresh();
            }
        }

        public void SetPostprocessorFolder(string folderPath) {
            if (this.folderPath != folderPath) {
                this.folderPath = folderPath;
                postprocessorConfigGuid = soAssetPostprocessorUtils.Get(this.assetType, this.folderPath);
                if (!string.IsNullOrEmpty(postprocessorConfigGuid)) {
                    var path = AssetDatabase.GUIDToAssetPath(postprocessorConfigGuid);
                    var id = path.GetHashCode();
                    SetSelection(new List<int>() {id});
                }

                Repaint();
            }
        }

        public void Refresh() {
            paths.Clear();
            var guids = AssetDatabase.FindAssets("", new string[] {
                Helper.GetSoAssetPostprocessorFolder(this.assetType),
            });
            if (guids != null) {
                foreach (var guid in guids) {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    paths.Add(path);
                }
            }

            Reload();
        }

        public string GetSelectSoPostprocessorGuid() {
            var selectIds = GetSelection();
            if (selectIds != null && selectIds.Count > 0) {
                var item = FindItem(selectIds[0], rootItem) as AssetPostprocessorConfigItem;
                if (item != null) {
                    return item.Guid;
                }
            }

            return string.Empty;
        }
    }

    public class AssetPostprocessorConfigItem : TreeViewItem {
        public readonly string Guid;
        public readonly string Path;
        public readonly bool IsDefault;

        public AssetPostprocessorConfigItem(string path, int depth, string name, string defaultGuid) : base(path.GetHashCode(), depth, name) {
            this.Guid = AssetDatabase.AssetPathToGUID(path);
            this.Path = path;
            IsDefault = this.Guid == defaultGuid;
        }
    }
}