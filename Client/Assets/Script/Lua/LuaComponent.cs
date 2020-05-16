using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public partial class LuaComponent {
    [SerializeField] public string name;
    [SerializeField] public string luaInjection;
    [NonSerialized] public LuaData[] luaDatas;

    public string Injection {
        get {
            if(string.IsNullOrEmpty(luaInjection)) {
                luaInjection = LuaInjection.Int.ToString();
            } else {
                var enumNames = Enum.GetNames(typeof(LuaInjection));
                if(Array.IndexOf(enumNames, luaInjection) == -1) {
                    return null;
                }
            }

            return luaInjection;
        }
    }
}

public class LuaData {
    public int intValue;
    public float floatValue;
    public Vector2 vector2Value;
    public Vector3 vector3Value;
    public AnimationCurve animationCurve;
    public UnityEngine.Object unityObject;
}

public enum LuaInjection {
    Int,
    Float,
    Vector2,
    Vector3,
    GameObject,
    Transform,
    RectTransform,
    Canvas,
    CanvasGroup,
    Image,
    Text,
    Button,
    RawImage,
    InputField,
    ScrollRect,
    Toggle,
    AnimationCurve,
    Component,
}