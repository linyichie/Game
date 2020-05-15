using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class LuaBehaviour : MonoBehaviour {
    [SerializeField] private LuaComponent[] luaComponents;

    public void StartLuaInjection(LuaTable luaTable) {
        if(luaComponents != null) {
            foreach(var luaComponent in luaComponents) {
                //luaTable.Set(luaComponent.name, luaComponent.Curve);
            }
        }
    }
}