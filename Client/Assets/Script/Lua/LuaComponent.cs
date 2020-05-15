using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[Serializable]
public class LuaComponent {
    [SerializeField] public string name;
    [SerializeField] public List<LuaComponentData> luaComponentDatas;
}

[Serializable]
public struct LuaComponentData {
}