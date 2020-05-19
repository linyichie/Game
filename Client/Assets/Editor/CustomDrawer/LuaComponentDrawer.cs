using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

[CustomPropertyDrawer(typeof(LuaComponent))]
public class LuaComponentDrawer : PropertyDrawer {
    static Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    class Styles {
        public static readonly int splitWidth = 3;
        public static readonly Color errorColor = new Color(Color.red.r, Color.red.g, Color.red.b, 0.35f);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var nameRect = new Rect(position) {
            y = position.y + Styles.splitWidth,
            width = position.width * 0.2f,
            height = EditorGUIUtility.singleLineHeight
        };
        var arrayRect = new Rect(nameRect) {
            x = nameRect.xMax + Styles.splitWidth,
            width = position.width * 0.2f,
        };
        var luaInjectionRect = new Rect(nameRect) {
            x = arrayRect.xMax + Styles.splitWidth,
            width = position.width * 0.4f,
        };
        var nameProperty = property.FindPropertyRelative("name");
        nameProperty.stringValue = EditorGUI.TextField(nameRect, nameProperty.stringValue);
        if(string.IsNullOrEmpty(nameProperty.stringValue) || !Regex.IsMatch(nameProperty.stringValue, @"^[a-zA-Z][a-zA-Z0-9]*$")) {
            EditorGUI.DrawRect(nameRect, Styles.errorColor);
        }

        var luaInjectionProperty = property.FindPropertyRelative("luaInjection");
        var luaInjectionChanged = false;
        EditorGUI.BeginChangeCheck();
        {
            if(string.IsNullOrEmpty(luaInjectionProperty.stringValue)) {
                luaInjectionProperty.stringValue = LuaInjection.GameObject.ToString();
            }

            var selected = (LuaInjection)Enum.Parse(typeof(LuaInjection), luaInjectionProperty.stringValue);
            luaInjectionProperty.stringValue = EditorGUI.EnumPopup(luaInjectionRect, selected).ToString();
        }
        if(EditorGUI.EndChangeCheck()) {
            luaInjectionChanged = true;
        }

        var datasProperty = GetLuaDatasProperty(property);
        var arraySize = EditorGUI.IntField(arrayRect, datasProperty.arraySize);
        arraySize = Mathf.Clamp(arraySize, 1, 15);
        if(arraySize != datasProperty.arraySize) {
            datasProperty.arraySize = arraySize;
        }

        var dataHorizontalPercent = 0.8f;
        var dataRect = new Rect(position) {
            y = nameRect.yMax + Styles.splitWidth,
            width = position.width * dataHorizontalPercent + 2 * Styles.splitWidth,
            height = EditorGUIUtility.singleLineHeight
        };

        if(arraySize > 1) {
            if(!foldouts.ContainsKey(nameProperty.stringValue)) {
                foldouts[nameProperty.stringValue] = true;
            }

            var foldoutRect = new Rect(dataRect) {
                width = 10,
            };

            foldouts[nameProperty.stringValue] = EditorGUI.Foldout(foldoutRect, foldouts[nameProperty.stringValue], "");
        }

        var displaySize = arraySize;
        if(arraySize > 1 && foldouts.ContainsKey(nameProperty.stringValue) && !foldouts[nameProperty.stringValue]) {
            displaySize = Mathf.Min(1, arraySize);
        }

        for(int i = 0; i < displaySize; i++) {
            var element = datasProperty.GetArrayElementAtIndex(i);
            DrawLuaDataElement(dataRect, element, luaInjectionProperty.stringValue);
            dataRect.y = dataRect.yMax + Styles.splitWidth;
        }

        if(luaInjectionChanged) {
            datasProperty.arraySize = 0;
        }
    }

