﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public static class ModelAssetPostprocessor {
        public static void OnPostprocessModel(ModelImporter importer) {
            var postprocessorFolder = SoAssetPostprocessorFolder.GetSoAssetPostprocessorFolder();
            var guid = postprocessorFolder.Get(PostprocessorAssetType.Model, importer.assetPath);
            if (string.IsNullOrEmpty(guid)) {
                return;
            }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<SoModelPostprocessor>(path);
            if (so == null) {
                so = SoAssetPostprocessor.GetDefault(PostprocessorAssetType.Model) as SoModelPostprocessor;
            }

            SetSceneSettings(importer, so);
            SetMeshSettings(importer, so);
            SetMaterialSettings(importer, so);
            SetAnimationSettings(importer, so);
            Reimport(importer);
        }

        public static void SetSceneSettings(ModelImporter importer, SoModelPostprocessor so) {
            importer.globalScale = so.GlobalScale;
            importer.importBlendShapes = so.ImportBlendShapes;
            importer.importVisibility = so.ImportVisibility;
            importer.importCameras = so.ImportCameras;
            importer.importLights = so.ImportLights;
        }

        public static void SetMeshSettings(ModelImporter importer, SoModelPostprocessor so) {
            importer.meshCompression = so.MeshCompression;
            importer.isReadable = so.IsReadable;
        }

        public static void SetMaterialSettings(ModelImporter importer, SoModelPostprocessor so) {
            importer.importMaterials = so.ImportMaterials;
            if (importer.importMaterials && so.SetMaterialMissing) {
                RemoveImportMaterial(importer);
            }
        }

        static void RemoveImportMaterial(ModelImporter importer) {
            using (var serializedObject = new SerializedObject(importer)) {
                var externalObjects = serializedObject.FindProperty("m_ExternalObjects");
                var materials = serializedObject.FindProperty("m_Materials");

                for (int materialIndex = 0; materialIndex < materials.arraySize; materialIndex++) {
                    var id = materials.GetArrayElementAtIndex(materialIndex);
                    var name = id.FindPropertyRelative("name").stringValue;
                    var type = id.FindPropertyRelative("type").stringValue;
                    var assembly = id.FindPropertyRelative("assembly").stringValue;

                    SerializedProperty materialProperty = null;

                    for (int objectIndex = 0; objectIndex < externalObjects.arraySize; objectIndex++) {
                        var pair = externalObjects.GetArrayElementAtIndex(objectIndex);
                        var externalName = pair.FindPropertyRelative("first.name").stringValue;
                        var externalType = pair.FindPropertyRelative("first.type").stringValue;

                        if (externalName == name && externalType == type) {
                            materialProperty = pair.FindPropertyRelative("second");
                            break;
                        }
                    }

                    if (materialProperty != null) {
                        materialProperty.objectReferenceValue = null;
                    } else {
                        var index = externalObjects.arraySize++;
                        var pair = externalObjects.GetArrayElementAtIndex(index);
                        pair.FindPropertyRelative("first.name").stringValue = name;
                        pair.FindPropertyRelative("first.type").stringValue = type;
                        pair.FindPropertyRelative("first.assembly").stringValue = assembly;
                        pair.FindPropertyRelative("second").objectReferenceValue = null;
                    }
                }

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public static void SetAnimationSettings(ModelImporter importer, SoModelPostprocessor so) {
            importer.animationType = so.AnimationType;
            importer.importAnimation = so.ImportAnimation;
            if (importer.importAnimation) {
                importer.animationCompression = so.AnimationCompression;

                if (so.IsRoleAnimation) {
                    var avatarPath = AssetDatabase.GUIDToAssetPath(so.SourceAvatarGuid);
                    var sourceAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);
                    if (sourceAvatar == null) {
                        throw new Exception("未指定 Source Avatar，请检查配置：" + AssetDatabase.GetAssetPath(so));
                    }

                    importer.sourceAvatar = sourceAvatar;

                    ModelImporterClipAnimation[] anim = new ModelImporterClipAnimation[1];
                    anim[0] = new ModelImporterClipAnimation();
                    anim[0].name = importer.defaultClipAnimations[0].name;
                    anim[0].firstFrame = importer.defaultClipAnimations[0].firstFrame;
                    anim[0].lastFrame = importer.defaultClipAnimations[0].lastFrame;
                    anim[0].lockRootHeightY = true;
                    anim[0].lockRootPositionXZ = true;
                    anim[0].lockRootRotation = true;
                    anim[0].keepOriginalOrientation = true;
                    anim[0].keepOriginalPositionXZ = true;
                    anim[0].keepOriginalPositionY = true;

                    importer.clipAnimations = anim;
                }
            }
        }

        static async Task Reimport(ModelImporter importer) {
            await Task.Delay(1);
            importer.SaveAndReimport();
        }
    }
}