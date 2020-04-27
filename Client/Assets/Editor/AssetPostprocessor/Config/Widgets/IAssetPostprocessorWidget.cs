using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAssetPostprocessorWidget {

    event Action OnChanged;
    void OnGUI();
}