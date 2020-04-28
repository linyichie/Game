using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Funny.AssetPostprocessor {
    public class AssetModelItem : AssetListItem {
        public AssetModelItem(string path, int depth, string displayName) : base(path, depth, displayName) { }

        public override void VerifyAssetState(SoAssetPostprocessor so) {
            IsChanged = false;
            
            var modelPostprocessor = so as SoModelPostprocessor;
            if(!ModelAssetPostprocessor.CompareSettings(GetAssetImporter<ModelImporter>(), modelPostprocessor)) {
                IsChanged = true;
            }

            IsDirty = false;
        }

        public override void VerifyAssetError(SoAssetPostprocessor so) {
            IsErrorDirty = false;
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
            var modelPostprocessor = so as SoModelPostprocessor;
            ModelAssetPostprocessor.SetSettings(GetAssetImporter<ModelImporter>(), modelPostprocessor);
            EditorUtility.SetDirty(modelPostprocessor);
        }
    }
}