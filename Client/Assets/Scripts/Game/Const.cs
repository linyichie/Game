using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Const {
    public static readonly string[] SplitLine_Identifier = new string[] {"\r\n"};
    public static readonly string[] SplitLine_Identifier_Android = new string[] {"\n"};
    public static readonly string[] SplitLine_Identifier_iOS = new string[] {"\r"};

    public static string[] SplitLine {
        get {
#if UNITY_ANDROID
            return SplitLine_Identifier_Android;
#elif UNITY_IOS
            return SplitLine_Identifier_iOS;
#endif
            return SplitLine_Identifier;
        }
    }
}