using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Window))]
public class WindowInspector : Editor {
    private SerializedProperty windowConfigProperty;

    private void OnEnable() {
        windowConfigProperty = serializedObject.FindProperty("windowConfig");
    }

    public override void OnInspectorGUI() {
        if(windowConfigProperty.objectReferenceValue == null) {
            if(GUILayout.Button("Create Window Config")) { }
        }

        base.OnInspectorGUI();
    }
}