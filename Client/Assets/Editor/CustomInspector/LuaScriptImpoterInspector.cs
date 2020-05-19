using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

[CustomEditor(typeof(LuaScriptImpoter))]
public class LuaScriptImpoterInspector : ScriptedImporterEditor {
    public override void OnInspectorGUI() {
        this.ApplyRevertGUI();
    }
}