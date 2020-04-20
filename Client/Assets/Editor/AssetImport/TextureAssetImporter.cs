using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class TextureAssetImporter {
    public static void OnPostprocessTexture(TextureImporter importer) {
        if (importer.assetPath.Contains("Sprite")) {
            importer.textureType = TextureImporterType.Sprite;
        }
        else if (importer.assetPath.Contains("Texture")) {
            importer.textureType = TextureImporterType.Default;
        }
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.isReadable = false;
        importer.mipmapEnabled = false;
        importer.streamingMipmaps = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
    }
}