using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConfigLoader {
    private static readonly object lockObject = new object();

    private static int count = 0;

    public static bool compelted = false;

    public static int Count {
        get { return count; }
        private set {
            lock(lockObject) {
                count = value;
            }
        }
    }

    public static void Initialize() {
        compelted = false;
        LoadConfig("Geo", s => { GeoConfig.Parse(true, s, () => { CompleteLoad(); }); });
    }

    static void LoadConfig(string fileName, Action<string> callback) {
        Count += 1;
        var addressableName = StringUtil.Contact("Txt/", fileName);
        AssetLoad.LoadAsync<TextAsset>(addressableName, asset => {
            var textAsset = asset.asset as TextAsset;
            callback?.Invoke(textAsset.text);
            AssetLoad.Release(addressableName);
        });
    }

    static void CompleteLoad() {
        Count = Count - 1;
        if(Count <= 0) {
            compelted = true;
        }
    }
}