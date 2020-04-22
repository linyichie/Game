using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public class CustomAssetPostprocessor : AssetPostprocessor {
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths) {
        for (int i = 0; i < importedAssets.Length; i++) {
            OnPoseprocessWithExtension(importedAssets[i]);
        }
    }

    private void OnPostprocessAnimation(GameObject root, AnimationClip clip) {
    }

    private void OnPostprocessAudio(AudioClip arg) {
    }

    private void OnPostprocessCubemap(Cubemap texture) {
    }

    private void OnPostprocessMaterial(Material material) {
    }

    private void OnPostprocessModel(GameObject g) {
    }

    private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites) {
        if (IsNewCreateFile(assetPath)) {
            var importer = assetImporter as TextureImporter;
            TextureAssetImporter.OnPostprocessTexture(importer);
        }
    }

    private void OnPostprocessTexture(Texture2D texture) {
        if (IsNewCreateFile(assetPath)) {
            var importer = assetImporter as TextureImporter;
            TextureAssetImporter.OnPostprocessTexture(importer);
        }
    }

    private void OnPostprocessMeshHierarchy(GameObject root) {
    }

    private void OnPostprocessSpeedTree(GameObject arg) {
    }

    private void OnPostprocessAssetbundleNameChanged(string assetPath, string previousAssetBundleName,
        string newAssetBundleName) {
    }

    private void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] propNames, object[] values) {
    }

    private void
        OnPostprocessGameObjectWithAnimatedUserProperties(GameObject gameObject, EditorCurveBinding[] bindings) {
    }

    private static void OnPoseprocessWithExtension(string assetName) {
        if (assetName.EndsWith(".spriteatlas")) {
            SpriteAtlasAssetPostprocessor.OnPostprocessSpriteAtlas(assetName);
        }
    }

    public static bool IsNewCreateFile(string assetPath) {
        var metaFilePath = StringUtility.Contact(assetPath, ".meta");
        if (!File.Exists(metaFilePath)) {
            return true;
        }
        var fileInfo = new FileInfo(metaFilePath);
        if (fileInfo != null) {
            var seconds = (DateTime.UtcNow - fileInfo.CreationTimeUtc).TotalSeconds;
            return seconds < 3;
        }

        return false;
    }
}