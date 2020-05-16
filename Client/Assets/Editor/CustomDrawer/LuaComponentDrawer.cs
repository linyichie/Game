using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomPropertyDrawer(typeof(LuaComponent))]
public class LuaComponentDrawer : PropertyDrawer {
    static Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    class Styles {
        public static readonly Color errorColor = new Color(Color.red.r, Color.red.g, Color.red.b, 0.35f);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var nameRect = new Rect(position) {
            width = position.width / 2 - 5,
            height = EditorGUIUtility.singleLineHeight
        };
        var luaInjectionRect = new Rect(position) {
            x = position.x + nameRect.width + 10,
            width = position.width / 2 - 5,
            height = EditorGUIUtility.singleLineHeight
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
                luaInjectionProperty.stringValue = LuaInjection.Int.ToString();
            }

            var selected = (LuaInjection)Enum.Parse(typeof(LuaInjection), luaInjectionProperty.stringValue);
            luaInjectionProperty.stringValue = EditorGUI.EnumPopup(luaInjectionRect, selected).ToString();
        }
        if(EditorGUI.EndChangeCheck()) {
            luaInjectionChanged = true;
        }

        var datasProperty = GetLuaDatasProperty(property);
        if(datasProperty.arraySize == 0) {
            datasProperty.arraySize = 1;
        }

        var dataRect = new Rect(position) {
            y = position.y + EditorGUIUtility.singleLineHeight + 5,
            height = EditorGUIUtility.singleLineHeight + 6,
        };

        var arraySizeRect = new Rect(dataRect) {
            x = dataRect.x + dataRect.width - 100 + 3,
            y = dataRect.y + 3,
            width = 80,
            height = dataRect.height - 6,
        };

        var arraySize = EditorGUI.IntField(arraySizeRect, datasProperty.arraySize);
        if(arraySize != datasProperty.arraySize && arraySize > 0 && arraySize < 15) {
            datasProperty.arraySize = arraySize;
        }

        if(arraySize > 3) {
            if(!foldouts.ContainsKey(nameProperty.stringValue)) {
                foldouts[nameProperty.stringValue] = true;
            }

            var foldoutRect = new Rect(dataRect) {
                width = 10,
            };

            foldouts[nameProperty.stringValue] = EditorGUI.Foldout(foldoutRect, foldouts[nameProperty.stringValue], "");
        }

        var displaySize = arraySize;
        if(arraySize > 3 && foldouts.ContainsKey(nameProperty.stringValue) && !foldouts[nameProperty.stringValue]) {
            displaySize = Mathf.Min(1, arraySize);
        }

        for(int i = 0; i < displaySize; i++) {
            var element = datasProperty.GetArrayElementAtIndex(i);
            DrawLuaDataElement(dataRect, element, luaInjectionProperty.stringValue);
            dataRect.y += EditorGUIUtility.singleLineHeight + 6 + 1;
        }

