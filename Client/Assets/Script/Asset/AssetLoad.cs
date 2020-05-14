using System;
using System.Collections;
using System.Collections.Generic;
using Funny.Asset;
using UnityEngine;

public static class AssetLoad {
    private static Dictionary<string, IAsset> assets = new Dictionary<string, IAsset>();

    public static void LoadAsync<T>(string addressableName, Action<IAsset> callback) where T : UnityEngine.Object {
        IAsset asset;
        if(!assets.TryGetValue(addressableName, out asset)) {
            asset = new AddressableAsset(addressableName);
            assets[addressableName] = asset;
        }

        asset.LoadAsync<T>(callback);
    }

    public static void Release(string addressableName) {
        IAsset asset;
        if(assets.TryGetValue(addressableName, out asset)) {
            asset.Release();
        }
    }

    public static void Release(string addressableName, UnityEngine.Object @object) {
        IAsset asset;
        if(assets.TryGetValue(addressableName, out asset)) {
            asset.Release(@object);
        }
    }
}