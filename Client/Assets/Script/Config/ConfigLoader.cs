using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConfigLoader {
    public static void Initialize() {
        LoadConfig("Geo", s => { GeoConfig.Parse(true, s, () => { Debug.Log(GeoConfig.Get(11).polygon); }); });
    }

    static void LoadConfig(string fileName, Action<string> callback) {
        AddressableSystem.LoadAsset<TextAsset>(StringUtility.Contact("Txt/", fileName),
            asset => { callback?.Invoke(asset.text); });
    }
}