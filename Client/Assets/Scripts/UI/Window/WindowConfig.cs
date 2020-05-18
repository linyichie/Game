using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WindowConfig : ScriptableObject {
#if UNITY_EDITOR
    private const string path = "Assets/AddressableAssets/Config/ScriptableObjects/UI/Windows/WindowName.asset";
    [UnityEditor.MenuItem("Funny/UI/Generate WindowConfig")]
    static void Create() {
        if(!File.Exists(path)) {
            var so = ScriptableObject.CreateInstance<WindowConfig>();
            UnityEditor.AssetDatabase.CreateAsset(so, path);
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.Selection.activeObject = so;
        }
    }
#endif

    [SerializeField] public WindowLayer windowLayer;
}

public enum WindowLayer {
    Base,
    Function,
    Tips,
    System
}