    private void DrawLuaDataElement(Rect position, SerializedProperty property, string luaInjection) {
        var selected = (LuaInjection)Enum.Parse(typeof(LuaInjection), luaInjection);
        SerializedProperty elementProperty = GetLuaDataElementProperty(property, selected);
        switch(selected) {
            case LuaInjection.AnimationCurve:
                if(elementProperty.animationCurveValue.keys.Length == 0) {
                    elementProperty.animationCurveValue = AnimationCurve.Linear(0, 0, 1, 1);
                }

                elementProperty.animationCurveValue = EditorGUI.CurveField(position, elementProperty.animationCurveValue);
                break;
            case LuaInjection.Component:
                position.width = position.width / 2;
                var rect = new Rect(position) {
                    x = position.x + position.width + Styles.splitWidth,
                    width = position.width - Styles.splitWidth
                };

                var component = EditorGUI.ObjectField(position, elementProperty.objectReferenceValue, typeof(Component), true) as Component;
                if(component != null) {
                    List<Component> components = new List<Component>();
                    component.GetComponents(components);
                    string[] displayNames;
                    GetCustomComponents(ref components, out displayNames);
                    if(components.Count == 0) {
                        break;
                    }

                    var index = components.IndexOf(component);
                    if(index == -1) {
                        component = components[0];
                        index = 0;
                    }

                    index = EditorGUI.Popup(rect, index, displayNames);
                    if(elementProperty.objectReferenceValue != components[index]) {
                        elementProperty.objectReferenceValue = components[index];
                    }
                } else {
                    elementProperty.objectReferenceValue = null;
                    EditorGUI.Popup(rect, 0, new string[0]);
                }

                break;
            default:
                elementProperty.objectReferenceValue = EditorGUI.ObjectField(position, elementProperty.objectReferenceValue, GetUnityObjectType(luaInjection), true);
                break;
        }
    }

    private static Type GetUnityObjectType(string luaInjection) {
        var selected = (LuaInjection)Enum.Parse(typeof(LuaInjection), luaInjection);
        switch(selected) {
            case LuaInjection.GameObject:
                return typeof(GameObject);
            case LuaInjection.Component:
                return typeof(Component);
        }

        return typeof(UnityEngine.Object);
    }

    public static SerializedProperty GetLuaDatasProperty(SerializedProperty property) {
        var luaInjectionProperty = property.FindPropertyRelative("luaInjection");
        var selected = (LuaInjection)Enum.Parse(typeof(LuaInjection), luaInjectionProperty.stringValue);
        switch(selected) {
            case LuaInjection.AnimationCurve:
                return property.FindPropertyRelative("animationCurveValues");
            default:
                return property.FindPropertyRelative("unityObjectValues");
        }
    }

    public static SerializedProperty GetLuaDataElementProperty(SerializedProperty property, LuaInjection luaInjection) {
        return property;
    }

    public static void GetCustomComponents(ref List<Component> components, out string[] displayNames) {
        //components.RemoveAll((x) => {
        //    var type = x.GetType();
        //    return !string.IsNullOrEmpty(type.Namespace) && type.Namespace.Contains("UnityEngine");
        //});
        displayNames = new string[components.Count];
        for(int i = 0; i < displayNames.Length; i++) {
            displayNames[i] = components[i].GetType().Name;
        }
    }

    public static float GetHeight(SerializedProperty property) {
        var height = EditorGUIUtility.singleLineHeight;
        height += Styles.splitWidth;
        var datasProperty = GetLuaDatasProperty(property);
        var nameProperty = property.FindPropertyRelative("name");

        if(datasProperty.arraySize > 1 && foldouts.ContainsKey(nameProperty.stringValue) && !foldouts[nameProperty.stringValue]) {
            height += EditorGUIUtility.singleLineHeight;
            height += Styles.splitWidth;
        } else {
            height += (datasProperty.arraySize * EditorGUIUtility.singleLineHeight);
            height += (datasProperty.arraySize - 1) * Styles.splitWidth;
            height += Styles.splitWidth;
        }

        return height;
    }
}