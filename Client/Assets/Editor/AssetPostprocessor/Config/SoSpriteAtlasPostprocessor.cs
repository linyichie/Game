using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class SoSpriteAtlasPostprocessor : SoTexturePostprocessorBase {
        private static readonly string path = "Assets/Editor/AssetPostprocessor/Config/SpriteAtlas/SoSpriteAtlasPostprocessor.asset";
        private static readonly string defaultPath = "Assets/Editor/AssetPostprocessor/Config/SpriteAtlas/DefaultSoSpriteAtlasPostprocessor.asset";

        [MenuItem("Tools/资源导入规范/SpriteAtlas")]
        static void Create() {
            Create(path);
        }

        static SoSpriteAtlasPostprocessor Create(string path) {
            var so = ScriptableObject.CreateInstance<SoSpriteAtlasPostprocessor>();
            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.Refresh();
            Selection.activeObject = so;
            return so;
        }

        public static SoSpriteAtlasPostprocessor GetSoSpriteAtlasImporter(string path) {
            var so = AssetDatabase.LoadAssetAtPath<SoSpriteAtlasPostprocessor>(path);
            return so;
        }

        public static SoSpriteAtlasPostprocessor GetDefaultSoPostprocessor() {
            var so = AssetDatabase.LoadAssetAtPath<SoSpriteAtlasPostprocessor>(defaultPath);
            if (so == null) {
                so = Create(defaultPath);
            }

            return so;
        }
    }

    [CustomEditor(typeof(SoSpriteAtlasPostprocessor))]
    public class SoSpriteAtlasPostprocessorInspector : SoTexturePostprocessorBaseInspector {
    }
}