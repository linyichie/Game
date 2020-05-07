using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Lifetime;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Funny.AssetPostprocessor {
    public static class ModelAssetPostprocessor {
        private static SoAssetPostprocessorUtils postprocessorUtils = null;

        public static void OnPostprocessModel(ModelImporter importer) {
            postprocessorUtils = postprocessorUtils ? postprocessorUtils : SoAssetPostprocessorUtils.GetSoAssetPostprocessorUtils();
            var guid = postprocessorUtils.Get(PostprocessorAssetType.Model, importer.assetPath);
            if(string.IsNullOrEmpty(guid)) {
                return;
            }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<SoModelPostprocessor>(path);
            if(so == null) {
                so = SoAssetPostprocessor.GetDefault(PostprocessorAssetType.Model) as SoModelPostprocessor;
            }

            SetSettings(importer, so);
            Reimport(importer, so);
        }

        public static void SetSettings(ModelImporter importer, SoModelPostprocessor so) {
            SetSceneSettings(importer, so);
            SetMeshSettings(importer, so);
            SetMaterialSettings(importer, so);
            SetAnimationSettings(importer, so);
        }

        private static void SetSceneSettings(ModelImporter importer, SoModelPostprocessor so) {
            importer.globalScale = so.GlobalScale;
            importer.importBlendShapes = so.ImportBlendShapes;
            importer.importVisibility = so.ImportVisibility;
            importer.importCameras = so.ImportCameras;
            importer.importLights = so.ImportLights;
        }

        private static void SetMeshSettings(ModelImporter importer, SoModelPostprocessor so) {
            importer.meshCompression = so.MeshCompression;
            importer.isReadable = so.IsReadable;
        }

        private static void SetMaterialSettings(ModelImporter importer, SoModelPostprocessor so) {
            importer.importMaterials = so.ImportMaterials;
            importer.materialLocation = so.MaterialLocation;
            if(importer.importMaterials && so.RemoveStandardMaterial && importer.materialLocation == ModelImporterMaterialLocation.InPrefab) {
                RemoveStandardMaterial(importer);
            }
        }

        static void RemoveStandardMaterial(ModelImporter importer) {
            using(var serializedObject = new SerializedObject(importer)) {
                var externalObjects = serializedObject.FindProperty("m_ExternalObjects");
                var materials = serializedObject.FindProperty("m_Materials");

                for(int materialIndex = 0; materialIndex < materials.arraySize; materialIndex++) {
                    var id = materials.GetArrayElementAtIndex(materialIndex);
                    var name = id.FindPropertyRelative("name").stringValue;
                    var type = id.FindPropertyRelative("type").stringValue;
                    var assembly = id.FindPropertyRelative("assembly").stringValue;

                    SerializedProperty materialProperty = null;

                    for(int objectIndex = 0; objectIndex < externalObjects.arraySize; objectIndex++) {
                        var pair = externalObjects.GetArrayElementAtIndex(objectIndex);
                        var externalName = pair.FindPropertyRelative("first.name").stringValue;
                        var externalType = pair.FindPropertyRelative("first.type").stringValue;

                        if(externalName == name && externalType == type) {
                            materialProperty = pair.FindPropertyRelative("second");
                            break;
                        }
                    }

                    if(materialProperty != null) {
                        if(materialProperty.objectReferenceValue != null) {
                            var assetPath = AssetDatabase.GetAssetPath(materialProperty.objectReferenceValue);
                            var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                            if(mat != null && postprocessorUtils.ContainsStandardShader(mat.shader.name)) {
                                materialProperty.objectReferenceValue = null;
                            }
                        }
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

        private static void SetAnimationSettings(ModelImporter importer, SoModelPostprocessor so) {
            importer.animationType = so.AnimationType;
            importer.importAnimation = so.ImportAnimation;
            if(importer.importAnimation) {
                importer.animationCompression = so.AnimationCompression;

                if(so.IsRoleAnimation) {
                    var avatarPath = AssetDatabase.GUIDToAssetPath(so.SourceAvatarGuid);
                    var sourceAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);
                    if(sourceAvatar == null) {
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

        static async Task Reimport(ModelImporter importer, SoModelPostprocessor so) {
            await Task.Delay(1);
            importer.SaveAndReimport();
        }

        public static bool CompareSettings(ModelImporter importer, SoModelPostprocessor so, out string message) {
            var same = true;
            message = string.Empty;
            same &= CompareSceneSetting(importer, so, ref message);
            same &= CompareMeshSetting(importer, so, ref message);
            same &= CompareMaterialSetting(importer, so, ref message);
            same &= CompareAnimationSetting(importer, so, ref message);
            return same;
        }

        static bool CompareSceneSetting(ModelImporter importer, SoModelPostprocessor so, ref string message) {
            var same = true;
            var sameInfo = string.Empty;
            if(importer.globalScale != so.GlobalScale) {
                same = false;
                sameInfo = StringUtil.Contact(sameInfo, "\n", "GlobalScale");
            }

            if(importer.importBlendShapes != so.ImportBlendShapes) {
                same = false;
                sameInfo = StringUtil.Contact(sameInfo, "\n", "ImportBlendShapes");
            }

            if(importer.importVisibility != so.ImportVisibility) {
                same = false;
                sameInfo = StringUtil.Contact(sameInfo, "\n", "ImportVisibility");
            }

            if(importer.importCameras != so.ImportCameras) {
                same = false;
                sameInfo = StringUtil.Contact(sameInfo, "\n", "ImportCameras");
            }

            if(importer.importLights != so.ImportLights) {
                same = false;
                sameInfo = StringUtil.Contact(sameInfo, "\n", "ImportLights");
            }

            if(!same) {
                message = StringUtil.Contact(message, "\n", "<b>Scene</b>", sameInfo);
            }

            return same;
        }

        static bool CompareMeshSetting(ModelImporter importer, SoModelPostprocessor so, ref string message) {
            var same = true;
            var sameInfo = string.Empty;
            if(importer.meshCompression != so.MeshCompression) {
                same = false;
                sameInfo = StringUtil.Contact(sameInfo, "\n", "MeshCompression");
            }

            if(importer.isReadable != so.IsReadable) {
                same = false;
                sameInfo = StringUtil.Contact(sameInfo, "\n", "Read/Write");
            }

            if(!same) {
                message = StringUtil.Contact(message, "\n", "<b>Mesh</b>", sameInfo);
            }

            return same;
        }

        static bool CompareMaterialSetting(ModelImporter importer, SoModelPostprocessor so, ref string message) {
            var same = true;
            var sameInfo = string.Empty;
            if(importer.importMaterials != so.ImportMaterials) {
                same = false;
                sameInfo = StringUtil.Contact(sameInfo, "\n", "ImportMaterials");
            }

            if(importer.materialLocation != so.MaterialLocation) {
                same = false;
                sameInfo = StringUtil.Contact(sameInfo, "\n", "MaterialLocation");
            }

            if(importer.importMaterials && importer.materialLocation == ModelImporterMaterialLocation.InPrefab) {
                var objects = AssetDatabase.LoadAllAssetsAtPath(importer.assetPath);
                if(objects != null && objects.Length > 0) {
                    for(int i = 0; i < objects.Length; i++) {
                        if(objects[i] is Material) {
                            var mat = objects[i] as Material;
                            postprocessorUtils = postprocessorUtils ? postprocessorUtils : SoAssetPostprocessorUtils.GetSoAssetPostprocessorUtils();
                            if(postprocessorUtils.ContainsStandardShader(mat.shader.name)) {
                                same = false;
                                sameInfo = StringUtil.Contact(sameInfo, "\n", "Using Standard Shader");
                                break;
                            }
                        }
                    }
                }
            }

            if(!same) {
                message = StringUtil.Contact(message, "\n", "<b>Material</b>", sameInfo);
            }

            return same;
        }

        static bool CompareAnimationSetting(ModelImporter importer, SoModelPostprocessor so, ref string message) {
            var same = true;
            var sameInfo = string.Empty;
            if(importer.importAnimation != so.ImportAnimation) {
                same = false;
                sameInfo = StringUtil.Contact(sameInfo, "\n", "ImportAnimation");
            }

            if(importer.animationType != so.AnimationType) {
                same = false;
                sameInfo = StringUtil.Contact(sameInfo, "\n", "AnimationType");
            }

            if(importer.importAnimation && so.ImportAnimation) {
                if(importer.animationCompression != so.AnimationCompression) {
                    same = false;
                    sameInfo = StringUtil.Contact(sameInfo, "\n", "AnimationCompression");
                }

                if(so.IsRoleAnimation) {
                    if(importer.sourceAvatar == null) {
                        same = false;
                        sameInfo = StringUtil.Contact(sameInfo, "\n", "Source Avatar is null");
                    } else {
                        var avatarAssetPath = AssetDatabase.GetAssetPath(importer.sourceAvatar);
                        var avatarGuid = AssetDatabase.AssetPathToGUID(avatarAssetPath);
                        if(avatarGuid != so.SourceAvatarGuid) {
                            same = false;
                            sameInfo = StringUtil.Contact(sameInfo, "\n", "SourceAvatar");
                        }
                    }
                }
            }

            if(!same) {
                message = StringUtil.Contact(message, "\n", "<b>Animation</b>", sameInfo);
            }

            return same;
        }
    }
}