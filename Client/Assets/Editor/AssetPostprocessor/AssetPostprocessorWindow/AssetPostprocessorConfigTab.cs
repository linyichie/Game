using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.Graphs;
using UnityEditor.IMGUI.Controls;
using UnityEditor.VersionControl;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class AssetPostprocessorConfigTab : TreeView {
        private readonly List<string> paths = new List<string>();
        private AssetPostprocessorHelper.PostprocessorAssetType assetType = (AssetPostprocessorHelper.PostprocessorAssetType)(-1);
        private readonly SoAssetPostprocessorFolder soAssetPostprocessorFolder;
        private Styles styles;
        private string folderPath;
        private string postprocessorConfigGuid = string.Empty;

        public event Action SelectChanged;

        class Styles {
            public readonly Texture checkIcon;
            public readonly Texture toggleIcon;

            public Styles() {
                checkIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/AssetPostprocessor/Texture/check.png");
                toggleIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/AssetPostprocessor/Texture/toggle.png");
            }
        }

        public static AssetPostprocessorConfigTab Get() {
            var treeView = new AssetPostprocessorConfigTab(new TreeViewState());
            treeView.Reload();
            treeView.showAlternatingRowBackgrounds = true;
            return treeView;
        }

        private AssetPostprocessorConfigTab(TreeViewState state) : base(state) {
            soAssetPostprocessorFolder = SoAssetPostprocessorFolder.GetSoAssetPostprocessorFolder();
        }

        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();
            foreach(var path in paths) {
                root.AddChild(new AssetPostprocessorConfigItem(path, 0, Path.GetFileNameWithoutExtension(path)));
            }

            return root;
        }

        public override void OnGUI(Rect pos) {
            styles = styles ?? new Styles();
            GUI.Box(pos, string.Empty);
            base.OnGUI(new Rect(pos.x + 1, pos.y + 1, pos.width - 2, pos.height - 2));
            if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && pos.Contains(Event.current.mousePosition)) {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }

        protected override void RowGUI(RowGUIArgs args) {
            var cellRect = args.rowRect;
            var item = args.item as AssetPostprocessorConfigItem;
            var iconRect = new Rect(cellRect.x + 2, cellRect.y + 2, cellRect.height - 4, cellRect.height - 4);
            if(item.Guid == this.postprocessorConfigGuid) {
                GUI.DrawTexture(iconRect, styles.toggleIcon);
                GUI.DrawTexture(iconRect, styles.checkIcon);
            } else {
                if(GUI.Toggle(iconRect, false, styles.toggleIcon)) {
                    
                }
            }

            DefaultGUI.BoldLabel(new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width, cellRect.height), item.displayName, args.selected, args.focused);
        }

        protected override bool CanMultiSelect(TreeViewItem item) {
            return false;
        }

        protected override void SelectionChanged(IList<int> selectedIds) {
            if(selectedIds != null && selectedIds.Count > 0) {
                SelectChanged?.Invoke();
            }
        }

        public void SetAssetType(AssetPostprocessorHelper.PostprocessorAssetType assetType) {
            if(this.assetType != assetType) {
                paths.Clear();
                this.assetType = assetType;
                var guids = AssetDatabase.FindAssets("", new string[] {
                    AssetPostprocessorHelper.GetSoAssetPostprocessorFolder(this.assetType),
                });
                if(guids != null) {
                    foreach(var guid in guids) {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        paths.Add(path);
                    }
                }

                Reload();
            }
        }

        public void SetPostprocessorFolder(string folderPath) {
            if(this.folderPath != folderPath) {
                this.folderPath = folderPath;
                postprocessorConfigGuid = soAssetPostprocessorFolder.Get(this.assetType, this.folderPath);
                Repaint();
            }
        }

        public string GetSelectSoPostprocessorGuid() {
            var selectIds = GetSelection();
            if(selectIds != null && selectIds.Count > 0) {
                var item = FindItem(selectIds[0], rootItem) as AssetPostprocessorConfigItem;
                if(item != null) {
                    return item.Guid;
                }
            }

            return string.Empty;
        }
    }

    public class AssetPostprocessorConfigItem : TreeViewItem {
        public readonly string Guid;

        public AssetPostprocessorConfigItem(string path, int depth, string name) : base(path.GetHashCode(), depth, name) {
            this.Guid = AssetDatabase.AssetPathToGUID(path);
        }
    }
}