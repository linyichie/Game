using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.WSA;

namespace LinChunJie.AssetPostprocessor {
    public class SoAssetPostprocessorFolder : ScriptableObject {
        private static readonly string path = "Assets/Editor/AssetPostprocessor/Config/SoAssetPostprocessorFolder.asset";

        [SerializeField] private List<AssetPostprocessorFolder> folders;

        public void Set(AssetPostprocessorHelper.PostprocessorAssetType assetType, string path, string guid) {
            if (folders == null) {
                folders = new List<AssetPostprocessorFolder>();
            }

            var index = folders.FindIndex((x) => x.path == path);
            if (index >= 0) {
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
            AssetDatabase.SaveAssets();
        }

        public void Remove(AssetPostprocessorHelper.PostprocessorAssetType assetType, string path) {
            if (folders != null) {
                var index = folders.FindIndex((x) => x.path == path && x.assetType == assetType);
                if (index >= 0) {
                    folders.RemoveAt(index);
                    EditorUtility.SetDirty(this);
                }
            }
        }

        public string Get(AssetPostprocessorHelper.PostprocessorAssetType assetType, string path) {
            if (folders != null) {
                var index = folders.FindIndex((x) => x.path == path && x.assetType == assetType);
                if (index >= 0) {
                    return folders[index].guid;
                }
            }

            return string.Empty;
        }

        public List<string> GetPaths(AssetPostprocessorHelper.PostprocessorAssetType assetType) {
            var query = folders.FindAll((x) => x.assetType == assetType);
            List<string> paths = new List<string>();
            for (int i = 0; i < query.Count; i++) {
                paths.Add(query[i].path);
            }

            return paths;
        }

        public static void VerifyConfigs() {
            var so = GetSoAssetPostprocessorFolder();
            var folders = so.folders;
            var dirty = false;
            Dictionary<AssetPostprocessorHelper.PostprocessorAssetType, string> defaultSoAssetPostprocessors = null;
            List<AssetPostprocessorFolder> lostFolders = null;
            if (folders != null) {
                foreach (var folder in folders) {
                    if (!Directory.Exists(folder.path)) {
                        lostFolders = lostFolders ?? new List<AssetPostprocessorFolder>();
                        lostFolders.Add(folder);
                        dirty = true;
                        continue;
                    }

                    var soPath = AssetDatabase.GUIDToAssetPath(folder.guid);
                    var soAssetPostprocessor = AssetDatabase.LoadAssetAtPath<SoAssetPostprocessor>(soPath);
                    if (soAssetPostprocessor == null) {
                        defaultSoAssetPostprocessors = defaultSoAssetPostprocessors ?? new Dictionary<AssetPostprocessorHelper.PostprocessorAssetType, string>();
                        if (!defaultSoAssetPostprocessors.ContainsKey(folder.assetType)) {
                            var defaultSo = SoAssetPostprocessor.GetDefault(folder.assetType);
                            var assetPath = AssetDatabase.GetAssetPath(defaultSo);
                            defaultSoAssetPostprocessors.Add(folder.assetType, AssetDatabase.AssetPathToGUID(assetPath));
                        }

                        folder.guid = defaultSoAssetPostprocessors[folder.assetType];
                        dirty = true;
                    }
                }
            }

            if (lostFolders != null) {
                foreach (var folder in lostFolders) {
                    so.folders.Remove(folder);
                }

                lostFolders.Clear();
            }

            if (dirty) {
                EditorUtility.SetDirty(so);
                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem("Tools/资源导入规范/文件夹规则")]
        static SoAssetPostprocessorFolder Create() {
            var so = AssetDatabase.LoadAssetAtPath<SoAssetPostprocessorFolder>(path);
            if (so == null) {
                so = ScriptableObject.CreateInstance<SoAssetPostprocessorFolder>();
                AssetDatabase.CreateAsset(so, path);
                AssetDatabase.Refresh();
                Selection.activeObject = so;
            }

            return so;
        }

        public static SoAssetPostprocessorFolder GetSoAssetPostprocessorFolder() {
            var so = AssetDatabase.LoadAssetAtPath<SoAssetPostprocessorFolder>(path);
            if (so == null) {
                so = Create();
            }

            return so;
        }

        [Serializable]
        class AssetPostprocessorFolder {
            public AssetPostprocessorHelper.PostprocessorAssetType assetType;
            public string path;
            public string guid;
        }
    }
}