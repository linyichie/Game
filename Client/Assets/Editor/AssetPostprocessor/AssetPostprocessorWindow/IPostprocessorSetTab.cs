using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinChunJie.AssetPostprocessor {
    public interface IPostprocessorSetTab {
        void Initialize(string guid);
        void OnGUI(Rect pos);
    }
}