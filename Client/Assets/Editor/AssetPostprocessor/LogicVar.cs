using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funny.AssetPostprocessor {
    public struct LogicVar<T> where T : struct {
        public T value;
        public bool dirty;
        public string message;

        public static readonly LogicVar<T> defaultLogic = new LogicVar<T>() {
            value = default,
            dirty = true,
            message = string.Empty,
        };

        public void SetValue(T value) {
            this.value = value;
            this.dirty = false;
        }

        public void SetMessage(string message) {
            this.message = message;
        }

        public void Reset() {
            this.dirty = true;
            this.message = string.Empty;
        }
    }
}