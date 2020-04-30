using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Funny.AssetPostprocessor {
    public class AssetModelItem : AssetListItem {
        public AssetModelItem(string path, int depth, string displayName) : base(path, depth, displayName) { }

        public override void VerifyAssetState(SoAssetPostprocessor so) {
            WarnLogic.SetValue(false);
            
            var modelPostprocessor = so as SoModelPostprocessor;
            string message;
            if(!ModelAssetPostprocessor.CompareSettings(GetAssetImporter<ModelImporter>(), modelPostprocessor, out message)) {
                WarnLogic.SetValue(true);
                WarnLogic.SetMessage(message.TrimStart('\n'));
            }
        }

        public override void VerifyAssetError(SoAssetPostprocessor so) {
            ErrorLogic.SetValue(false);
        }

        public override void FixAndReimport(SoAssetPostprocessor so) {
            var modelPostprocessor = so as SoModelPostprocessor;
            ModelAssetPostprocessor.SetSettings(GetAssetImporter<ModelImporter>(), modelPostprocessor);
            EditorUtility.SetDirty(modelPostprocessor);
        }
    }
}