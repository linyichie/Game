using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class SerializableDictionaryDrawer<TKey, TValue> : PropertyDrawer {
    static readonly Dictionary<Type, Func<Rect, object, object>> s_DisplayFields = new Dictionary<Type, Func<Rect, object, object>>() {
        {typeof(int), (rect, value) => EditorGUI.IntField(rect, (int)value)},
        {typeof(float), (rect, value) => EditorGUI.FloatField(rect, (float)value)},
        {typeof(string), (rect, value) => EditorGUI.TextField(rect, (string)value)},
        {typeof(bool), (rect, value) => EditorGUI.Toggle(rect, (bool)value)},
        {typeof(Vector2), (rect, value) => EditorGUI.Vector2Field(rect, GUIContent.none, (Vector2)value)},
        {typeof(Vector3), (rect, value) => EditorGUI.Vector3Field(rect, GUIContent.none, (Vector3)value)},
        {typeof(Vector2Int), (rect, value) => EditorGUI.Vector2IntField(rect, GUIContent.none, (Vector2Int)value)},
        {typeof(Vector3Int), (rect, value) => EditorGUI.Vector3IntField(rect, GUIContent.none, (Vector3Int)value)},
        {typeof(Bounds), (rect, value) => EditorGUI.BoundsField(rect, (Bounds)value)},
        {typeof(Rect), (rect, value) => EditorGUI.RectField(rect, (Rect)value)},
    };

    SerializableDictionary<TKey, TValue> dictionary;

    bool IsFoldout = false;

    const float kButtonWidth = 30f;
    const float SingleLineHeight = 17;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        Initialize(property, label);
        if(!IsFoldout) {
            return SingleLineHeight;
        }

        return (dictionary.Count + 1) * SingleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        Initialize(property, label);

        position.height = SingleLineHeight;

        var foldoutRect = position;
        foldoutRect.width -= 2 * kButtonWidth;

        EditorGUI.BeginChangeCheck();
        IsFoldout = EditorGUI.Foldout(foldoutRect, IsFoldout, label, true);
        if(EditorGUI.EndChangeCheck()) {
            EditorPrefs.SetBool(label.text, IsFoldout);
        }

        if(!IsFoldout) {
            return;
        }

        var buttonRect = position;
        buttonRect.x = position.width - kButtonWidth + position.x;
        buttonRect.width = kButtonWidth;

        if(GUI.Button(buttonRect, new GUIContent("+", "Add item"), EditorStyles.miniButton)) {
            AddNewItem();
        }

        buttonRect.x -= kButtonWidth;

        if(GUI.Button(buttonRect, new GUIContent("X", "Clear dictionary"), EditorStyles.miniButtonRight)) {
            ClearDictionary();
        }

        foreach(var item in dictionary) {
            var key = item.Key;
            var value = item.Value;

            position.y += SingleLineHeight;

            var keyRect = position;
            keyRect.width /= 2;
            keyRect.width -= 4;
            EditorGUI.BeginChangeCheck();
            var newKey = DisplayKeyValueField(keyRect, typeof(TKey), key);
            if(EditorGUI.EndChangeCheck()) {
                try {
                    List<TKey> keys = new List<TKey>(dictionary.Keys);
                    List<TValue> values = new List<TValue>(dictionary.Values);
                    dictionary.Clear();
                    for(int i = 0; i < keys.Count; i++) {
                        if(keys[i].Equals(key)) {
                            dictionary.Add(newKey, value);
                        } else {
                            dictionary.Add(keys[i], values[i]);
                        }
                    }
                } catch(Exception e) {
                    Debug.LogError(e.Message);
                }

                break;
            }

            var valueRect = position;
            valueRect.x = position.width / 2 + 15;
            valueRect.width = keyRect.width - kButtonWidth;
            EditorGUI.BeginChangeCheck();
            value = DisplayKeyValueField(valueRect, typeof(TValue), value);
            if(EditorGUI.EndChangeCheck()) {
                dictionary[key] = value;
                break;
            }

            var removeRect = valueRect;
            removeRect.x = valueRect.xMax + 2;
            removeRect.width = kButtonWidth;
            if(GUI.Button(removeRect, new GUIContent("x", "Remove item"), EditorStyles.miniButtonRight)) {
                dictionary.Remove(key);
                break;
            }
        }

        if(GUI.changed) {
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }

    private void Initialize(SerializedProperty property, GUIContent label) {
        if(dictionary == null) {
            var target = property.serializedObject.targetObject;
            dictionary = fieldInfo.GetValue(target) as SerializableDictionary<TKey, TValue>;
            if(dictionary == null) {
                dictionary = new SerializableDictionary<TKey, TValue>();
                fieldInfo.SetValue(target, dictionary);
            }

            IsFoldout = EditorPrefs.GetBool(label.text);
        }
    }

    private static T DisplayKeyValueField<T>(Rect rect, Type type, T value) {
        Func<Rect, object, object> field;
        if(s_DisplayFields.TryGetValue(type, out field))
            return (T)field(rect, value);

        if(type.IsEnum)
            return (T)(object)EditorGUI.EnumPopup(rect, (Enum)(object)value);

        if(typeof(UnityEngine.Object).IsAssignableFrom(type))
            return (T)(object)EditorGUI.ObjectField(rect, (UnityEngine.Object)(object)value, type, true);

        Debug.Log("Type is not supported: " + type);
        return value;
    }

    private void ClearDictionary() {
        dictionary.Clear();
    }

    private void AddNewItem() {
        TKey key;
        if(typeof(TKey) == typeof(string)) {
            key = (TKey)(object)"";
        } else if(typeof(TKey).IsEnum) {
            key = (TKey)(object)-1;
        } else {
            key = default(TKey);
        }

        var value = default(TValue);

        try {
            dictionary.Add(key, value);
        } catch(Exception e) {
            Debug.LogError(e.Message);
        }
    }
}

[CustomPropertyDrawer(typeof(UIConfig.DictionaryWindowLayerOrder))]
public class SerializableDictionaryDrawer1 : SerializableDictionaryDrawer<WindowLayer, int> { }