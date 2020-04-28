using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public class AssetModelItem : AssetListItem {
        public AssetModelItem(string path, int depth, string displayName) : base(path, depth, displayName) { }

        public override void VerifyAssetState(SoAssetPostprocessor so) {
            IsDirty = false;
        }

        public override void VerifyAssetError(SoAssetPostprocessor so) {
            IsErrorDirty = false;
        }

        public override void FixAndReimport(SoAssetPostprocessor so) { }
    }
}