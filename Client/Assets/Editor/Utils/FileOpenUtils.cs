using System;
using System.IO;
using System.Text;
using System.Threading;
using NPOI.HSSF.UserModel;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class FileOpenUtils {
    [OnOpenAsset(1)]
    static bool OpenAssetStep1(int instanceId, int line) {
        var path = AssetDatabase.GetAssetPath(instanceId);
        if (ConfigFileOpenUtils.VerifyConfigFile(path)) {
            ConfigFileOpenUtils.StartOpenConfigFile(path);
            return true;
        }
        return false;
    }

    [OnOpenAsset(2)]
    static bool OpenAssetStep2(int instanceId, int line) {
        return false;
    }
}