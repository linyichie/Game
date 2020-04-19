using System.Collections.Generic;
using UnityEngine;
using System;

public partial class GeoConfig {
    public int id { get; private set; }
	public string polygon { get; private set; }

    static Dictionary<int, GeoConfig> configs = new Dictionary<int, GeoConfig>();

    private GeoConfig(string[] values) {
        try {
            id = ConfigParse.ParseInt(values[0].Trim());

			polygon = values[1].Trim();

            configs.Add(id, this);
        }
        catch (System.Exception e) {
            Debug.LogError(string.Format("Config Parse Error : {0} key ==> {1}", "GeoConfig", values[0]));
        }
    }

    public static GeoConfig Get(int _key) {
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
            var config = new GeoConfig(values);
        }

        callback?.Invoke();
    }
}