using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public partial class LuaComponent : ISerializationCallbackReceiver {
    [SerializeField] private int[] intValues;
    [SerializeField] private float[] floatValues;
    [SerializeField] private Vector2[] vector2Values;
    [SerializeField] private Vector3[] vector3Values;
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
            case LuaInjection.Int:
                intValues = new int[luaDatas.Length];
                for(int i = 0; i < intValues.Length; i++) {
                    intValues[i] = luaDatas[i].intValue;
                }

                break;
            case LuaInjection.Float:
                floatValues = new float[luaDatas.Length];
                for(int i = 0; i < floatValues.Length; i++) {
                    floatValues[i] = luaDatas[i].floatValue;
                }

                break;
            case LuaInjection.Vector2:
                vector2Values = new Vector2[luaDatas.Length];
                for(int i = 0; i < vector2Values.Length; i++) {
                    vector2Values[i] = luaDatas[i].vector2Value;
                }

                break;
            case LuaInjection.Vector3:
                vector3Values = new Vector3[luaDatas.Length];
                for(int i = 0; i < vector3Values.Length; i++) {
                    vector3Values[i] = luaDatas[i].vector3Value;
                }

                break;
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
        intValues = null;
        floatValues = null;
        vector2Values = null;
        vector3Values = null;
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
            case LuaInjection.Int:
                if(intValues != null) {
                    luaDatas = new LuaData[intValues.Length];
                    for(int i = 0; i < intValues.Length; i++) {
                        luaDatas[i] = new LuaData();
                        luaDatas[i].intValue = intValues[i];
                    }
                }
                break;
            case LuaInjection.Float:
                if(floatValues != null) {
                    luaDatas = new LuaData[floatValues.Length];
                    for(int i = 0; i < floatValues.Length; i++) {
                        luaDatas[i] = new LuaData();
                        luaDatas[i].floatValue = floatValues[i];
                    }
                }
                break;
            case LuaInjection.Vector2:
                if(vector2Values != null) {
                    luaDatas = new LuaData[vector2Values.Length];
                    for(int i = 0; i < vector2Values.Length; i++) {
                        luaDatas[i] = new LuaData();
                        luaDatas[i].vector2Value = vector2Values[i];
                    }
                }
                break;
            case LuaInjection.Vector3:
                if(vector3Values != null) {
                    luaDatas = new LuaData[vector3Values.Length];
                    for(int i = 0; i < vector3Values.Length; i++) {
                        luaDatas[i] = new LuaData();
                        luaDatas[i].vector3Value = vector3Values[i];
                    }
                }
                break;
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