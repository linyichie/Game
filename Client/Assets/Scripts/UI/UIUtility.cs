using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIUtility {
    public static int RayCrossingCount(Vector2 p, List<Vector3> vertices) {
        var crossNum = 0;
        for (int i = 0, count = vertices.Count; i < count; i++) {
            var v1 = vertices[i];
            var v2 = vertices[(i + 1) % count];

            if (((v1.y <= p.y) && (v2.y > p.y)) || ((v1.y > p.y) && (v2.y <= p.y))) {
                if (p.x < v1.x + (p.y - v1.y) / (v2.y - v1.y) * (v2.x - v1.x)) {
                    crossNum += 1;
                }
            }
        }

        return crossNum;
    }
}