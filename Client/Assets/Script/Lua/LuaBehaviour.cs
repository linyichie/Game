using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class LuaBehaviour : MonoBehaviour {
    [Reorderable] [SerializeField] private LuaInjection[] luaInjections;

    public void StartLuaInjection(LuaTable luaTable) {
        if(luaInjections != null) {
            foreach(var luaInjection in luaInjections) {
                luaTable[luaInjection.]
            }
        }
    }
}