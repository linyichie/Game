using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConfigLoader {
    private static readonly object lockObject = new object();

    private static int count = 0;

    public static Action compelted;

    public static int Count {
        get { return count; }
        private set {
            lock (lockObject) {
                count = value;
            }
        }
    }

    public static void Initialize() {
        LoadConfig("Geo", s => { GeoConfig.Parse(true, s, () => { CompleteLoad(); }); });
        LoadConfig(StringUtility.Contact("Language_", Localization.language),
            s => { LanguageConfig.Parse(true, s, () => { CompleteLoad(); }); });
        
        Game.Instance.StartCoroutine(OnLoadConfigs());
    }

    public static void ReloadLocalization() {
        LanguageConfig.Clear();
        LoadConfig(StringUtility.Contact("Language_", Localization.language),
            s => { LanguageConfig.Parse(true, s, () => { CompleteLoad(); }); });

        Game.Instance.StartCoroutine(OnLoadConfigs());
    }

    public static IEnumerator OnLoadConfigs() {
        while (Count > 0) {
            yield return null;
        }

        compelted?.Invoke();
        compelted = null;
    }

    static void LoadConfig(string fileName, Action<string> callback) {
        Count += 1;
        AddressableSystem.LoadAsset<TextAsset>(StringUtil.Contact("Txt/", fileName),
            asset => { callback?.Invoke(asset.text); });
    }

    static void CompleteLoad() {
        Count = Count - 1;
    }
}