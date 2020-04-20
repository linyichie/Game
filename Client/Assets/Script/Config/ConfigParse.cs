using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConfigParse {
    public static int ParseInt(string value) {
        var result = 0;
        int.TryParse(value, out result);
        return result;
    }
}