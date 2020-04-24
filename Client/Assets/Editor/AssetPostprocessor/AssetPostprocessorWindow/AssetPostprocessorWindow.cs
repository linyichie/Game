﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace LinChunJie.AssetPostprocessor {
    public class AssetPostprocessorWindow : EditorWindow {
        [MenuItem("Tools/资源导入工具")]
        private static void Open() {
            var window = EditorWindow.CreateInstance<AssetPostprocessorWindow>();
            window.autoRepaintOnSceneChange = true;
            window.titleContent = new GUIContent("资源导入工具");
            window.Show();
        }

        private static readonly float toolBarHeight = 20;
        private static readonly float splitterWidth = 3f;

        private readonly Rect toolBarRect;
        private readonly string[] assetTypeOptions;

        private AssetPostprocessorConfigTab configTab;
        private AssetPostprocessorFolderTab folderTab;
        private AssetPostprocessorSetTab setTab;
        private AssetListTab assetListTab;

        [SerializeField] private float horizontalLeftSplitterPercent;
        [SerializeField] private float horizontalRightSplitterPercent;
        [SerializeField] private float verticalSplitterPercent;

        private bool resizingHorizontalLeftSplitter;
        private bool resizingHorizontalRightSplitter;
        private bool resizingVerticalSplitter;
        private Rect horizontalLeftSplitterRect, horizontalRightSplitterRect, verticalSplitterRect;
        private Rect subRect;

        private AssetPostprocessorHelper.PostprocessorAssetType selectAssetType = AssetPostprocessorHelper.PostprocessorAssetType.SpriteAtlas;

        private AssetPostprocessorWindow() {
            toolBarRect = new Rect(20, splitterWidth, position.width, toolBarHeight);
            horizontalLeftSplitterPercent = 0.4f;
            horizontalRightSplitterPercent = 0.7f;
            verticalSplitterPercent = 0.5f;
            assetTypeOptions = Enum.GetNames(typeof(AssetPostprocessorHelper.PostprocessorAssetType));
        }

        private void OnEnable() {
            subRect = GetSubWindowArea();
            horizontalLeftSplitterRect = new Rect(subRect.x + subRect.width * horizontalLeftSplitterPercent, subRect.y, splitterWidth, subRect.height);
            horizontalRightSplitterRect = new Rect(subRect.x + subRect.width * horizontalRightSplitterPercent - splitterWidth, subRect.y, splitterWidth, subRect.height);
            verticalSplitterRect = new Rect(subRect.x + subRect.width * horizontalLeftSplitterPercent + splitterWidth, subRect.y + subRect.height * verticalSplitterPercent, horizontalRightSplitterRect.x - horizontalLeftSplitterRect.x - splitterWidth, splitterWidth);
        }

        private void OnDisable() {
        }

        private void OnGUI() {
            subRect = GetSubWindowArea();
            ShowToggle();
            ShowSubTabs();
        }

        private void ShowToggle() {
            selectAssetType = (AssetPostprocessorHelper.PostprocessorAssetType)GUI.Toolbar(toolBarRect, (int)selectAssetType, assetTypeOptions);
        }

        private void ShowSubTabs() {
            subRect = GetSubWindowArea();
            if (configTab == null) {
                folderTab = folderTab ?? AssetPostprocessorFolderTab.Get(this);
                configTab = configTab ?? AssetPostprocessorConfigTab.Get();
                setTab = setTab ?? new AssetPostprocessorSetTab();
                assetListTab = assetListTab ?? new AssetListTab();
            }

            HandleHorizontalResize();
            HandleVerticalResize();

            var assetFolderRect = new Rect(subRect.x, subRect.y, subRect.width * horizontalLeftSplitterPercent, subRect.height);

            var assetListRect = new Rect(subRect.x + subRect.width * horizontalRightSplitterPercent, subRect.y, subRect.width * (1 - horizontalRightSplitterPercent), subRect.height);

            var configTabRect = new Rect(subRect.x + assetFolderRect.width + splitterWidth, subRect.y, assetListRect.x - assetFolderRect.xMax - 2 * splitterWidth, subRect.height * verticalSplitterPercent);

            var detailTabRect = new Rect(subRect.x + assetFolderRect.width + splitterWidth, subRect.y + configTabRect.height + splitterWidth, configTabRect.width, subRect.height - configTabRect.height - splitterWidth);

            folderTab.SetAssetType(selectAssetType);
            var selectFolderPath = folderTab.GetSelectPostprocessorFolder();
            configTab.SetAssetType(selectAssetType);
            configTab.SetPostprocessorFolder(selectFolderPath);
            var selectGuid = configTab.GetSelectSoPostprocessorGuid();
            setTab.SetPostprocessor(selectAssetType, selectGuid);

            configTab.OnGUI(configTabRect);
            folderTab.OnGUI(assetFolderRect);
            setTab.OnGUI(detailTabRect);
            assetListTab.OnGUI(assetListRect);
        }
        private void HandleHorizontalResize() {
            horizontalLeftSplitterRect.x = (int) (subRect.x + subRect.width * horizontalLeftSplitterPercent);
            horizontalRightSplitterRect.x = (int) (subRect.x + subRect.width * horizontalRightSplitterPercent - splitterWidth);

            EditorGUIUtility.AddCursorRect(horizontalLeftSplitterRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown && horizontalLeftSplitterRect.Contains(Event.current.mousePosition)) {
                resizingHorizontalLeftSplitter = true;
            }

            EditorGUIUtility.AddCursorRect(horizontalRightSplitterRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown && horizontalRightSplitterRect.Contains(Event.current.mousePosition)) {
                resizingHorizontalRightSplitter = true;
            }

            if (resizingHorizontalLeftSplitter) {
                horizontalLeftSplitterPercent = Mathf.Clamp(Event.current.mousePosition.x / subRect.width, 0.1f, horizontalRightSplitterPercent - 0.1f);
                horizontalLeftSplitterRect.x = (int) (subRect.x + subRect.width * horizontalLeftSplitterPercent);
            }

            if (resizingHorizontalRightSplitter) {
                horizontalRightSplitterPercent = Mathf.Clamp(Event.current.mousePosition.x / subRect.width, horizontalLeftSplitterPercent + 0.1f, 0.9f);
                horizontalRightSplitterRect.x = (int) (subRect.x + subRect.width * horizontalRightSplitterPercent - splitterWidth);
            }

            if (resizingHorizontalLeftSplitter || resizingHorizontalRightSplitter) {
                Repaint();
            }

            if (Event.current.type == EventType.MouseUp) {
                resizingHorizontalLeftSplitter = false;
                resizingHorizontalRightSplitter = false;
            }
        }

        private void HandleVerticalResize() {
            verticalSplitterRect.x = (int) (subRect.x + subRect.width * horizontalLeftSplitterPercent + splitterWidth);
            verticalSplitterRect.y = (int) (subRect.y + subRect.height * verticalSplitterPercent);
            verticalSplitterRect.width = (int) (horizontalRightSplitterRect.x - horizontalLeftSplitterRect.x - splitterWidth);

            EditorGUIUtility.AddCursorRect(verticalSplitterRect, MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && verticalSplitterRect.Contains(Event.current.mousePosition)) {
                resizingVerticalSplitter = true;
            }

            if (resizingVerticalSplitter) {
                verticalSplitterPercent = Mathf.Clamp((Event.current.mousePosition.y - subRect.y) / subRect.height, 0.25f, 0.75f);
                verticalSplitterRect.y = (int) (subRect.y + subRect.height * verticalSplitterPercent);
            }

            if (resizingVerticalSplitter) {
                Repaint();
            }

            if (Event.current.type == EventType.MouseUp) {
                resizingVerticalSplitter = false;
            }
        }

        private Rect GetSubWindowArea() {
            var rect = new Rect(0, 0, position.width, position.height);
            rect.y = rect.y + toolBarHeight + splitterWidth * 2;
            rect.height = rect.height - toolBarHeight - splitterWidth;
            return rect;
        }
    }
}