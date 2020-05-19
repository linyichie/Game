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
                luaInjection = LuaInjection.GameObject.ToString();
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
    public AnimationCurve animationCurve;
    public UnityEngine.Object unityObject;
}

public enum LuaInjection {
    GameObject,
    Component,
    AnimationCurve,
}