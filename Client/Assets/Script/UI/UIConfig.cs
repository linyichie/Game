using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class UIConfig : ScriptableObject {
#if UNITY_EDITOR
    private const string path = "Assets/AddressableAssets/Config/ScriptableObjects/UI/UIConfig.asset";
    [MenuItem("Funny/UI/Generate UIConfig")]
    static void Create() {
        if(!File.Exists(path)) {
            var so = ScriptableObject.CreateInstance<UIConfig>();
            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.Refresh();
            Selection.activeObject = so;
        }
    }
#endif

    [SerializeField] private DictionaryWindowLayerOrder windowLayerOrders;

    [Serializable]
    public class DictionaryWindowLayerOrder : SerializableDictionary<WindowLayer, int> { }
}