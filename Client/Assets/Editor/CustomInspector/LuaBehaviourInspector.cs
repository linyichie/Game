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
    private SerializedProperty luaComponentsProperty;
    private List<string> fieldNames = new List<string>();
    private List<string> repeatNames = new List<string>();
    
    class Styles {
        public static readonly int splitWidth = 3;
        public static readonly Color errorColor = new Color(Color.red.r, Color.red.g, Color.red.b, 0.35f);
    }

    private void OnEnable() {
        luaComponentsProperty = serializedObject.FindProperty("luaComponents");
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
        var nameProperty = element.FindPropertyRelative("name");
        if(!string.IsNullOrEmpty(nameProperty.stringValue)) {
            if(repeatNames.Contains(nameProperty.stringValue)) {
                var nameRect = new Rect(rect) {
                    y = rect.y + Styles.splitWidth,
                    width = rect.width * 0.2f,
                    height = EditorGUIUtility.singleLineHeight
                };
                EditorGUI.DrawRect(nameRect, Styles.errorColor);
            }
        }
    }

    private void DrawHeaderCallback(Rect rect) {
        GUI.Label(rect, "Lua Components");
    }

    public override void OnInspectorGUI() {
        GUILayout.Space(3);
        if(repeatNames.Count > 0) {
            var oldFontStyle = EditorStyles.helpBox.fontStyle;
            EditorStyles.helpBox.fontStyle = FontStyle.Bold;
            EditorGUILayout.HelpBox(StringUtil.Contact("There are duplicate field names :\n", string.Join("\n", repeatNames)), MessageType.Error);
            EditorStyles.helpBox.fontStyle = oldFontStyle;
        }
        serializedObject.Update();
        var height = Mathf.Min(800, reorderableList.GetHeight());
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(height));
        reorderableList.DoLayoutList();
        EditorGUILayout.EndScrollView();
        serializedObject.ApplyModifiedProperties();

        if(GUI.changed) {
            fieldNames.Clear();
            repeatNames.Clear();
            for(int i = 0; i < luaComponentsProperty.arraySize; i++) {
                var element = reorderableList.serializedProperty.GetArrayElementAtIndex(i);
                var nameProperty = element.FindPropertyRelative("name");
                if(!string.IsNullOrEmpty(nameProperty.stringValue)) {
                    if(fieldNames.Contains(nameProperty.stringValue)) {
                        if(!repeatNames.Contains(nameProperty.stringValue)) {
                            repeatNames.Add(nameProperty.stringValue);
                        }
                    } else {
                        fieldNames.Add(nameProperty.stringValue);
                    }
                }
            }
        }
    }
}