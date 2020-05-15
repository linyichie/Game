using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[Serializable]
public class LuaInjection {
    [SerializeField] private string name;
    [SerializeField] private GameObject gameObject;
    [SerializeField] private string[] componentNames;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LuaInjection))]
public class LuaInjectionDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        base.OnGUI(position, property, label);
    }
}
#endif