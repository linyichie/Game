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
    public class AssetImporterConfigTab : TreeView {
        private static readonly string spriteAtlasConfigPath = "Assets/Editor/AssetPostprocessor/Config/SpriteAtlas";

        private List<string> paths = new List<string>();
        private string importType = string.Empty;

        public event Action SelectChanged;

        public static AssetImporterConfigTab Get() {
            var treeView = new AssetImporterConfigTab(new TreeViewState());
            treeView.Reload();
            treeView.showAlternatingRowBackgrounds = true;
            return treeView;
        }

        public AssetImporterConfigTab(TreeViewState state) : base(state) {
        }

        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();
            foreach (var path in paths) {
                root.AddChild(new AssetImporterConfigTreeItem(path, 0, Path.GetFileNameWithoutExtension(path)));
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
                var item = FindItem(selectedIds[0], rootItem) as AssetImporterConfigTreeItem;
                return item.Path;
            }

            return string.Empty;
        }

        public void SetImportType(string importType) {
            if (this.importType != importType) {
                paths.Clear();
                this.importType = importType;
                var guids = AssetDatabase.FindAssets("", new string[] {
                    spriteAtlasConfigPath
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

    public class AssetImporterConfigTreeItem : TreeViewItem {
        public string Path;

        public AssetImporterConfigTreeItem(string path, int depth, string name) : base(path.GetHashCode(), depth, name) {
            this.Path = path;
        }
    }
}