        if(luaInjectionChanged) {
            datasProperty.arraySize = 0;
        }
    }

    private void DrawLuaDataElement(Rect position, SerializedProperty property, string luaInjection) {
        var boxRect = new Rect(position) {
            width = position.width - 100,
        };
        var valueRect = new Rect(boxRect) {
            x = boxRect.x + 3,
            y = boxRect.y + 3,
            width = boxRect.width - 6,
            height = boxRect.height - 6,
        };
        var selected = (LuaInjection)Enum.Parse(typeof(LuaInjection), luaInjection);
        SerializedProperty elementProperty = GetLuaDataElementProperty(property, selected);
        EditorGUI.HelpBox(boxRect, string.Empty, MessageType.None);
        switch(selected) {
            case LuaInjection.Int:
                elementProperty.intValue = EditorGUI.IntField(valueRect, elementProperty.intValue);
                break;
            case LuaInjection.Float:
                elementProperty.floatValue = EditorGUI.FloatField(valueRect, elementProperty.floatValue);
                break;
            case LuaInjection.Vector3:
                elementProperty.vector3Value = EditorGUI.Vector3Field(valueRect, string.Empty, elementProperty.vector3Value);
                break;
            case LuaInjection.Vector2:
                elementProperty.vector2Value = EditorGUI.Vector2Field(valueRect, string.Empty, elementProperty.vector2Value);
                break;
            case LuaInjection.AnimationCurve:
                if(elementProperty.animationCurveValue.keys.Length == 0) {
                    elementProperty.animationCurveValue = AnimationCurve.Linear(0, 0, 1, 1);
                }

                elementProperty.animationCurveValue = EditorGUI.CurveField(valueRect, elementProperty.animationCurveValue);
                break;
            case LuaInjection.Component:
                valueRect.width = valueRect.width / 2;
                var component = EditorGUI.ObjectField(valueRect, elementProperty.objectReferenceValue, typeof(Component), true) as Component;
                if(component != null) {
                    var rect = new Rect(valueRect) {
                        x = valueRect.x + valueRect.width
                    };
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
                }

                break;
            default:
                elementProperty.objectReferenceValue = EditorGUI.ObjectField(valueRect, elementProperty.objectReferenceValue, GetUnityObjectType(luaInjection), true);
                break;
        }

        var addRect = new Rect(position) {
            x = position.x + boxRect.width + 10,
            width = 30,
        };
    }

    private static Type GetUnityObjectType(string luaInjection) {
        var selected = (LuaInjection)Enum.Parse(typeof(LuaInjection), luaInjection);
        switch(selected) {
            case LuaInjection.GameObject:
                return typeof(GameObject);
            case LuaInjection.Transform:
                return typeof(Transform);
            case LuaInjection.Canvas:
                return typeof(Canvas);
            case LuaInjection.RectTransform:
                return typeof(RectTransform);
            case LuaInjection.Image:
                return typeof(Image);
            case LuaInjection.Text:
                return typeof(Text);
            case LuaInjection.CanvasGroup:
                return typeof(CanvasGroup);
            case LuaInjection.ScrollRect:
                return typeof(ScrollRect);
            case LuaInjection.Button:
                return typeof(Button);
            case LuaInjection.RawImage:
                return typeof(RawImage);
            case LuaInjection.InputField:
                return typeof(InputField);
            case LuaInjection.Toggle:
                return typeof(Toggle);
            case LuaInjection.Component:
                return typeof(MonoBehaviour);
        }

        return typeof(UnityEngine.Object);
    }

    public static SerializedProperty GetLuaDatasProperty(SerializedProperty property) {
        var luaInjectionProperty = property.FindPropertyRelative("luaInjection");
        var selected = (LuaInjection)Enum.Parse(typeof(LuaInjection), luaInjectionProperty.stringValue);
        switch(selected) {
            case LuaInjection.Int:
                return property.FindPropertyRelative("intValues");
            case LuaInjection.Float:
                return property.FindPropertyRelative("floatValues");
            case LuaInjection.Vector2:
                return property.FindPropertyRelative("vector2Values");
            case LuaInjection.Vector3:
                return property.FindPropertyRelative("vector3Values");
            case LuaInjection.AnimationCurve:
                return property.FindPropertyRelative("animationCurveValues");
            default:
                return property.FindPropertyRelative("unityObjectValues");
        }

        return property.FindPropertyRelative("luaDatas");
    }

    public static SerializedProperty GetLuaDataElementProperty(SerializedProperty property, LuaInjection luaInjection) {
        return property;
        switch(luaInjection) {
            case LuaInjection.Int:
                return property.FindPropertyRelative("intValue");
            case LuaInjection.Float:
                return property.FindPropertyRelative("floatValue");
            case LuaInjection.Vector2:
                return property.FindPropertyRelative("vector2Value");
            case LuaInjection.Vector3:
                return property.FindPropertyRelative("vector3Value");
            case LuaInjection.AnimationCurve:
                return property.FindPropertyRelative("animationCurve");
            default:
                return property.FindPropertyRelative("unityObject");
        }
    }

    public static void GetCustomComponents(ref List<Component> components, out string[] displayNames) {
        components.RemoveAll((x) => {
            var type = x.GetType();
            return !string.IsNullOrEmpty(type.Namespace) && type.Namespace.Contains("UnityEngine");
        });
        displayNames = new string[components.Count];
        for(int i = 0; i < displayNames.Length; i++) {
            displayNames[i] = components[i].GetType().Name;
        }
    }

    public static float GetHeight(SerializedProperty property) {
        var height = EditorGUIUtility.singleLineHeight;
        var datasProperty = GetLuaDatasProperty(property);
        var nameProperty = property.FindPropertyRelative("name");
        if(datasProperty.arraySize > 3 && foldouts.ContainsKey(nameProperty.stringValue) && !foldouts[nameProperty.stringValue]) {
            height += 5;
            height += (EditorGUIUtility.singleLineHeight + 6);
        } else {
            height += 5;
            height += (datasProperty.arraySize * (EditorGUIUtility.singleLineHeight + 6));
            height += (datasProperty.arraySize - 1) * 1;
        }

        return height;
    }
}