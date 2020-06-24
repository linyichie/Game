using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FilletImage : BaseMeshEffect {
    [SerializeField] private float radius = 10f;
    [SerializeField, Range(8, 128)] private int segments = 8;
    [SerializeField, EnumFlags] private Fillet fillet = (Fillet)(-1);

    private Image target;
    private RectTransform rectTransform {
        get {
            return transform as RectTransform;
        }
    }
    private Sprite overrideSprite {
        get {
            return target.overrideSprite;
        }
    }

    [Flags]
    enum Fillet {
        TopLeft = 1 << 0,
        TopRight = 1 << 1,
        BottomLeft = 1 << 2,
        BottomRight = 1 << 3,
    }

    readonly Dictionary<Vector3, int> indices = new Dictionary<Vector3, int>();

    public override void ModifyMesh(VertexHelper vh) {
        vh.Clear();
        indices.Clear();
        if (target == null) {
            target = GetComponent<Image>();
        }

        switch (target.type) {
            case Image.Type.Simple:
                break;
            case Image.Type.Sliced:
                break;
            case Image.Type.Tiled:
                GenerateTiledSprite(vh);
                break;
        }
    }

    private void GenerateSlicedSprite(VertexHelper vh) {
    }

    private void GenerateTiledSprite(VertexHelper vh) {
        var uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
        var rect = graphic.GetPixelAdjustedRect();

        radius = Mathf.Min(radius, rect.width / 2, rect.height / 2);
        radius = Mathf.Max(0, radius);

        var xMin = rect.xMin + radius;
        var yMin = rect.yMin;
        var xMax = rect.xMax - radius;
        var yMax = rect.yMax;

        AddRectangle(vh, xMin, yMin, xMax, yMax);

        xMin = rect.xMin;
        yMin = rect.yMin + radius;
        xMax = rect.xMin + radius;
        yMax = rect.yMax - radius;

        AddRectangle(vh, xMin, yMin, xMax, yMax);

        xMin = rect.xMax - radius;
        yMin = rect.yMin + radius;
        xMax = rect.xMax;
        yMax = rect.yMax - radius;

        AddRectangle(vh, xMin, yMin, xMax, yMax);

        if (radius > 0) {
            var topLeft = 0 != (fillet & Fillet.TopLeft);
            var topRight = 0 != (fillet & Fillet.TopRight);
            var bottomLeft = 0 != (fillet & Fillet.BottomLeft);
            var bottomRight = 0 != (fillet & Fillet.BottomRight);

            if (!topLeft) {
                AddRectangle(vh, rect.xMin, rect.yMax - radius, rect.xMin + radius, rect.yMax);
            } else {
                AddCircle(vh, Fillet.TopLeft);
            }
            if (!topRight) {
                AddRectangle(vh, rect.xMax - radius, rect.yMax - radius, rect.xMax, rect.yMax);
            } else {
                AddCircle(vh, Fillet.TopRight);
            }
            if (!bottomLeft) {
                AddRectangle(vh, rect.xMin, rect.yMin, rect.xMin + radius, rect.yMin + radius);
            } else {
                AddCircle(vh, Fillet.BottomLeft);
            }
            if (!bottomRight) {
                AddRectangle(vh, rect.xMax - radius, rect.yMin, rect.xMax, rect.yMin + radius);
            } else {
                AddCircle(vh, Fillet.BottomRight);
            }
        }
    }

    private void AddCircle(VertexHelper vh, Fillet fillet) {
        var rect = graphic.GetPixelAdjustedRect();
        var filletRect = new Rect(rect);
        switch (fillet) {
            case Fillet.TopLeft:
                filletRect.yMin = rect.yMax - radius;
                filletRect.xMax = rect.xMin + radius;
                break;
            case Fillet.TopRight:
                filletRect.xMin = rect.xMax - radius;
                filletRect.yMin = rect.yMax - radius;
                break;
            case Fillet.BottomLeft:
                filletRect.xMax = rect.xMin + radius;
                filletRect.yMax = rect.yMin + radius;
                break;
            case Fillet.BottomRight:
                filletRect.xMin = rect.xMax - radius;
                filletRect.yMax = rect.yMin + radius;
                break;
        }

        var tileWidth = overrideSprite.rect.width;
        var tileHeight = overrideSprite.rect.height;

        var points = GetCirclePoints(filletRect, fillet);
        var minIndices = GetTileVertIndex(new Vector2(filletRect.xMin, filletRect.yMin));
        var maxIndices = GetTileVertIndex(new Vector2(filletRect.xMax, filletRect.yMax));
        for (int i = minIndices.y; i < maxIndices.y + 1; i++) {
            var minPoint = Vector2.zero;
            var maxPoint = Vector2.zero;
            minPoint.y = Math.Max(rect.yMin + i * tileHeight, filletRect.yMin);
            maxPoint.y = Mathf.Min(rect.yMin + (i + 1) * tileHeight, filletRect.yMax);
            for (int j = minIndices.x; j < maxIndices.x + 1; j++) {
                minPoint.x = Math.Max(rect.xMin + j * tileWidth, filletRect.xMin);
                maxPoint.x = Mathf.Min(rect.xMin + (j + 1) * tileWidth, filletRect.xMax);
                var boundsPoints = points.FindAll(x => { return RectContainsPoint(minPoint, maxPoint, x); });
                if (boundsPoints.Count == 0) {
                    var distance = float.MaxValue;
                    switch (fillet) {
                        case Fillet.TopLeft:
                            distance = Vector3.Distance(new Vector3(filletRect.xMax, filletRect.yMin), new Vector3(minPoint.x, maxPoint.y));
                            break;
                        case Fillet.TopRight:
                            distance = Vector3.Distance(new Vector3(filletRect.xMin, filletRect.yMin), new Vector3(maxPoint.x, maxPoint.y));
                            break;
                        case Fillet.BottomLeft:
                            distance = Vector3.Distance(new Vector3(filletRect.xMax, filletRect.yMax), new Vector3(minPoint.x, minPoint.y));
                            break;
                        case Fillet.BottomRight:
                            distance = Vector3.Distance(new Vector3(filletRect.xMin, filletRect.yMax), new Vector3(maxPoint.x, minPoint.y));
                            break;
                    }
                    if (distance < radius) {
                        AddRectangle(vh, minPoint.x, minPoint.y, maxPoint.x, maxPoint.y);
                    }
                } else {
                    var trianglePoint = Vector3.zero;
                    switch (fillet) {
                        case Fillet.TopLeft:
                            trianglePoint = new Vector3(maxPoint.x, minPoint.y);
                            break;
                        case Fillet.TopRight:
                            trianglePoint = new Vector3(minPoint.x, minPoint.y);
                            break;
                        case Fillet.BottomLeft:
                            trianglePoint = new Vector3(maxPoint.x, maxPoint.y);
                            break;
                        case Fillet.BottomRight:
                            trianglePoint = new Vector3(minPoint.x, maxPoint.y);
                            break;
                    }
                    for (int k = 0; k < boundsPoints.Count; k++) {
                        Vector3 p1 = Vector3.zero;
                        Vector3 p2 = Vector3.zero;
                        Vector3 p3 = Vector3.zero;
                        if (k >= boundsPoints.Count - 1) {
                            switch (fillet) {
                                case Fillet.TopLeft:
                                    if (boundsPoints[k].x < maxPoint.x) {
                                        p1 = boundsPoints[k];
                                        p2 = new Vector3(maxPoint.x, maxPoint.y);
                                        p3 = trianglePoint;
                                    }
                                    break;
                                case Fillet.TopRight:
                                    if (boundsPoints[k].y > minPoint.y) {
                                        p1 = boundsPoints[k];
                                        p2 = new Vector3(maxPoint.x, minPoint.y);
                                        p3 = trianglePoint;
                                    }
                                    break;
                                case Fillet.BottomLeft:
                                    if (boundsPoints[k].x < maxPoint.x) {
                                        p1 = boundsPoints[k];
                                        p2 = trianglePoint;
                                        p3 = new Vector3(maxPoint.x, minPoint.y);
                                    }
                                    break;
                                case Fillet.BottomRight:
                                    if (boundsPoints[k].y < maxPoint.y) {
                                        p1 = boundsPoints[k];
                                        p2 = trianglePoint;
                                        p3 = new Vector3(maxPoint.x, maxPoint.y);
                                    }
                                    break;
                            }
                        } else {
                            switch (fillet) {
                                case Fillet.TopLeft:
                                    p1 = boundsPoints[k];
                                    p2 = boundsPoints[k + 1];
                                    p3 = trianglePoint;
                                    break;
                                case Fillet.TopRight:
                                    p1 = boundsPoints[k];
                                    p2 = boundsPoints[k + 1];
                                    p3 = trianglePoint;
                                    break;
                                case Fillet.BottomLeft:
                                    p1 = boundsPoints[k];
                                    p3 = boundsPoints[k + 1];
                                    p2 = trianglePoint;
                                    break;
                                case Fillet.BottomRight:
                                    p1 = boundsPoints[k];
                                    p3 = boundsPoints[k + 1];
                                    p2 = trianglePoint;
                                    break;
                            }
                        }
                        AddTriangle(vh, p1, p2, p3, minPoint);
                        if (k == 0) {
                            switch (fillet) {
                                case Fillet.TopLeft:
                                    if (boundsPoints[k].x == minPoint.x) {
                                        p1 = new Vector3(minPoint.x, minPoint.y);
                                        p2 = boundsPoints[k];
                                        p3 = trianglePoint;
                                        AddTriangle(vh, p1, p2, p3, minPoint);
                                    }
                                    break;
                                case Fillet.TopRight:
                                    if (boundsPoints[k].y == maxPoint.y) {
                                        p1 = new Vector3(minPoint.x, maxPoint.y);
                                        p2 = boundsPoints[k];
                                        p3 = trianglePoint;
                                        AddTriangle(vh, p1, p2, p3, minPoint);
                                    }
                                    break;
                                case Fillet.BottomLeft:
                                    if (boundsPoints[k].x == minPoint.x) {
                                        p1 = new Vector3(minPoint.x, maxPoint.y);
                                        p2 = trianglePoint;
                                        p3 = boundsPoints[k];
                                        AddTriangle(vh, p1, p2, p3, minPoint);
                                    }
                                    break;
                                case Fillet.BottomRight:
                                    if (boundsPoints[k].y == minPoint.y) {
                                        p1 = new Vector3(minPoint.x, minPoint.y);
                                        p2 = trianglePoint;
                                        p3 = boundsPoints[k];
                                        AddTriangle(vh, p1, p2, p3, minPoint);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void AddTriangle(VertexHelper vh, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 min) {
        var startIndex = vh.currentVertCount;
        vh.AddVert(p1, target.color, GetTileVertUV(GetTileVertIndex(min), p1));
        vh.AddVert(p2, target.color, GetTileVertUV(GetTileVertIndex(min), p2));
        vh.AddVert(p3, target.color, GetTileVertUV(GetTileVertIndex(min), p3));
        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
    }

    private void AddRectangle(VertexHelper vh, float xMin, float yMin, float xMax, float yMax) {
        var rect = graphic.GetPixelAdjustedRect();
        var tileWidth = overrideSprite.rect.width;
        var tileHeight = overrideSprite.rect.height;

        var minIndices = GetTileVertIndex(new Vector2(xMin, yMin));
        var maxIndices = GetTileVertIndex(new Vector2(xMax, yMax));

        for (long i = minIndices.y; i < maxIndices.y + 1; i++) {
            var posMin = Vector2.zero;
            var posMax = Vector2.zero;
            posMin.y = Math.Max(rect.yMin + i * tileHeight, yMin);
            posMax.y = Mathf.Min(rect.yMin + (i + 1) * tileHeight, yMax);
            for (long j = minIndices.x; j < maxIndices.x + 1; j++) {
                posMin.x = Math.Max(rect.xMin + j * tileWidth, xMin);
                posMax.x = Mathf.Min(rect.xMin + (j + 1) * tileWidth, xMax);
                AddQuad(vh, posMin, posMax);
            }
        }
    }

    private void AddQuad(VertexHelper vh, Vector2 min, Vector2 max) {
        var startIndex = vh.currentVertCount;

        var position = new Vector2(min.x, min.y);
        vh.AddVert(position, target.color, GetTileVertUV(GetTileVertIndex(min), position));
        position = new Vector2(min.x, max.y);
        vh.AddVert(position, target.color, GetTileVertUV(GetTileVertIndex(min), position));
        position = new Vector2(max.x, max.y);
        vh.AddVert(position, target.color, GetTileVertUV(GetTileVertIndex(min), position));
        position = new Vector2(max.x, min.y);
        vh.AddVert(position, target.color, GetTileVertUV(GetTileVertIndex(min), position));

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }

    private Vector2Int GetTileVertIndex(Vector2 position) {
        var rect = graphic.GetPixelAdjustedRect();
        var tileWidth = overrideSprite.rect.width;
        var tileHeight = overrideSprite.rect.height;
        return new Vector2Int((int)((position.x - rect.xMin) / tileWidth), (int)((position.y - rect.yMin) / tileHeight));
    }

    private Vector2 GetTileVertUV(Vector2Int index, Vector2 position) {
        var rect = graphic.GetPixelAdjustedRect();
        var uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
        var tileWidth = overrideSprite.rect.width;
        var tileHeight = overrideSprite.rect.height;
        var posMin = new Vector2(rect.xMin + index.x * tileWidth, rect.yMin + index.y * tileHeight);
        var tileUV = new Vector2(uv.x, uv.y);
        if (posMin != position) {
            tileUV = new Vector2(uv.x + (uv.z - uv.x) * (position.x - posMin.x) / tileWidth, uv.y + (uv.w - uv.y) * (position.y - posMin.y) / tileHeight);
        }
        return tileUV;
    }

    private List<Vector3> GetCirclePoints(Rect filletRect, Fillet fillet) {
        List<Vector3> points = new List<Vector3>();
        var interval = radius / segments;
        for (int i = 0; i <= segments; i++) {
            var point = Vector3.zero;
            point.x = filletRect.xMin + i * interval;
            switch (fillet) {
                case Fillet.TopLeft: {
                    var offsetX = filletRect.xMax - point.x;
                    point.y = filletRect.yMin + Mathf.Sqrt(Mathf.Max(0, radius * radius - offsetX * offsetX));
                    break;
                }
                case Fillet.TopRight: {
                    var offsetX = i * interval;
                    point.y = filletRect.yMin + Mathf.Sqrt(Mathf.Max(0, radius * radius - offsetX * offsetX));
                    break;
                }
                case Fillet.BottomLeft: {
                    var offsetX = filletRect.xMax - point.x;
                    point.y = filletRect.yMax - Mathf.Sqrt(Mathf.Max(0, radius * radius - offsetX * offsetX));
                    break;
                }
                case Fillet.BottomRight: {
                    var offsetX = i * interval;
                    point.y = filletRect.yMax - Mathf.Sqrt(Mathf.Max(0, radius * radius - offsetX * offsetX));
                    break;
                }
            }
            points.Add(point);
        }

        var rect = graphic.GetPixelAdjustedRect();
        var minIndices = GetTileVertIndex(new Vector2(filletRect.xMin, filletRect.yMin));
        var maxIndices = GetTileVertIndex(new Vector2(filletRect.xMax, filletRect.yMax));
        var tileWidth = overrideSprite.rect.width;
        var tileHeight = overrideSprite.rect.height;
        var intersectionPoints = new List<Vector3>();
        for (int i = minIndices.y; i < maxIndices.y + 1; i++) {
            Vector3 p1 = new Vector3(filletRect.xMin, rect.yMin + i * tileHeight);
            Vector3 p2 = new Vector3(filletRect.xMin + radius, rect.yMin + i * tileHeight);
            for (int j = 0; j < points.Count - 1; j++) {
                var p3 = points[j];
                var p4 = points[j + 1];
                var intersection = GetIntersection(p1, p2, p3, p4);
                if (IsInsideLine(p1, p2, intersection) && IsInsideLine(p3, p4, intersection)) {
                    intersectionPoints.Add(intersection);
                }
            }
        }
        for (int i = minIndices.x; i < maxIndices.x + 1; i++) {
            Vector3 p1 = new Vector3(rect.xMin + i * tileWidth, filletRect.yMax - radius);
            Vector3 p2 = new Vector3(rect.xMin + i * tileWidth, filletRect.yMax);
            for (int j = 0; j < points.Count - 1; j++) {
                var p3 = points[j];
                var p4 = points[j + 1];
                var intersection = GetIntersection(p1, p2, p3, p4);
                if (IsInsideLine(p1, p2, intersection) && IsInsideLine(p3, p4, intersection)) {
                    intersectionPoints.Add(intersection);
                }
            }
        }
        points.AddRange(intersectionPoints);
        points.Sort((lhs, rhs) => { return lhs.x.CompareTo(rhs.x); });
        return points;
    }

    static Vector3 GetIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
        var t1 = LineCut(p1, p2, p3, p4);
        var t2 = LineCut(p3, p4, p1, p2);
        var intersection = (p2 - p1) * t1 + p1;
        return intersection;
    }

    static float LineCut(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
        var line = p4 - p3;
        var n = new Vector2(line.y, -line.x);
        var div = Vector2.Dot(p2 - p1, n);
        return Vector2.Dot(p3 - p1, n) / div;
    }

    static bool RectContainsPoint(Vector3 min, Vector3 max, Vector3 point) {
        if (point.x >= min.x && point.x <= max.x && point.y >= min.y && point.y <= max.y) {
            return true;
        }
        return false;
    }

    static bool IsInsideLine(Vector3 left, Vector3 right, Vector3 point) {
        if (point.x >= left.x && point.x <= right.x) {
            return true;
        }
        return false;
    }
}