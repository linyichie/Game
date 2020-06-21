using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class Game : SingletonMonobehaviour<Game> {
    // Start is called before the first frame update
    private LuaTable luaGame;
    private Action<LuaTable> luaStart;

    void Start() {
        WindowController.Instance.OpenWindow("Launch");
    }

    public void LuaStart() {
        LuaUtility.Initialize();
        var textAsset = AssetLoad.Load<TextAsset>("Scripts.Game");
        var objects = LuaUtility.luaEnv.DoString(textAsset.bytes, "Game");
        if(objects != null) {
            luaGame = objects[0] as LuaTable;
            luaStart = luaGame.Get<Action<LuaTable>>("Start");
        }

        luaStart?.Invoke(luaGame);
    }

    private void Update() {
        LuaUtility.Update();
    }
}