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
            case Image.Type.Tiled:
                GenerateTiledSprite(vh);
                break;
        }
    }

    private void GenerateTiledSprite(VertexHelper vh) {
        var uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
        var rect = graphic.GetPixelAdjustedRect();

        radius = Mathf.Min(radius, rect.width / 2, rect.height / 2);
        radius = Mathf.Max(0, radius);

        var circleBL = new Vector2(rect.xMin + radius, rect.yMin + radius);
        var circleTL = new Vector2(rect.xMin + radius, rect.yMax - radius);
        var circleBR = new Vector2(rect.xMax - radius, rect.yMin + radius);
        var circleTR = new Vector2(rect.xMax - radius, rect.yMax - radius);

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

        var tileWidth = overrideSprite.rect.width;
        var tileHeight = overrideSprite.rect.height;

        if (radius > 0) {
            xMin = rect.xMin;
            yMin = rect.yMax - radius;
            xMax = rect.xMin + radius;
            yMax = rect.yMax;

            var points = GetCirclePoints(xMin, yMin, xMax, yMax);
            var minIndices = GetTileVertIndex(new Vector2(xMin, yMin));
            var maxIndices = GetTileVertIndex(new Vector2(xMax, yMax));
            for (int i = minIndices.y; i < maxIndices.y + 1; i++) {
                var posMin = Vector2.zero;
                var posMax = Vector2.zero;
                posMin.y = Math.Max(rect.yMin + i * tileHeight, yMin);
                posMax.y = Mathf.Min(rect.yMin + (i + 1) * tileHeight, yMax);
                for (int j = minIndices.x; j < maxIndices.x + 1; j++) {
                    posMin.x = Math.Max(rect.xMin + j * tileWidth, xMin);
                    posMax.x = Mathf.Min(rect.xMin + (j + 1) * tileWidth, xMax);
                    var boundsPoints = GetBoundsPoints(posMin, posMax, points);
                    if (boundsPoints.Count == 0) {
                        if (Vector3.Distance(circleTL, posMin) < radius) {
                            AddRectangle(vh, posMin.x, posMin.y, posMax.x, posMax.y);
                        }
                    } else {
                        for (int k = 0; k < boundsPoints.Count; k++) {
                            if (k == 0) {
                                if (boundsPoints[k].x == posMin.x) {
                                    var startIndex = vh.currentVertCount;
                                    var point = new Vector3(posMin.x, posMin.y);
                                    vh.AddVert(point, target.color, GetTileVertUV(GetTileVertIndex(posMin), point));
                                    vh.AddVert(boundsPoints[k], target.color, GetTileVertUV(GetTileVertIndex(posMin), boundsPoints[k]));
                                    point = new Vector3(posMax.x, posMin.y);
                                    vh.AddVert(point, target.color, GetTileVertUV(GetTileVertIndex(posMin), point));
                                    vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                                }
                            }
                            if (k >= boundsPoints.Count - 1) {
                                if (boundsPoints[k].x < posMax.x) {
                                    var startIndex = vh.currentVertCount;
                                    vh.AddVert(boundsPoints[k], target.color, GetTileVertUV(GetTileVertIndex(posMin), boundsPoints[k]));
                                    var point = new Vector3(posMax.x, posMax.y);
                                    vh.AddVert(point, target.color, GetTileVertUV(GetTileVertIndex(posMin), point));
                                    point = new Vector3(posMax.x, posMin.y);
                                    vh.AddVert(point, target.color, GetTileVertUV(GetTileVertIndex(posMin), point));
                                    vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                                }
                                break;
                            }
                            {
                                var startIndex = vh.currentVertCount;
                                vh.AddVert(boundsPoints[k], target.color, GetTileVertUV(GetTileVertIndex(posMin), boundsPoints[k]));
                                vh.AddVert(boundsPoints[k + 1], target.color, GetTileVertUV(GetTileVertIndex(posMin), boundsPoints[k + 1]));
                                var point = new Vector3(posMax.x, posMin.y);
                                vh.AddVert(point, target.color, GetTileVertUV(GetTileVertIndex(posMin), point));
                                vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                            }
                        }
                    }
                }
            }
        }
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

    private void AddQuad(VertexHelper vh, Vector2 posMin, Vector2 posMax) {
        var startIndex = vh.currentVertCount;

        var position = new Vector2(posMin.x, posMin.y);
        vh.AddVert(position, target.color, GetTileVertUV(GetTileVertIndex(posMin), position));
        position = new Vector2(posMin.x, posMax.y);
        vh.AddVert(position, target.color, GetTileVertUV(GetTileVertIndex(posMin), position));
        position = new Vector2(posMax.x, posMax.y);
        vh.AddVert(position, target.color, GetTileVertUV(GetTileVertIndex(posMin), position));
        position = new Vector2(posMax.x, posMin.y);
        vh.AddVert(position, target.color, GetTileVertUV(GetTileVertIndex(posMin), position));

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
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

    private List<Vector3> GetCirclePoints(float xMin, float yMin, float xMax, float yMax) {
        List<Vector3> points = new List<Vector3>();
        var interval = radius / segments;
        for (int i = 0; i <= segments; i++) {
            var point = Vector3.zero;
            point.x = xMin + i * interval;
            var offsetX = xMax - point.x;
            point.y = yMin + Mathf.Sqrt(Mathf.Max(0, radius * radius - offsetX * offsetX));
            points.Add(point);
        }
        return points;
    }

    private List<Vector3> GetBoundsPoints(Vector3 posMin, Vector3 posMax, List<Vector3> points) {
        List<Vector3> boundPoints = new List<Vector3>();
        for (int i = 0; i < points.Count; i++) {
            if (!BoundsContainsPoint(posMin, posMax,points[i]) && i < points.Count - 1 && !BoundsContainsPoint(posMin, posMax, points[i + 1])) {
                continue;
            }
            bool contains = false;
            if (BoundsContainsPoint(posMin, posMax, points[i])) {
                boundPoints.Add(points[i]);
                contains = true;
            }
            if (i + 1 < points.Count) {
                var p1 = points[i];
                var p2 = points[i + 1];
                if (contains) {
                    if (p1.y < posMax.y && p2.y > posMax.y) {
                        var p3 = new Vector3(posMin.x, posMax.y);
                        var p4 = new Vector3(posMax.x, posMax.y);
                        boundPoints.Add(GetIntersection(p1, p2, p3, p4));
                    } else if (p1.x < posMax.x && p2.x > posMax.x) {
                        var p3 = new Vector3(posMax.x, posMin.y);
                        var p4 = new Vector3(posMax.x, posMax.y);
                        boundPoints.Add(GetIntersection(p1, p2, p3, p4));
                    }
                } else {
                    if (p1.y < posMin.y && p2.y > posMin.y) {
                        var p3 = new Vector3(posMin.x, posMin.y);
                        var p4 = new Vector3(posMax.x, posMin.y);
                        boundPoints.Add(GetIntersection(p1, p2, p3, p4));
                    } else if (p1.x < posMin.x && p2.x > posMin.x) {
                        var p3 = new Vector3(posMin.x, posMin.y);
                        var p4 = new Vector3(posMin.x, posMax.y);
                        boundPoints.Add(GetIntersection(p1, p2, p3, p4));
                    }
                }

            }
        }
        return boundPoints;
    }

    static Vector3 GetIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) {
        float x1 = p1.x, y1 = p1.y;
        float x2 = p2.x, y2 = p2.y;

        float x3 = p3.x, y3 = p3.y;
        float x4 = p4.x, y4 = p4.y;

        float x, y;

        if (x1 == x2) {
            float m2 = (y4 - y3) / (x4 - x3);
            float c2 = -m2 * x3 + y3;

            x = x1;
            y = c2 + m2 * x1;
        } else if (x3 == x4) {
            float m1 = (y2 - y1) / (x2 - x1);
            float c1 = -m1 * x1 + y1;

            x = x3;
            y = c1 + m1 * x3;
        } else {
            float m1 = (y2 - y1) / (x2 - x1);
            float c1 = -m1 * x1 + y1;

            float m2 = (y4 - y3) / (x4 - x3);
            float c2 = -m2 * x3 + y3;

            x = (c1 - c2) / (m2 - m1);
            y = c2 + m2 * x;
        }

        return new Vector3(x, y, 0);
    }

    static bool BoundsContainsPoint(Vector3 posMin, Vector3 posMax, Vector3 point) {
        if (point.x >= posMin.x && point.x <= posMax.x && point.y >= posMin.y && point.y <= posMax.y) {
            return true;
        }
        return false;
    }
}