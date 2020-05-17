using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class LuaComponent : ISerializationCallbackReceiver {
    [SerializeField] private AnimationCurve[] animationCurveValues;
    [SerializeField] private UnityEngine.Object[] unityObjectValues;

    public void OnBeforeSerialize() {
        ClearSerializeValues();
        if(luaDatas == null) {
            return;
        }

        var luaInjection = Injection;
        if(string.IsNullOrEmpty(luaInjection)) {
            return;
        }
        var selected = Enum.Parse(typeof(LuaInjection), luaInjection);
        switch(selected) {
            case LuaInjection.AnimationCurve:
                animationCurveValues = new AnimationCurve[luaDatas.Length];
                for(int i = 0; i < animationCurveValues.Length; i++) {
                    animationCurveValues[i] = luaDatas[i].animationCurve;
                }

                break;
            default:
                unityObjectValues = new UnityEngine.Object[luaDatas.Length];
                for(int i = 0; i < unityObjectValues.Length; i++) {
                    unityObjectValues[i] = luaDatas[i].unityObject;
                }

                break;
        }
    }

    private void ClearSerializeValues() {
        animationCurveValues = null;
        unityObjectValues = null;
    }

    public void OnAfterDeserialize() {
        luaDatas = null;
        var luaInjection = Injection;
        if(string.IsNullOrEmpty(luaInjection)) {
            return;
        }
        var selected = Enum.Parse(typeof(LuaInjection), luaInjection);
        switch(selected) {
            case LuaInjection.AnimationCurve:
                if(animationCurveValues != null) {
                    luaDatas = new LuaData[animationCurveValues.Length];
                    for(int i = 0; i < animationCurveValues.Length; i++) {
                        luaDatas[i] = new LuaData();
                        luaDatas[i].animationCurve = animationCurveValues[i];
                    }
                }
                break;
            default:
                if(unityObjectValues != null) {
                    luaDatas = new LuaData[unityObjectValues.Length];
                    for(int i = 0; i < unityObjectValues.Length; i++) {
                        luaDatas[i] = new LuaData();
                        luaDatas[i].unityObject = unityObjectValues[i];
                    }
                }

                break;
        }
    }
}