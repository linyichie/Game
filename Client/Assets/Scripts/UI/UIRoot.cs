using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRoot : MonoBehaviour {
    [SerializeField] private Transform root;
    [SerializeField] private Camera camera;

    public Transform Root {
        get {
            return root;
        }
    }

    public Camera UICamera {
        get {
            return camera;
        }
    }
}