//--------------------------------------------------------
//    [Author]:               Sausage
//    [  Date ]:             2020年7月7日
//--------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System;
using UnityEngine;
public partial class ExampleConfig {
    public readonly int id;
	public readonly float floatValue;
	public readonly string label;
	public readonly Vector3 position;
	public readonly bool boolValue;
	public readonly int[] intValues;
	public readonly float[] floatValues;
	public readonly string[] labels;
	public readonly Vector3[] positions;
    public ExampleConfig(string input) {
        try {
            var tables = input.Split('\t');
            int.TryParse(tables[0],out id); 
			float.TryParse(tables[1],out floatValue); 
			label = tables[2];
			position=tables[3].Vector3Parse();
			var boolValueTemp = 0;
			int.TryParse(tables[4],out boolValueTemp); 
			boolValue=boolValueTemp!=0;
			string[] intValuesStringArray = tables[5].Trim().Split(StringUtil.splitSeparator,StringSplitOptions.RemoveEmptyEntries);
			intValues = new int[intValuesStringArray.Length];
			for (int i = 0; i <intValuesStringArray.Length;i++)
			{
				 int.TryParse(intValuesStringArray[i],out intValues[i]);
			}
			string[] floatValuesStringArray = tables[6].Trim().Split(StringUtil.splitSeparator,StringSplitOptions.RemoveEmptyEntries);
			floatValues = new float[floatValuesStringArray.Length];
			for (int i = 0; i <floatValuesStringArray.Length;i++)
			{
				 float.TryParse(floatValuesStringArray[i],out floatValues[i]);
			}
			labels = tables[7].Trim().Split(StringUtil.splitSeparator,StringSplitOptions.RemoveEmptyEntries);
			string[] positionsStringArray = tables[8].Trim().Split(StringUtil.splitSeparator,StringSplitOptions.RemoveEmptyEntries);
			positions = new Vector3[positionsStringArray.Length];
			for (int i = 0; i <positionsStringArray.Length;i++)
			{
				positions[i]=positionsStringArray[i].Vector3Parse();
			}
        } catch (Exception ex) {
            Debug.Log(ex);
        }
    }
    static Dictionary<string, ExampleConfig> configs = null;
    public static ExampleConfig Get(string id) {
        if (!Inited) {
            Init(true);
        }
        if (string.IsNullOrEmpty(id)) {
            return null;
        }
        if (configs.ContainsKey(id)) {
            return configs[id];
        }
        ExampleConfig config = null;
        if (rawDatas.ContainsKey(id)) {
            config = configs[id] = new ExampleConfig(rawDatas[id]);
            rawDatas.Remove(id);
        }
        if (config == null) {
            Debug.LogFormat("获取配置失败 ExampleConfig id:{0}", id);
        }
        return config;
    }
    public static ExampleConfig Get(int id) {
        return Get(id.ToString());
    }
    public static bool Has(string id) {
        if (!Inited) {
            Init(true);
        }
        return configs.ContainsKey(id) || rawDatas.ContainsKey(id);
    }
    public static List<string> GetKeys() {
        if (!Inited) {
            Init(true);
        }
        var keys = new List<string>();
        keys.AddRange(configs.Keys);
        keys.AddRange(rawDatas.Keys);
        return keys;
    }
    public static bool Inited { get; private set; }
    protected static Dictionary<string, string> rawDatas = null;
    public static void Init(bool sync = false) {
        Inited = false;
        var lines = ConfigLoader.GetConfigRawDatas("Example");
        configs = new Dictionary<string, ExampleConfig>();
        if (sync) {
            rawDatas = new Dictionary<string, string>(lines.Length - 3);
            for (var i = 3; i < lines.Length; i++) {
                var line = lines[i];
                var index = line.IndexOf("\t");
                var id = line.Substring(0, index);
                rawDatas.Add(id, line);
            }
            Inited = true;
        } else {
            ThreadPool.QueueUserWorkItem((object @object) => {
                rawDatas = new Dictionary<string, string>(lines.Length - 3);
                for (var i = 3; i < lines.Length; i++) {
                    var line = lines[i];
                    var index = line.IndexOf("\t");
                    var id = line.Substring(0, index);
                    rawDatas.Add(id, line);
                }
                Inited = true;
            });
        }
    }
}
