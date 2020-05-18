using System.Collections.Generic;
using UnityEngine;
using System;

public partial class LanguageConfig {
    public string key { get; private set; }
	public string text { get; private set; }

    static Dictionary<string, LanguageConfig> configs = new Dictionary<string, LanguageConfig>();

    private LanguageConfig(string[] values) {
        try {
            key = values[0].Trim();

			text = values[1].Trim();

            configs.Add(key, this);
        }
        catch (System.Exception e) {
            Debug.LogError(string.Format("Config Parse Error : {0} key ==> {1}", "LanguageConfig", values[0]));
        }
    }

    public static LanguageConfig Get(string _key) {
        if (configs.ContainsKey(_key)) {
            return configs[_key];
        }

        return null;
    }

    public static void Parse(bool async, string content, Action callback) {
        var lines = content.Split(Const.SplitLine, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 3; i < lines.Length; i++) {
            var line = lines[i];
            var values = line.Split('\t');
            var config = new LanguageConfig(values);
        }

        callback?.Invoke();
    }
    
    public static void Clear() {
        configs.Clear();
    }    
}