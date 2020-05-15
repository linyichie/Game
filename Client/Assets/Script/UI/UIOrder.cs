using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOrder : MonoBehaviour {
    [SerializeField] private int order;
    [SerializeField] private UIOrderType uiOrderType;

    private List<Renderer> renderers;
    private Canvas canvas;

    private void Awake() {
        switch(uiOrderType) {
            case UIOrderType.Canvas:
                canvas = GetComponent<Canvas>();
                break;
        }
    }

    public void SetOrder(int parentOrder) {
        switch(uiOrderType) {
            case UIOrderType.Canvas:
                if(canvas != null) {
                    canvas.sortingOrder = parentOrder + order;
                }

                break;
        }
    }

    enum UIOrderType {
        Canvas,
    }
}