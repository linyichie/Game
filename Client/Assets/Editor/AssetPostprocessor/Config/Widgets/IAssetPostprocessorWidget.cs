using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funny.AssetPostprocessor {
    public interface IAssetPostprocessorWidget {

        event Action OnChanged;
        void OnGUI();
    }
}
