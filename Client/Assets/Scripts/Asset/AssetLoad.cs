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
            asset = Get<T>(addressableName);
            assets[addressableName] = asset;
        }

        asset.LoadAsync<T>(callback);
    }

    public static T Load<T>(string assetPath) where T : UnityEngine.Object {
        IAsset asset;
        if(!assets.TryGetValue(assetPath, out asset)) {
            asset = Get<T>(assetPath);
            assets[assetPath] = asset;
        }

        return asset.Load<T>();
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

    static IAsset Get<T>(string assetPath) {
        if(typeof(T) == typeof(GameObject)) {
            return new PrefabAsset(assetPath);
        } else if(typeof(T) == typeof(TextAsset)) {
            if(assetPath.Contains("Scripts")) {
                return new LuaAsset(assetPath);
            }
        }

        return new AddressableAsset(assetPath);
    }
}