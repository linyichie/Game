using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour {
    [SerializeField] private WindowConfig windowConfig;
    
    private bool initialized = false;
    private string windowName = string.Empty;

    public WindowState state { get; private set; } = WindowState.Closed;

    public void Open() {
        if (!initialized) {
            try {
                windowName = gameObject.name;
                OnInitialize();
            }
            catch (Exception e) {
                Debug.LogErrorFormat("{0} OnInitialize {1}", windowName, e.ToString());
                initialized = true;
            }
        }

        
        try {
            state = WindowState.ReadyOpen;
            OnReadyOepn();
        }
        catch (Exception e) {
            Debug.LogErrorFormat("{0} OnReadyOepn {1}", windowName, e.ToString());
        }

        gameObject.SetActive(true);
        state = WindowState.OpenAnimation;
    }

    public void Close() {
        try {
            state = WindowState.ReadyClose;
            OnReadyClose();
        }
        catch (Exception e) {
            Debug.LogErrorFormat("{0} OnReadyClose {1}", windowName, e.ToString());
        }

        state = WindowState.CloseAnimation;
    }

    void OnOpenAnimationComplete() {
        try {
            state = WindowState.Opened;
            OnOpened();
        }
        catch (Exception e) {
            Debug.LogErrorFormat("{0} OnOpened {1}", windowName, e.ToString());
        }
    }

    void OnCloseAnimationComplete() {
        try {
            state = WindowState.Closed;
            OnClosed();
        }
        catch (Exception e) {
            Debug.LogErrorFormat("{0} OnClosed {1}", windowName, e.ToString());
        }

        gameObject.SetActive(false);
    }

    #region protected functions

    protected virtual void OnInitialize() {
        
    }

    protected virtual void OnReadyOepn() {
    }

    protected virtual void OnOpenAnimation(Action callback) {
        callback?.Invoke();
    }

    protected virtual void OnOpened() {
    }

    protected virtual void OnLateUpdate() {
    }

    protected virtual void OnReadyClose() {
    }

    protected virtual void OnCloseAnimation(Action callback) {
        callback?.Invoke();
    }

    protected virtual void OnClosed() {
    }

    #endregion

    private void LateUpdate() {
        OnLateUpdate();

        switch (state) {
            case WindowState.ReadyOpen:
                break;
            case WindowState.OpenAnimation:
                try {
                    OnOpenAnimation(OnOpenAnimationComplete);
                }
                catch (Exception e) {
                    Debug.LogErrorFormat("{0} OnOpenAnimation {1}", windowName, e.ToString());
                    OnOpenAnimationComplete();
                }

                break;
            case WindowState.Opened:
                break;
            case WindowState.ReadyClose:
                break;
            case WindowState.CloseAnimation:
                try {
                    OnCloseAnimation(OnCloseAnimationComplete);
                }
                catch (Exception e) {
                    Debug.LogErrorFormat("{0} OnCloseAnimation {1}", windowName, e.ToString());
                }

                break;
            case WindowState.Closed:
                break;
            default:
                break;
        }
    }
}

public enum WindowState {
    ReadyOpen,
    OpenAnimation,
    Opened,
    ReadyClose,
    CloseAnimation,
    Closed
}