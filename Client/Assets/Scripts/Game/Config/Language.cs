using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Language {
    public static string Get(string key) {
        return LanguageConfig.Get(key).text;
    }
}