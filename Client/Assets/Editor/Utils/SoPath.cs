using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SoPath : ScriptableObject {
    [SerializeField] private List<PathConfig> paths;
    private const string Path = "Assets/Editor/SoPath.asset";

    public string this[string key] {
        get {
            if (paths == null) {
                throw new Exception("key not exist : " + key);
            }

            var index = paths.FindIndex((x) => { return x.key == key; });
            if (index == -1) {
                throw new Exception("key not exist : " + key);
            }

            return paths[index].path;
        }
    }

    private static SoPath instance = null;

    public static SoPath Instance {
        get {
            if (instance == null) {
                instance = AssetDatabase.LoadAssetAtPath<SoPath>(Path);
            }

            return instance;
        }
    }

    [MenuItem("Tools/路径配置文件")]
    static void Create() {
        var so = AssetDatabase.LoadAssetAtPath<SoPath>(Path);
        if (so == null) {
            so = ScriptableObject.CreateInstance<SoPath>();
            AssetDatabase.CreateAsset(so, Path);
            AssetDatabase.Refresh();
        }
    }

    [Serializable]
    struct PathConfig {
        public string key;
        public string path;
    }
}