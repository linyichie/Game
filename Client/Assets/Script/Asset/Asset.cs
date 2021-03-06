﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Funny.Asset {
    public abstract class Asset : IAsset {
        protected event Action<IAsset> Completed;
        public bool isDone { get; protected set; }

        public UnityEngine.Object asset { get; protected set; }

        public Asset() { }

        public abstract void LoadAsync<T>(Action<IAsset> callback) where T : UnityEngine.Object;
        public abstract void Release();
        public abstract void Release(UnityEngine.Object asset);

        protected virtual void OnLoadAssetComplete() {
            isDone = true;
            Completed?.Invoke(this);
        }
    }

    public class AddressableAsset : Asset {
        protected readonly string addressableName;
        protected AsyncOperationHandle asyncOperationHandle;

        public AddressableAsset(string addressableName) : base() {
            this.addressableName = addressableName;
        }

        public override void LoadAsync<T>(Action<IAsset> callback) {
            Completed += callback;

            if(asset != null) {
                base.OnLoadAssetComplete();
                return;
            }

            asyncOperationHandle = Addressables.LoadAssetAsync<T>(addressableName);
            asyncOperationHandle.Completed += OnLoadAsyncCompleted;
        }

        protected virtual void OnLoadAsyncCompleted(AsyncOperationHandle obj) {
            if(!(asyncOperationHandle.Result is UnityEngine.Object)) {
                Debug.LogErrorFormat("资源加载失败:{0}", addressableName);
                return;
            }

            asset = asyncOperationHandle.Result as UnityEngine.Object;
            base.OnLoadAssetComplete();
        }

        public override void Release() {
            if(asset) {
                Addressables.Release(asset);
            }
        }

        public override void Release(UnityEngine.Object asset) { }
    }

    public class PrefabAsset : AddressableAsset {
        public PrefabAsset(string addressableName) : base(addressableName) { }

        public override void LoadAsync<T>(Action<IAsset> callback) {
            Completed += callback;

            if(asset) {
                base.OnLoadAssetComplete();
                return;
            }

            asyncOperationHandle = Addressables.InstantiateAsync(addressableName);
            asyncOperationHandle.Completed += OnLoadAsyncCompleted;
        }

        public override void Release(Object asset) {
            if(asset is GameObject) {
                Addressables.ReleaseInstance(asset as GameObject);
            }
        }
    }

    public interface IAsset {
        bool isDone { get; }
        UnityEngine.Object asset { get; }
        void Release();
        void Release(UnityEngine.Object asset);
        void LoadAsync<T>(Action<IAsset> callback) where T : UnityEngine.Object;
    }

    public enum AssetType {
        Prefab,
    }
}