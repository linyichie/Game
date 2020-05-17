using System;
using UnityEngine;
using XLua;

public class TestLuaInjection : MonoBehaviour {
    [SerializeField] private TextAsset luaScript;
    public int testValue = 10;

    // Start is called before the first frame update
    void Start() {
        LuaUtility.Initialize();
        var objects = LuaUtility.luaEnv.DoString(luaScript.bytes, "luaInjection");
        var table = objects[0] as LuaTable;
        var luaInitialize = table.Get<Action<LuaTable>>("OnInitialize");
        table.Set("go", this.gameObject);
        luaInitialize?.Invoke(table);
    }
}