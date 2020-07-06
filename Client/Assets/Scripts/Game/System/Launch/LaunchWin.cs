using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchWin : Window {
    protected override void OnInitialize() {
    }

    protected override void OnReadyOepn() {
    }

    protected override void OnOpened() {
        Game.Instance.LuaStart();
    }

    protected override void OnReadyClose() {
    }

    protected override void OnClosed() {
    }
}