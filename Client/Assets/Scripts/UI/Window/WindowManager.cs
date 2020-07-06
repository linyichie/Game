using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[XLua.LuaCallCSharp]
public class WindowManager : Singleton<WindowManager> {
    private UIRoot uiRoot;

    private Dictionary<string, Window> windows = new Dictionary<string, Window>();
    private Dictionary<string, Action<Window>> loadedCallbacks = new Dictionary<string, Action<Window>>();

    private List<string> openList = new List<string>();
    private List<string> openingList = new List<string>();
    private List<string> closeList = new List<string>();

    public void Initialize(UIRoot uiRoot) {
        this.uiRoot = uiRoot;
    }

    public void OpenWindow(string windowName) {
        if (!openingList.Contains(windowName) && !openList.Contains(windowName)) {
            openList.Add(windowName);
        }
    }

    public void OpenWindow(string windowName, Action<Window> callback) {
        if (!openingList.Contains(windowName) && !openList.Contains(windowName)) {
            openList.Add(windowName);
        }

        if (closeList.Contains(windowName)) {
            closeList.Remove(windowName);
        }

        loadedCallbacks[windowName] = callback;
    }

    public void CloseWindow(string windowName) {
        if (openList.Contains(windowName)) {
            openList.Remove(windowName);
        }

        if (!closeList.Contains(windowName)) {
            closeList.Add(windowName);
        }
    }

    public void OnUpdate() {
        if (openList.Count > 0) {
            foreach (var windowName in openList) {
                TryOpen(windowName);
            }

            openList.Clear();
        }

        foreach (var windowName in closeList) {
            TryClose(windowName);
        }
        closeList.Clear();
    }

    private void OnLoadedWindow(string windowName, GameObject go) {
        if (openingList.Contains(windowName)) {
            openingList.Remove(windowName);
            go.transform.SetParent(uiRoot.Root);
            var window = go.GetComponent<Window>();
            var canvas = go.GetComponent<Canvas>();
            canvas.worldCamera = uiRoot.UICamera;
            windows.Add(windowName, window);
            if (loadedCallbacks.ContainsKey(windowName)) {
                loadedCallbacks[windowName](window);
            }

            window.Open();
        } else {
            var addressableName = StringUtil.Concat("Windows/", windowName);
            AssetLoad.Release(addressableName, go);
        }
    }

    private void TryOpen(string windowName) {
        if (windows.ContainsKey(windowName)) {
            var window = windows[windowName];
            if (window.state == WindowState.Closed) {
                window.Open();
            }
        } else {
            openingList.Add(windowName);
            var addressableName = StringUtil.Concat("Windows/", windowName);
            AssetLoad.LoadAsync<GameObject>(addressableName, asset => { OnLoadedWindow(windowName, asset.asset as GameObject); });
        }
    }

    private void TryClose(string windowName) {
        if (windows.ContainsKey(windowName)) {
            var window = windows[windowName];
            if (window.state != WindowState.Closed && window.state != WindowState.CloseAnimation && window.state != WindowState.ReadyClose) {
                window.Close();
            }
        }
    }
}