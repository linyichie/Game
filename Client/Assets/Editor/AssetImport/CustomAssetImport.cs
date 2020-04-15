using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public class CustomAssetImport : AssetPostprocessor {

    public const string Platform_Standalone = "Standalone";
    public const string Platform_iPhone = "iPhone";
    public const string Platform_Android = "Android";

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths) {
        for (int i = 0; i < importedAssets.Length; i++) {
            OnPoseprocessWithExtension(importedAssets[i]);
        }
    }

    private void OnPostprocessAnimation(GameObject root, AnimationClip clip) {
        throw new NotImplementedException();
    }

    private void OnPostprocessAudio(AudioClip arg) {
        throw new NotImplementedException();
    }

    private void OnPostprocessCubemap(Cubemap texture) {
        throw new NotImplementedException();
    }

    private void OnPostprocessMaterial(Material material) {
        throw new NotImplementedException();
    }

    private void OnPostprocessModel(GameObject g) {
        throw new NotImplementedException();
    }

    private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites) {
        throw new NotImplementedException();
    }

    private void OnPostprocessTexture(Texture2D texture) {
        throw new NotImplementedException();
    }

    private void OnPostprocessMeshHierarchy(GameObject root) {
        throw new NotImplementedException();
    }

    private void OnPostprocessSpeedTree(GameObject arg) {
        throw new NotImplementedException();
    }

    private void OnPostprocessAssetbundleNameChanged(string assetPath, string previousAssetBundleName,
        string newAssetBundleName) {
        throw new NotImplementedException();
    }

    private void OnPostprocessGameObjectWithUserProperties(GameObject go, string[] propNames, object[] values) {
        throw new NotImplementedException();
    }

    private void
        OnPostprocessGameObjectWithAnimatedUserProperties(GameObject gameObject, EditorCurveBinding[] bindings) {
        throw new NotImplementedException();
    }

    private static void OnPoseprocessWithExtension(string assetName) {
        if (assetName.EndsWith(".spriteatlas")) {
            SpriteAtlasAssetImporter.OnPostprocessSpriteAtlas(assetName);
        }
    }
}