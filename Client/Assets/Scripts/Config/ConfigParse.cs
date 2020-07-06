using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConfigParse {
    public static Vector3 Vector3Parse(this string input) {
        if (string.IsNullOrEmpty(input)) {
            return Vector3.zero;
        }

        input = input.Replace("(", "").Replace(")", "");
        var stringArray = input.Split(',');

        var x = 0f;
        var y = 0f;
        var z = 0f;
        if (stringArray.Length > 0) {
            float.TryParse(stringArray[0], out x);
        }

        if (stringArray.Length > 1) {
            float.TryParse(stringArray[1], out y);
        }

        if (stringArray.Length > 2) {
            float.TryParse(stringArray[2], out z);
        }

        return new Vector3(x, y, z);
    }
}