using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowLoadTask {
    public string windowName { get; private set; } = string.Empty;

    public bool isDone { get; private set; } = false;

    public bool loading { get; private set; } = false;

    private Action<WindowLoadTask, GameObject> completed;

    public WindowLoadTask(string windowName) {
        this.windowName = windowName;
    }

    public void Begin(Action<WindowLoadTask, GameObject> callback) {
        loading = true;
        completed = callback;
        AddressableSystem.LoadAsset<GameObject>(StringUtility.Contact("Windows/", this.windowName),
            o => {
                if (completed == null) {
                    AddressableSystem.Release(o);
                }
                else {
                    completed.Invoke(this, o);
                }

                loading = false;
            });
    }

    public void Stop() {
        completed = null;
    }
}