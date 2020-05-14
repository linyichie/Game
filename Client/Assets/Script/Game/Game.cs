using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : SingletonMonobehaviour<Game> {
    // Start is called before the first frame update
    void Start() {
        WindowController.Instance.OpenWindow("Launch");
    }

    // Update is called once per frame
    void Update() {
    }
}