using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Funny.AssetPostprocessor {
    public class SoSpritePostprocessor : SoTexturePostprocessorBase {
        private static readonly string path = "Assets/Editor/AssetPostprocessor/Config/Sprite/New Postprocessor.asset";
        private static readonly string defaultPath = "Assets/Editor/AssetPostprocessor/Config/Sprite/DefaultSoSpritePostprocessor.asset";

        [MenuItem("Funny/资源导入规范/Sprite")]
        internal static SoSpritePostprocessor Create() {
            return Create(path);
        }

        static SoSpritePostprocessor Create(string path) {
            var so = ScriptableObject.CreateInstance<SoSpritePostprocessor>();
            AssetDatabase.CreateAsset(so, path);
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