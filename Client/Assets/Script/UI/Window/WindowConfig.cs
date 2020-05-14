using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowConfig : ScriptableObject {
    [SerializeField] private WindowLayer windowLayer;
}

public enum WindowLayer {
    Base,
    Function,
    Tips,
    System
}