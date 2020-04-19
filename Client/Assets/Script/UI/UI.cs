using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class UI : SingletonMonobehaviour<UI> {
    [SerializeField] private Transform uiRoot;
    [SerializeField] private Transform launchBackground;
    [SerializeField] private Camera uiCamera;

    Dictionary<string, Window> windows = new Dictionary<string, Window>();

    List<WindowLoadTask> openTasks = new List<WindowLoadTask>();
    List<string> closeList = new List<string>();

    public void OpenWindow(string windowName) {
        if (openTasks.Find((x) => { return x.windowName == windowName; }) == null) {
            openTasks.Add(new WindowLoadTask(windowName));
        }

        if (closeList.Contains(windowName)) {
            closeList.Remove(windowName);
        }
    }

    public void CloseWindow(string windowName) {
        var index = openTasks.FindIndex((x) => { return x.windowName == windowName; });
        if (index >= 0) {
            openTasks[index].Stop();
            openTasks.RemoveAt(index);
        }

        if (!closeList.Contains(windowName)) {
            closeList.Add(windowName);
        }
    }

    private void LateUpdate() {
        if (openTasks.Count > 0) {
            var currentTask = openTasks[0];
            TryOpen(currentTask);
        }

        if (closeList.Count > 0) {
            for (int i = 0; i < closeList.Count; i++) {
                TryClose(closeList[i]);
            }

            closeList.Clear();
        }
    }

    private void OnLoadedWindow(WindowLoadTask task, GameObject go) {
        if (openTasks.Contains(task)) {
            openTasks.Remove(task);
            go = GameObject.Instantiate(go, uiRoot);
            var window = go.GetComponent<Window>();
            var canvas = go.GetComponent<Canvas>();
            canvas.worldCamera = uiCamera;
            windows.Add(task.windowName, window);
            window.Open();
        }
        else {
            AddressableSystem.Release(go);
        }
    }

    private void TryOpen(WindowLoadTask task) {
        if (windows.ContainsKey(task.windowName)) {
            var window = windows[task.windowName];
            if (window.state == WindowState.Closed) {
                window.Open();
            }
        }
        else {
            if (!task.loading) {
                task.Begin(OnLoadedWindow);
            }
        }
    }

    private void TryClose(string windowName) {
        if (windows.ContainsKey(windowName)) {
            var window = windows[windowName];
            if (window.state != WindowState.Closed && window.state != WindowState.CloseAnimation &&
                window.state != WindowState.ReadyClose) {
                window.Close();
            }
        }
    }
}