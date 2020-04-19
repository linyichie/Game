using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddressableSystem {
    public static void LoadAsset<T>(string key, Action<T> callback) {
        var assetAsync = Addressables.LoadAssetAsync<T>(key);
        assetAsync.Completed += (@object) => { callback?.Invoke(@object.Result); };
    }

    public static void Release(GameObject go) {
        Addressables.Release(go);
    }
}