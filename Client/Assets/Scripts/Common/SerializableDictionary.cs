using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] List<TKey> keys;
    [SerializeField] List<TValue> values;

    public void OnAfterDeserialize()
    {
        this.Clear();
        if (keys != null && values != null)
        {
            var min = Mathf.Min(keys.Count, values.Count);
            for (int i = 0; i < min; i++)
            {
                this.Add(keys[i], values[i]);
            }
        }
    }

    public void OnBeforeSerialize()
    {
        keys = new List<TKey>(this.Keys);
        values = new List<TValue>(this.Values);
    }
}