using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SoAssetImporterFolder : ScriptableObject {
    [SerializeField] private List<FolderRoot> folderRoots;
    [SerializeField] private List<FolderSoImporter> folderSoImporters;

    public void Set(string path, string guid) {
        if (folderSoImporters == null) {
            folderSoImporters = new List<FolderSoImporter>();
        }

        var index = folderSoImporters.FindIndex((x) => { return x.path == path; });
        if (index >= 0) {
            folderSoImporters[index] = new FolderSoImporter() {
                path = path,
                guid = guid,
            };
        } else {
            folderSoImporters.Add(new FolderSoImporter() {
                path = path,
                guid = guid,
            });
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public string Get(string path) {
        if (folderSoImporters != null) {
            var index = folderSoImporters.FindIndex((x) => { return x.path == path; });
            if (index >= 0) {
                return folderSoImporters[index].guid;
            }
        }

        return string.Empty;
    }

    [MenuItem("Tools/资源导入规范/文件夹规则")]
    static SoAssetImporterFolder Create() {
        var so = AssetDatabase.LoadAssetAtPath<SoAssetImporterFolder>(SoPath.Instance["AssetImporterFolder"]);
        if (so == null) {
            so = ScriptableObject.CreateInstance<SoAssetImporterFolder>();
            AssetDatabase.CreateAsset(so, SoPath.Instance["AssetImporterFolder"]);
            AssetDatabase.Refresh();
            Selection.activeObject = so;
        }

        return so;
    }

    public static SoAssetImporterFolder GetSoAssetImporterFolder() {
        var so = AssetDatabase.LoadAssetAtPath<SoAssetImporterFolder>(SoPath.Instance["AssetImporterFolder"]);
        if (so == null) {
            so = Create();
        }

        return so;
    }

    [Serializable]
    struct FolderSoImporter {
        public string path;
        public string guid;
    }
    
    [Serializable]
    struct FolderRoot {
        public string importType;
        public string path;
    }
}