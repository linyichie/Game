using System;
using System.Collections;
using System.Collections.Generic;
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

    public override void ModifyMesh(VertexHelper vh) {
        vh.Clear();
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

        if (radius > 0) {
            xMin = rect.xMin;
            yMin = rect.yMin;
            xMax = rect.xMin + radius;
            yMax = rect.yMin + radius;

            var minIndex = GetTileVertIndex(new Vector2(xMin, yMin));
            var maxIndex = GetTileVertIndex(new Vector2(xMax, yMax));
            for (int i = 0; i < segments; i++) {
            }
        }
    }

    private void AddRectangle(VertexHelper vh, float xMin, float yMin, float xMax, float yMax) {
        var rect = graphic.GetPixelAdjustedRect();
        var tileWidth = overrideSprite.rect.width;
        var tileHeight = overrideSprite.rect.height;

        var minIndex = GetTileVertIndex(new Vector2(xMin, yMin));
        var maxIndex = GetTileVertIndex(new Vector2(xMax, yMax));

        for (long i = minIndex.y; i < maxIndex.y + 1; i++) {
            var posMin = Vector2.zero;
            var posMax = Vector2.zero;
            posMin.y = Math.Max(rect.yMin + i * tileHeight, yMin);
            posMax.y = Mathf.Min(rect.yMin + (i + 1) * tileHeight, yMax);
            for (long j = minIndex.x; j < maxIndex.x + 1; j++) {
                posMin.x = Math.Max(rect.xMin + j * tileWidth, xMin);
                posMax.x = Mathf.Min(rect.xMin + (j + 1) * tileWidth, xMax);
                AddQuad(vh, posMin, posMax);
            }
        }
    }

    private void AddQuad(VertexHelper vh, Vector2 posMin, Vector2 posMax) {
        int startIndex = vh.currentVertCount;

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
}