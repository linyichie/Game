using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSkipApp : MonoBehaviour {
    [SerializeField] private string packageUrl;
    [SerializeField] private Image testImage;

    private void OnGUI() {
        if (GUILayout.Button("Installed", GUILayout.Width(100), GUILayout.Height(200))) {
            var installed = SDK.InstalledApp(packageUrl);
            testImage.color = installed ? Color.green : Color.red;
        }

        if (GUILayout.Button("Open", GUILayout.Width(100), GUILayout.Height(200))) {
            SDK.OpenApp(packageUrl);
        }

        if (GUILayout.Button("Install", GUILayout.Width(100), GUILayout.Height(200))) {
            SDK.GoInstallApp(@"itms-apps://itunes.apple.com/cn/app/id527572895");
        }
    }
}