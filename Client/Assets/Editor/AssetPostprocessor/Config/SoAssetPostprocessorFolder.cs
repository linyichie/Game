using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class SoAssetPostprocessorFolder : ScriptableObject {
        private static readonly string path = "Assets/Editor/AssetPostprocessor/Config/SoAssetPostprocessorFolder.asset";

        [SerializeField] private List<AssetPostprocessorFolder> assetPostprocessorFolders;

        public void Set(AssetPostprocessorHelper.PostprocessorAssetType assetType, string path, string guid) {
            if (assetPostprocessorFolders == null) {
                assetPostprocessorFolders = new List<AssetPostprocessorFolder>();
            }

            var index = assetPostprocessorFolders.FindIndex((x) => x.path == path);
            if (index >= 0) {
                assetPostprocessorFolders[index] = new AssetPostprocessorFolder() {
                    assetType = assetType,
                    path = path,
                    guid = guid,
                };
            } else {
                assetPostprocessorFolders.Add(new AssetPostprocessorFolder() {
                    assetType = assetType,
                    path = path,
                    guid = guid,
                });
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void Remove(AssetPostprocessorHelper.PostprocessorAssetType assetType, string path) {
            if (assetPostprocessorFolders != null) {
                var index = assetPostprocessorFolders.FindIndex((x) => x.path == path && x.assetType == assetType);
                if (index >= 0) {
                    assetPostprocessorFolders.RemoveAt(index);
                    EditorUtility.SetDirty(this);
                }
            }
        }

        public string Get(AssetPostprocessorHelper.PostprocessorAssetType assetType, string path) {
            if (assetPostprocessorFolders != null) {
                var index = assetPostprocessorFolders.FindIndex((x) => x.path == path && x.assetType == assetType);
                if (index >= 0) {
                    return assetPostprocessorFolders[index].guid;
                }
            }

            return string.Empty;
        }

        public List<string> GetPaths(AssetPostprocessorHelper.PostprocessorAssetType assetType) {
            var query = assetPostprocessorFolders.FindAll((x) => x.assetType == assetType);
            List<string> paths = new List<string>();
            for (int i = 0; i < query.Count; i++) {
                paths.Add(query[i].path);
            }

            return paths;
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
        struct AssetPostprocessorFolder {
            public AssetPostprocessorHelper.PostprocessorAssetType assetType;
            public string path;
            public string guid;
        }
    }
}