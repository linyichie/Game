using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[RequireComponent(typeof(LuaBehaviour))]
[XLua.LuaCallCSharp]
public class LuaWindow : Window {
    private LuaTable luaTable;
    private Action<LuaTable> luaInitialize;
    private Action<LuaTable> luaReadyOpen;
    private Action<LuaTable> luaOpened;
    private Action<LuaTable> luaReadyClose;
    private Action<LuaTable> luaClosed;
    private Action<LuaTable> luaLateUpdate;
    private Action<LuaTable> luaOnOpenAnimation;
    private Action<LuaTable> luaOnCloseAnimation;
    private LuaBehaviour luaBehaviour;

    public void LuaBind(LuaTable luaTable) {
        this.luaTable = luaTable;
        luaInitialize = this.luaTable.Get<Action<LuaTable>>("OnInitialize");
        luaReadyOpen = this.luaTable.Get<Action<LuaTable>>("OnReadyOepn");
        luaOpened = this.luaTable.Get<Action<LuaTable>>("OnOpened");
        luaReadyClose = this.luaTable.Get<Action<LuaTable>>("OnReadyClose");
        luaClosed = this.luaTable.Get<Action<LuaTable>>("OnClosed");
        if(luaBehaviour == null) {
            luaBehaviour = GetComponent<LuaBehaviour>();
        }
        luaBehaviour.StartLuaInjection(luaTable);
    }

    #region override
    protected override void OnInitialize() {
        luaInitialize?.Invoke(luaTable);
    }

    protected override void OnReadyOepn() {
        luaReadyOpen?.Invoke(luaTable);
    }

    protected override void OnOpened() {
        luaOpened?.Invoke(luaTable);
    }

    protected override void OnReadyClose() {
        luaReadyClose?.Invoke(luaTable);
    }

    protected override void OnClosed() {
        luaClosed?.Invoke(luaTable);
    }

    protected override void OnLateUpdate() {
    }

    protected override void OnCloseAnimation(Action callback) {
        base.OnCloseAnimation(callback);
    }

    protected override void OnOpenAnimation(Action callback) {
        base.OnOpenAnimation(callback);
    }
    #endregion
}