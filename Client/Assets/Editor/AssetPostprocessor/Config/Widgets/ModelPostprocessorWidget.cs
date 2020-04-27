using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class ModelPostprocessorWidget : IAssetPostprocessorWidget {
        private readonly SoModelPostprocessor so;
        private readonly bool allowEdit;
        private Styles styles;
        private Avatar sourceAvatar;
        private Vector2 scrollPosition;

        public event Action OnChanged;

        class Styles {
            public readonly GUIContent SceneContent = new GUIContent("Scene");
            public readonly GUIContent GlobalScaleContent = new GUIContent("Scale Factor");
            public readonly GUIContent BlendShapesContent = new GUIContent("Import BlendShapes");
            public readonly GUIContent VisibilityContent = new GUIContent("Import Visibility");
            public readonly GUIContent CamerasContent = new GUIContent("Import Cameras");
            public readonly GUIContent LightsContent = new GUIContent("Import Lights");

            public readonly GUIContent MeshContent = new GUIContent("Meshs");
            public readonly GUIContent MeshCompressionContent = new GUIContent("Mesh Compression");
            public readonly GUIContent ReadableContent = new GUIContent("Read/Write Enabled");

            public readonly GUIContent MaterialContent = new GUIContent("Materials");
            public readonly GUIContent ImportMaterialContent = new GUIContent("Import Materials");
            public readonly GUIContent MaterialMissingContent = new GUIContent("Set Materials Missing");

            public readonly GUIContent AnimationContent = new GUIContent("Animations");
            public readonly GUIContent AnimationTypeContent = new GUIContent("Animation Type");
            public readonly GUIContent AnimationCompressionContent = new GUIContent("Animation Compression");
            public readonly GUIContent ImportAnimationContent = new GUIContent("Import Animation");
            public readonly GUIContent RoleAnimationContent = new GUIContent("Role Animation");
            public readonly GUIContent SourceAvatarContent = new GUIContent("Source Avatar");

            public readonly GUIStyle labelBoldStyle = new GUIStyle() {
                fontStyle = FontStyle.Bold,
            };

            public Styles() {
            }
        }

        private ModelPostprocessorWidget() {
            if (so != null) {
                var assetPath = AssetDatabase.GUIDToAssetPath(so.SourceAvatarGuid);
                sourceAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(assetPath);
            }
        }

        public ModelPostprocessorWidget(string guid, bool allowEdit) : this() {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            this.so = AssetDatabase.LoadAssetAtPath<SoModelPostprocessor>(path);
            this.allowEdit = allowEdit;
        }

        public ModelPostprocessorWidget(SoModelPostprocessor so, bool allowEdit) : base() {
            this.so = so;
            this.allowEdit = allowEdit;
        }

        public void OnGUI() {
            if (so == null) {
                return;
            }

            styles = styles ?? new Styles();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            EditorGUI.BeginChangeCheck();
            using (new EditorGUI.DisabledScope(!this.allowEdit)) {
                EditorGUILayout.LabelField(styles.SceneContent, styles.labelBoldStyle);
                so.GlobalScale = EditorGUILayout.FloatField(styles.GlobalScaleContent, so.GlobalScale);
                so.ImportBlendShapes = EditorGUILayout.Toggle(styles.BlendShapesContent, so.ImportBlendShapes);
                so.ImportVisibility = EditorGUILayout.Toggle(styles.VisibilityContent, so.ImportVisibility);
                so.ImportCameras = EditorGUILayout.Toggle(styles.CamerasContent, so.ImportCameras);
                so.ImportLights = EditorGUILayout.Toggle(styles.LightsContent, so.ImportLights);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(styles.MeshContent, styles.labelBoldStyle);
                so.MeshCompression = (ModelImporterMeshCompression) EditorGUILayout.EnumPopup(styles.MeshCompressionContent, so.MeshCompression);
                so.IsReadable = EditorGUILayout.Toggle(styles.ReadableContent, so.IsReadable);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(styles.MaterialContent, styles.labelBoldStyle);
                so.ImportMaterials = EditorGUILayout.Toggle(styles.ImportMaterialContent, so.ImportMaterials);
                so.SetMaterialMissing = EditorGUILayout.Toggle(styles.MaterialMissingContent, so.SetMaterialMissing);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(styles.AnimationContent, styles.labelBoldStyle);
                so.ImportAnimation = EditorGUILayout.Toggle(styles.ImportAnimationContent, so.ImportAnimation);
                so.AnimationType = (ModelImporterAnimationType) EditorGUILayout.EnumPopup(styles.AnimationTypeContent, so.AnimationType);
                if (so.ImportAnimation) {
                    so.AnimationCompression = (ModelImporterAnimationCompression) EditorGUILayout.EnumPopup(styles.AnimationCompressionContent, so.AnimationCompression);
                    so.IsRoleAnimation = EditorGUILayout.Toggle(styles.RoleAnimationContent, so.IsRoleAnimation);
                    if (so.IsRoleAnimation) {
                        EditorGUI.BeginChangeCheck();
                        sourceAvatar = EditorGUILayout.ObjectField(styles.SourceAvatarContent, sourceAvatar, typeof(Avatar), false) as Avatar;
                        if (EditorGUI.EndChangeCheck()) {
                            if (sourceAvatar == null) {
                                so.SourceAvatarGuid = string.Empty;
                            } else {
                                var path = AssetDatabase.GetAssetPath(sourceAvatar);
                                so.SourceAvatarGuid = AssetDatabase.AssetPathToGUID(path);
                            }
                        }
                    }
                }

                GUILayout.Space(30);
            }

            GUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(so);
                AssetDatabase.SaveAssets();
                OnChanged?.Invoke();
            }
        }
    }
}