using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XLua.LuaCallCSharp]
public class WindowAnimation : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private float duration;
    
    public event Action onAnimationComplete;

    public virtual void StartAnimation() {
        
    }

    public virtual void StopAnimation() {
        
    }
}