using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class LuaBehaviour : MonoBehaviour {
    [SerializeField] private LuaComponent[] luaComponents;

    public void StartLuaInjection(LuaTable luaTable) {
        if(luaComponents != null) {
            foreach(var luaComponent in luaComponents) {
                if(luaComponent.luaDatas == null || luaComponent.luaDatas.Length == 0) {
                    continue;
                }

                if(string.IsNullOrEmpty(luaComponent.luaInjection) || string.IsNullOrEmpty(luaComponent.Injection)) {
                    continue;
                }

                var luaInjection = (LuaInjection)Enum.Parse(typeof(LuaInjection), luaComponent.Injection);
                if(luaComponent.luaDatas.Length == 1) {
                    SetLuaTable(luaTable, luaComponent.name, luaComponent.luaDatas[0], luaInjection);
                } else {
                    var table = LuaUtility.luaEnv.NewTable();
                    for(int i = 0; i < luaComponent.luaDatas.Length; i++) {
                        SetLuaTable(table, i + 1, luaComponent.luaDatas[i], luaInjection);
                    }
                    luaTable.Set(luaComponent.name, table);
                }
            }
        }
    }

    private void SetLuaTable<TKey>(LuaTable luaTable, TKey fieldName, LuaData luaData, LuaInjection luaInjection) {
        switch(luaInjection) {
            case LuaInjection.AnimationCurve:
                if(luaData.animationCurve == null) {
                    Debug.LogErrorFormat("The component is not assigned : {0} Type : {1}", fieldName, luaInjection);
                }
                luaTable.Set(fieldName, luaData.animationCurve);
                break;
            case LuaInjection.Component:
                if(luaData.unityObject == null) {
                    Debug.LogErrorFormat("The component is not assigned : {0} Type : {1}", fieldName, luaInjection);
                }
                luaTable.Set(fieldName, luaData.unityObject);
                break;
            default:
                if(luaData.unityObject == null) {
                    Debug.LogErrorFormat("The component is not assigned : {0} Type : {1}", fieldName, luaInjection);
                }
                luaTable.Set(fieldName, luaData.unityObject);
                break;
        }
    }
}