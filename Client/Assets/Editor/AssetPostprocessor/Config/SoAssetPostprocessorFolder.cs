using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.WSA;

namespace Funny.AssetPostprocessor {
    public class SoAssetPostprocessorFolder : ScriptableObject {
        private static readonly string path = "Assets/Editor/AssetPostprocessor/Config/SoAssetPostprocessorFolder.asset";

        [SerializeField] private List<AssetPostprocessorFolder> folders;

        public void Set(PostprocessorAssetType assetType, string path, string guid, bool forceUpdate = true) {
            if(folders == null) {
                folders = new List<AssetPostprocessorFolder>();
            }

            var index = folders.FindIndex((x) => x.path == path);
            if(index >= 0) {
                folders[index] = new AssetPostprocessorFolder() {
                    assetType = assetType,
                    path = path,
                    guid = guid,
                };
            } else {
                folders.Add(new AssetPostprocessorFolder() {
                    assetType = assetType,
                    path = path,
                    guid = guid,
                });
            }

            EditorUtility.SetDirty(this);
            if(forceUpdate) {
                AssetDatabase.SaveAssets();
            }
        }

        public void Remove(PostprocessorAssetType assetType, string path) {
            if(folders != null) {
                var index = folders.FindIndex((x) => x.path == path && x.assetType == assetType);
                if(index >= 0) {
                    folders.RemoveAt(index);
                    EditorUtility.SetDirty(this);
                }
            }
        }

        public string Get(PostprocessorAssetType assetType, string path) {
            var list = folders?.FindAll((x) => x.assetType == assetType && path.StartsWith(x.path));
            if(list != null && list.Count > 0) {
                list.Sort((lhs, rhs) => { return -lhs.path.Length.CompareTo(rhs.path.Length); });
                return list[0].guid;
            }

            return string.Empty;
        }

        public List<string> GetPaths(PostprocessorAssetType assetType) {
            var query = folders.FindAll((x) => x.assetType == assetType);
            List<string> paths = new List<string>();
            for(int i = 0; i < query.Count; i++) {
                paths.Add(query[i].path);
            }

            return paths;
        }

        public static void VerifyConfigs() {
            var so = GetSoAssetPostprocessorFolder();
            var folders = so.folders;
            var dirty = false;
            Dictionary<PostprocessorAssetType, string> defaultSoAssetPostprocessors = null;
            List<AssetPostprocessorFolder> lostFolders = null;
            if(folders != null) {
                foreach(var folder in folders) {
                    if(!Directory.Exists(folder.path)) {
                        lostFolders = lostFolders ?? new List<AssetPostprocessorFolder>();
                        lostFolders.Add(folder);
                        dirty = true;
                        continue;
                    }

                    var soPath = AssetDatabase.GUIDToAssetPath(folder.guid);
                    var soAssetPostprocessor = AssetDatabase.LoadAssetAtPath<SoAssetPostprocessor>(soPath);
                    if(soAssetPostprocessor == null) {
                        defaultSoAssetPostprocessors = defaultSoAssetPostprocessors ?? new Dictionary<PostprocessorAssetType, string>();
                        if(!defaultSoAssetPostprocessors.ContainsKey(folder.assetType)) {
                            var defaultSo = SoAssetPostprocessor.GetDefault(folder.assetType);
                            var assetPath = AssetDatabase.GetAssetPath(defaultSo);
                            defaultSoAssetPostprocessors.Add(folder.assetType, AssetDatabase.AssetPathToGUID(assetPath));
                        }

                        folder.guid = defaultSoAssetPostprocessors[folder.assetType];
                        dirty = true;
                    }
                }
            }

            if(lostFolders != null) {
                foreach(var folder in lostFolders) {
                    so.folders.Remove(folder);
                }

                lostFolders.Clear();
            }

            if(dirty) {
                EditorUtility.SetDirty(so);
                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem("Funny/资源导入规范/文件夹规则")]
        static SoAssetPostprocessorFolder Create() {
            var so = AssetDatabase.LoadAssetAtPath<SoAssetPostprocessorFolder>(path);
            if(so == null) {
                so = ScriptableObject.CreateInstance<SoAssetPostprocessorFolder>();
                AssetDatabase.CreateAsset(so, path);
                AssetDatabase.Refresh();
                Selection.activeObject = so;
            }

            return so;
        }

        public static SoAssetPostprocessorFolder GetSoAssetPostprocessorFolder() {
            var so = AssetDatabase.LoadAssetAtPath<SoAssetPostprocessorFolder>(path);
            if(so == null) {
                so = Create();
            }

            return so;
        }

        [Serializable]
        class AssetPostprocessorFolder {
            public PostprocessorAssetType assetType;
            public string path;
            public string guid;
        }
    }

    [CustomEditor(typeof(SoAssetPostprocessorFolder))]
    public class SoAssetPostprocessorFolderInspector : Editor {
        public override void OnInspectorGUI() {
            using(new EditorGUI.DisabledScope(true)) {
                base.OnInspectorGUI();
            }
        }
    }
}