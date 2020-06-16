using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Funny.Tools.UIBatch {

    public class UIBatchAnalyzer {
        [MenuItem("GameObject/UI/UI Batch 分析")]
        static void Start() {
            var go = Selection.activeGameObject;
            if (null == go) {
                return;
            }
            var canvas = go.GetComponent<Canvas>();
            if (canvas == null) {
                return;
            }
            var root = new UIBatchRoot(canvas);
        }
    }

    public class UIBatchRoot : IQuadTree {
        private Component root;
        private List<UIBatchRoot> childRoots = new List<UIBatchRoot>();
        private List<UIBatchElement> elements = new List<UIBatchElement>();

        public UIBatchRoot(Component root) {
            this.root = root;
            Traverse(root.transform);
        }

        private void Traverse(Transform parent) {
            Component[] components = null;
            foreach (Transform child in parent) {
                components = child.GetComponents<Component>();
                var isRoot = false;
                foreach (var component in components) {
                    if (component is Canvas || component is Mask || component is RectMask2D) {
                        var root = new UIBatchRoot(component);
                        childRoots.Add(root);
                        isRoot = true;
                        break;
                    }
                }
                if (!isRoot) {
                    Traverse(child);
                }
            }
            var maskableGraphics = parent.GetComponents<MaskableGraphic>();
            foreach (var maskableGraphic in maskableGraphics) {
                var element = new UIBatchElement(maskableGraphic);
                elements.Add(element);
            }
        }
    }

    public class UIBatchElement : IQuadTreeNode {
        private MaskableGraphic element;
        public Rect rectangle { get; private set; }

        public UIBatchElement(MaskableGraphic element) {
            this.element = element;
            CalculateRectangle();
        }

        private void CalculateRectangle() {
            var rect = element.transform as RectTransform;
            if (element is Image || element is RawImage) {
                rectangle = rect.rect;
            } else if (element is Text) {
                var text = element as Text;
            }
        }
    }

    public interface IQuadTree {
    }

    public interface IQuadTreeNode {
        Rect rectangle { get; }
    }

}