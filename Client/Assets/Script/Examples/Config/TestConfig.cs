using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class TestConfig : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        ConfigLoader.Initialize();
    }

    // Update is called once per frame
    void Update() {
        if (ConfigLoader.compelted) {
            Debug.Log(GeoConfig.Get(11).polygon);
        }
    }
}