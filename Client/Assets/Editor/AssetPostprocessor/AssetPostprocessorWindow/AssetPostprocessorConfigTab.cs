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

        private List<string> paths = new List<string>();
        private AssetPostprocessorHelper.PostprocessorAssetType assetType;

        public event Action SelectChanged;

        public static AssetPostprocessorConfigTab Get() {
            var treeView = new AssetPostprocessorConfigTab(new TreeViewState());
            treeView.Reload();
            treeView.showAlternatingRowBackgrounds = true;
            return treeView;
        }

        public AssetPostprocessorConfigTab(TreeViewState state) : base(state) {
        }

        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();
            foreach (var path in paths) {
                root.AddChild(new AssetPostprocessorConfigItem(path, 0, Path.GetFileNameWithoutExtension(path)));
            }

            return root;
        }

        public override void OnGUI(Rect pos) {
            GUI.Box(pos, string.Empty);
            base.OnGUI(new Rect(pos.x + 1, pos.y + 1, pos.width - 2, pos.height - 2));
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && pos.Contains(Event.current.mousePosition)) {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }

        protected override void RowGUI(RowGUIArgs args) {
            var cellRect = args.rowRect;
            var item = args.item;
            var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
            DefaultGUI.BoldLabel(new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width, cellRect.height), item.displayName, args.selected, args.focused);
        }

        protected override bool CanMultiSelect(TreeViewItem item) {
            return false;
        }

        protected override void SelectionChanged(IList<int> selectedIds) {
            if (selectedIds != null && selectedIds.Count > 0) {
                SelectChanged?.Invoke();
            }
        }

        public string GetSelectSoImporterPath() {
            var selectedIds = GetSelection();
            if (selectedIds != null && selectedIds.Count > 0) {
                var item = FindItem(selectedIds[0], rootItem) as AssetPostprocessorConfigItem;
                return item.Path;
            }

            return string.Empty;
        }

        public void SetAssetType(AssetPostprocessorHelper.PostprocessorAssetType assetType) {
            if (this.assetType != assetType) {
                paths.Clear();
                this.assetType = assetType;
                var guids = AssetDatabase.FindAssets("", new string[] {
                    AssetPostprocessorHelper.GetSoAssetPostprocessorFolder(this.assetType),
                });
                if (guids != null) {
                    for (int i = 0; i < guids.Length; i++) {
                        var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        paths.Add(path);
                    }
                }

                Reload();
            }
        }
    }

    public class AssetPostprocessorConfigItem : TreeViewItem {
        public string Path;

        public AssetPostprocessorConfigItem(string path, int depth, string name) : base(path.GetHashCode(), depth, name) {
            this.Path = path;
        }
    }
}