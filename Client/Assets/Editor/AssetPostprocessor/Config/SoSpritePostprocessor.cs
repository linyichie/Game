using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class SoSpritePostprocessor : SoTexturePostprocessorBase {
        private static readonly string path = "Assets/Editor/AssetPostprocessor/Config/Sprite/SoSpritePostprocessor.asset";
        private static readonly string defaultPath = "Assets/Editor/AssetPostprocessor/Config/Sprite/DefaultSoSpritePostprocessor.asset";

        [MenuItem("Tools/资源导入规范/Sprite")]
        static void Create() {
            Create(path);
        }

        static SoSpritePostprocessor Create(string path) {
            var so = ScriptableObject.CreateInstance<SoSpritePostprocessor>();
            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.Refresh();
            Selection.activeObject = so;
            return so;
        }

        public static SoSpritePostprocessor GetSoSpriteImporter(string path) {
            var so = AssetDatabase.LoadAssetAtPath<SoSpritePostprocessor>(path);

            return so;
        }

        public static SoSpritePostprocessor GetDefaultSoPostprocessor() {
            var so = AssetDatabase.LoadAssetAtPath<SoSpritePostprocessor>(defaultPath);
            if (so == null) {
                so = Create(defaultPath);
            }

            return so;
        }
    }

    [CustomEditor(typeof(SoSpritePostprocessor))]
    public class SoSpriteImpoterInspector : SoTexturePostprocessorBaseInspector {
    }
}