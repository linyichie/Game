using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReorderableAttribute))]
public class ReorderableAttributeDrawer : PropertyDrawer {
    private ReorderableList reorderableList;

    private ReorderableList GetReorderableList(SerializedProperty property) {
        if(reorderableList == null) {
            reorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true);
            reorderableList.drawElementCallback = (rect, index, active, focused) => {
                rect.width -= 40;
                rect.x += 20;
                EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index), true);
            };
        }

        return reorderableList;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var path = property.propertyPath;
        path = property.propertyPath.Substring(0, path.LastIndexOf('.'));
        var arrayProperty = property.serializedObject.FindProperty(path);
        if(arrayProperty.isArray) {
            var reorderableList = GetReorderableList(arrayProperty);
            var height = 100f;
            for(var i = 0; i < arrayProperty.arraySize; i++) {
                height = Mathf.Max(height, EditorGUI.GetPropertyHeight(arrayProperty.GetArrayElementAtIndex(i)));
            }

            reorderableList.elementHeight = height;
            reorderableList.DoList(position);
        } else {
            base.OnGUI(position, property, label);
        }
    }
}