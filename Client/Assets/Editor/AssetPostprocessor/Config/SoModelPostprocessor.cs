using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Funny.AssetPostprocessor {
    public class SoModelPostprocessor : SoAssetPostprocessor {
        private static readonly string path = "Assets/Editor/AssetPostprocessor/Config/Model/New Postprocessor.asset";
        private static readonly string defaultPath = "Assets/Editor/AssetPostprocessor/Config/Model/DefaultSoModelPostprocessor.asset";

        public float GlobalScale = 1.0f;
        public bool IsReadable = true;
        public bool ImportBlendShapes = false;
        public bool ImportVisibility = false;
        public bool ImportCameras = false;
        public bool ImportLights = false;
        public bool ImportMaterials = false;
        public bool SetMaterialMissing = true;
        public bool ImportAnimation = false;
        public bool IsRoleAnimation = false;
        public string SourceAvatarGuid = string.Empty;
        public ModelImporterAnimationType AnimationType = ModelImporterAnimationType.Generic;
        public ModelImporterMeshCompression MeshCompression = ModelImporterMeshCompression.Low;
        public ModelImporterAnimationCompression AnimationCompression = ModelImporterAnimationCompression.KeyframeReduction;


        [MenuItem("Funny/资源导入规范/Model")]
        public static SoModelPostprocessor Create() {
            return Create(path);
        }

        static SoModelPostprocessor Create(string path) {
            var so = ScriptableObject.CreateInstance<SoModelPostprocessor>();
            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.Refresh();
            Selection.activeObject = so;
            return so;
        }

        public static SoModelPostprocessor GetSoModelPostprocessor(string path) {
            var so = AssetDatabase.LoadAssetAtPath<SoModelPostprocessor>(path);
            return so;
        }

        public static SoModelPostprocessor GetDefaultSoPostprocessor() {
            var so = AssetDatabase.LoadAssetAtPath<SoModelPostprocessor>(defaultPath);
            if (so == null) {
                so = Create(defaultPath);
            }

            return so;
        }
    }

    [CustomEditor(typeof(SoModelPostprocessor))]
    public class SoModelPostprocessorInspector : Editor {
        private IAssetPostprocessorWidget postprocessorWidget;
        private void OnEnable() {
            postprocessorWidget = postprocessorWidget ?? new ModelPostprocessorWidget(this.target as SoModelPostprocessor, false);
        }

        public override void OnInspectorGUI() {
            postprocessorWidget?.OnGUI();
        }
    }
}