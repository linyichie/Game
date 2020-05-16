using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(LuaBehaviour))]
public class LuaBehaviourInspector : Editor {
    private ReorderableList reorderableList;
    private Vector2 scrollPosition;

    private void OnEnable() {
        var luaComponentsProperty = serializedObject.FindProperty("luaComponents");
        reorderableList = new ReorderableList(serializedObject, luaComponentsProperty, true, true, true, true);
        reorderableList.drawHeaderCallback = DrawHeaderCallback;
        reorderableList.drawElementCallback = DrawElementCallback;
        reorderableList.elementHeightCallback = ElementHeightCallback;
    }

    private float ElementHeightCallback(int index) {
        var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
        return LuaComponentDrawer.GetHeight(element) + 5;
    }

    private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused) {
        var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
        EditorGUI.PropertyField(rect, element);
    }

    private void DrawHeaderCallback(Rect rect) {
        GUI.Label(rect, "Lua Components");
    }

    public override void OnInspectorGUI() {
        GUILayout.Space(10);
        serializedObject.Update();
        var height = Mathf.Min(500,reorderableList.GetHeight());
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(height));
        reorderableList.DoLayoutList();
        EditorGUILayout.EndScrollView();
        serializedObject.ApplyModifiedProperties();
    }
}