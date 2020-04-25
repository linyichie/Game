﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class SoTexturePostprocessor : SoTexturePostprocessorBase {
        private static readonly string path = "Assets/Editor/AssetPostprocessor/Config/Sprite/New Postprocessor.asset";
        private static readonly string defaultPath = "Assets/Editor/AssetPostprocessor/Config/Sprite/DefaultSoSpritePostprocessor.asset";

        [MenuItem("Tools/资源导入规范/Texture")]
        public static SoTexturePostprocessor Create() {
            return Create(path);
        }

        static SoTexturePostprocessor Create(string path) {
            var so = ScriptableObject.CreateInstance<SoTexturePostprocessor>();
            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.Refresh();
            Selection.activeObject = so;
            return so;
        }

        public static SoTexturePostprocessor GetSoTextureImporter(string path) {
            var so = AssetDatabase.LoadAssetAtPath<SoTexturePostprocessor>(path);
            return so;
        }

        public static SoTexturePostprocessor GetDefaultSoPostprocessor() {
            var so = AssetDatabase.LoadAssetAtPath<SoTexturePostprocessor>(defaultPath);
            if (so == null) {
                so = Create(defaultPath);
            }

            return so;
        }
    }

    [CustomEditor(typeof(SoTexturePostprocessor))]
    public class SoTexturePostprocessorInspector : SoTexturePostprocessorBaseInspector {
    }
}