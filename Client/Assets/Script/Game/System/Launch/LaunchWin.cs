﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchWin : Window {
    protected override void OnReadyOepn() {
        ConfigLoader.Initialize();
        ConfigLoader.compelted = () => { UI.Instance.OpenWindow("Login"); };
    }

    protected override void OnReadyClose() {
    }
}