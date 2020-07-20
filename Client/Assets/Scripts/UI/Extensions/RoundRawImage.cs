using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class RoundRawImage : BaseMeshEffect {
    [SerializeField] private float radius = 10f;
    [SerializeField, Range(8, 128)] private int segments = 8;
    [SerializeField, Range(0, 90)] private float radian = 15;
    [SerializeField, EnumFlags] private Fillet fillet = (Fillet)(-1);

    private RawImage rawImage = null;

    [Flags]
    enum Fillet {
        TopLeft = 1 << 0,
        TopRight = 1 << 1,
        BottomLeft = 1 << 2,
        BottomRight = 1 << 3,
    }

    public override void ModifyMesh(VertexHelper vh) {
        var rect = graphic.GetPixelAdjustedRect();
        radius = Mathf.Min(radius, rect.width / 2, rect.height / 2);
        if (radius <= 0) {
            return;
        }

        if (rawImage == null) {
            rawImage = GetComponent<RawImage>();
        }

        vh.Clear();
        Generate(vh);
    }

    private void Generate(VertexHelper vh) {
        var rect = graphic.GetPixelAdjustedRect();
        AddSimpleQuad(vh, rect.xMin, rect.yMin + radius, rect.xMin + radius, rect.yMax - radius);
        AddSimpleQuad(vh, rect.xMin + radius, rect.yMax - radius, rect.xMax - radius, rect.yMax);
        AddSimpleQuad(vh, rect.xMax - radius, rect.yMin + radius, rect.xMax, rect.yMax - radius);
        AddSimpleQuad(vh, rect.xMin + radius, rect.yMin, rect.xMax - radius, rect.yMin + radius);
        AddSimpleQuad(vh, rect.xMin + radius, rect.yMin + radius, rect.xMax - radius, rect.yMax - radius);
        if (radius > 0) {
            var topLeft = 0 != (fillet & Fillet.TopLeft);
            var topRight = 0 != (fillet & Fillet.TopRight);
            var bottomLeft = 0 != (fillet & Fillet.BottomLeft);
            var bottomRight = 0 != (fillet & Fillet.BottomRight);
            if (topLeft) {
                AddSimpleCircle(vh, Fillet.TopLeft);
            } else {
                AddSimpleQuad(vh, rect.xMin, rect.yMax - radius, rect.xMin + radius, rect.yMax);
            }
            if (topRight) {
                AddSimpleCircle(vh, Fillet.TopRight);
            } else {
                AddSimpleQuad(vh, rect.xMax - radius, rect.yMax - radius, rect.xMax, rect.yMax);
            }
            if (bottomLeft) {
                AddSimpleCircle(vh, Fillet.BottomLeft);
            } else {
                AddSimpleQuad(vh, rect.xMin, rect.yMin, rect.xMin + radius, rect.yMin + radius);
            }
            if (bottomRight) {
                AddSimpleCircle(vh, Fillet.BottomRight);
            } else {
                AddSimpleQuad(vh, rect.xMax - radius, rect.yMin, rect.xMax, rect.yMin + radius);
            }
        }
    }

    private void AddSimpleQuad(VertexHelper vh, float xMin, float yMin, float xMax, float yMax) {
        var startIndex = vh.currentVertCount;

        var position = new Vector2(xMin, yMin);
        vh.AddVert(position, rawImage.color, GetSimpleVertUV(position));
        position = new Vector2(xMin, yMax);
        vh.AddVert(position, rawImage.color, GetSimpleVertUV(position));
        position = new Vector2(xMax, yMax);
        vh.AddVert(position, rawImage.color, GetSimpleVertUV(position));
        position = new Vector2(xMax, yMin);
        vh.AddVert(position, rawImage.color, GetSimpleVertUV(position));

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }

    private void AddSimpleCircle(VertexHelper vh, Fillet fillet) {
        var rect = graphic.GetPixelAdjustedRect();
        var center = Vector3.zero;
        switch (fillet) {
            case Fillet.TopLeft:
                center = new Vector3(rect.xMin + radius, rect.yMax - radius);
                break;
            case Fillet.TopRight:
                center = new Vector3(rect.xMax - radius, rect.yMax - radius);
                break;
            case Fillet.BottomLeft:
                center = new Vector3(rect.xMin + radius, rect.yMin + radius);
                break;
            case Fillet.BottomRight:
                center = new Vector3(rect.xMax - radius, rect.yMin + radius);
                break;
        }
        var startIndex = vh.currentVertCount;
        var centerVert = GetSimpleVert(center);
        vh.AddVert(centerVert);
        var interval = radius / segments;
        for (int i = 0; i <= segments; i++) {
            var point = Vector3.zero;
            switch (fillet) {
                case Fillet.TopLeft: {
                    point.x = rect.xMin + i * interval;
                    var offsetX = rect.xMin + radius - point.x;
                    point.y = rect.yMax - radius + Mathf.Sqrt(Mathf.Max(0, radius * radius - offsetX * offsetX));
                    break;
                }
                case Fillet.TopRight: {
                    point.x = rect.xMax - radius + i * interval;
                    var offsetX = i * interval;
                    point.y = rect.yMax - radius + Mathf.Sqrt(Mathf.Max(0, radius * radius - offsetX * offsetX));
                    break;
                }
                case Fillet.BottomLeft: {
                    point.x = rect.xMin + i * interval;
                    var offsetX = rect.xMin + radius - point.x;
                    point.y = rect.yMin + radius - Mathf.Sqrt(Mathf.Max(0, radius * radius - offsetX * offsetX));
                    break;
                }
                case Fillet.BottomRight: {
                    point.x = rect.xMax - radius + i * interval;
                    var offsetX = i * interval;
                    point.y = rect.yMin + radius - Mathf.Sqrt(Mathf.Max(0, radius * radius - offsetX * offsetX));
                    break;
                }
            }
            var vert = GetSimpleVert(point);
            vh.AddVert(vert);
        }
        for (int i = 1; i <= segments; i++) {
            if (fillet == Fillet.TopLeft || fillet == Fillet.TopRight) {
                vh.AddTriangle(startIndex + i, startIndex + i + 1, startIndex);
            } else {
                vh.AddTriangle(startIndex + i, startIndex, startIndex + i + 1);
            }
        }
    }

    private UIVertex GetSimpleVert(Vector3 position) {
        var vertex = new UIVertex();
        vertex.position = position;
        vertex.color = rawImage.color;
        vertex.uv0 = GetSimpleVertUV(position);
        return vertex;
    }

    private Vector2 GetSimpleVertUV(Vector2 position) {
        var rect = graphic.GetPixelAdjustedRect();
        var uv = new Vector4(0, 0, 1, 1);
        var posMin = new Vector2(rect.xMin, rect.yMin);
        var tileUV = new Vector2(uv.x, uv.y);
        if (posMin != position) {
            tileUV = new Vector2(uv.x + (uv.z - uv.x) * (position.x - posMin.x) / rect.width, uv.y + (uv.w - uv.y) * (position.y - posMin.y) / rect.height);
        }
        return tileUV;
    }
}