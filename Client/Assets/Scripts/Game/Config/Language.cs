using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
public static class Language {
    public static string Get(string key) {
        return LanguageConfig.Get(key).text;
    }
}