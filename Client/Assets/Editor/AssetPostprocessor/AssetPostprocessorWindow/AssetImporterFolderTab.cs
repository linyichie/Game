using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class AssetImporterFolderTab : TreeView {
        private string importType = string.Empty;
        private List<string> paths = new List<string>();

        private static readonly string spriteAtlasFolderPath = "Assets/AddressableAssets/UI/Atlas";

        public static AssetImporterFolderTab Get() {
            var treeView = new AssetImporterFolderTab(new TreeViewState());
            treeView.Reload();
            treeView.showAlternatingRowBackgrounds = true;
            return treeView;
        }

        public AssetImporterFolderTab(TreeViewState state) : base(state) {
        }

        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();
            foreach (var path in paths) {
                root.AddChild(new AssetImporterFolderTreeItem(path, 0, Path.GetFileNameWithoutExtension(path)));
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

        public void SetImportType(string importType) {
            if (this.importType != importType) {
                paths.Clear();
                this.importType = importType;
                var guids = AssetDatabase.FindAssets("t:folder", new string[] {
                    spriteAtlasFolderPath,
                });
                if (guids != null) {
                    for (int i = 0; i < guids.Length; i++) {
                        var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        //paths.Add(path);
                    }
                }

                Reload();
            }
        }

        public void CheckDragFolder() {
            
        }
    }

    public class AssetImporterFolderTreeItem : TreeViewItem {
        public AssetImporterFolderTreeItem(string path, int depth, string displayName) : base(path.GetHashCode(), depth, displayName) {
            
        }
    }
}