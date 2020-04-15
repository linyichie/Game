using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using UnityEngine;

public static class SDK {
    [DllImport("__Internal")]
    static extern bool CanOpenApp(string packageUrl);

    public static bool InstalledApp(string packageUrl) {
#if UNITY_ANDROID && !UNITY_EDITOR
        return InstalledAndroidApp(packageUrl);
#elif UNITY_IOS && !UNITY_EDITOR
        return CanOpenApp(packageUrl);
#endif
        return true;
    }

    static bool InstalledAndroidApp(string packageName) {
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaObject packageManager = androidJavaObject.Call<AndroidJavaObject>("getPackageManager")) {
            using (AndroidJavaObject appList = packageManager.Call<AndroidJavaObject>("getInstalledPackages", 0)) {
                var count = appList.Call<int>("size");
                for (int i = 0; i < count; i++) {
                    AndroidJavaObject appInfo = appList.Call<AndroidJavaObject>("get", i);
                    var tempPackageName = appInfo.Get<string>("packageName");
                    appInfo.Dispose();
                    if (tempPackageName.CompareTo(packageName) == 0) {
                        return true;
                    }
                }

                return false;
            }
        }
    }

    public static void OpenApp(string packageUrl) {
#if UNITY_ANDROID && !UNITY_EDITOR
        OpenAndroidApp(packageUrl);
#elif UNITY_IOS && !UNITY_EDITOR
        Application.OpenURL(@packageUrl);
#endif
    }

    static void OpenAndroidApp(string packageName) {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
        using (AndroidJavaObject intent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName)) {
            if (intent != null) {
                intent.Call("startActivity", intent);
            }
        }
    }

    public static void GoInstallApp(string packageUrl) {
        Application.OpenURL(@packageUrl);
    }
}