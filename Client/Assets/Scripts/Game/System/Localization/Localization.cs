using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[XLua.LuaCallCSharp]
public static class Localization {
    public static string language { get; private set; } = "CN";
    public static Action languageChange;

    public static void Set(string language) {
        if (Localization.language != language) {
            Localization.language = language;
            languageChange?.Invoke();
        }
    }
}