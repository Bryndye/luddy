using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
namespace FlexLayout
{
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/Flex Layout/UI Flex Item")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class UIFlexItem : UIBehaviour, IComparable<UIFlexItem>, FlexItem
    {
        public enum Alignment
        {
            Stretch,
            Start,
            Center,
            End
        }

        public int order        = 0;
        public float flexGrow   = 0f;
        public float flexShrink = 1f;
        public UISize2D flexBasis   = new UISize2D() { width = UISize.pixels100, height = UISize.pixels100 };
        public UISize2D flexMinimum = default;
        public UISize2D flexMaximum = default;
        public Bool2D flexMinimumEnabled    = default;
        public Bool2D flexMaximumEnabled    = default;
        public bool alignSelfEnabled    = false;
        public Alignment alignSelf      = Alignment.Stretch;
        public UIBorder margin          = default;

        public event Action onEvaluate = default;
        public event Action onContainerRebuild = default;
        public event Action onChangeContainer  = default;
        private bool onEvaluateRunning = false;

        private UIFlexContainer container = null;
        private int orderApplied = 0;

        private new RectTransform transform { get { return (base.transform as RectTransform); } }

        public RectTransform rectTransform { get { return (transform); } }


        public float BaseSize(int axis, float relativeSize)
        {
            return (flexBasis[axis].GetSize(relativeSize));
        }

        public float BaseSizeClamped(int axis, float relativeSize)
        {
            float min, max, size;

            min = MinimumSize(axis, relativeSize);
            max = MaximumSize(axis, relativeSize);
            size = BaseSize(axis, relativeSize);
            return (Mathf.Max(min, Mathf.Min(max, size)));
        }

        public float MinimumSize(int axis, float relativeSize)
        {
            float min;

            min = 0f;
            if (flexMinimumEnabled[axis])
                min = flexMinimum[axis].GetSize(relativeSize);
            return (min);
        }

        public float MaximumSize(int axis, float relativeSize)
        {
            float max;

            max = float.PositiveInfinity;
            if (flexMaximumEnabled[axis])
                max = flexMaximum[axis].GetSize(relativeSize);
            return (max);
        }

        public float MarginSize(int axis, float relativeSize)
        {
            return (margin.SizeAxis(axis, relativeSize));
        }

        public float MaximumSizeDelta(int axis, float relativeSize)
        {
            return (MaximumSize(axis, relativeSize) - BaseSizeClamped(axis, relativeSize));
        }

        public float MinimumSizeDelta(int axis, float relativeSize)
        {
            return (MinimumSize(axis, relativeSize) - BaseSizeClamped(axis, relativeSize));
        }

        public float BaseSizeMargin(int axis, float relativeSize)
        {
            return (BaseSizeClamped(axis, relativeSize) + MarginSize(axis, relativeSize));
        }

        public float FlexScale(int flexSign)
        {
            if (flexSign == 0)
                return (0f);
            if (flexSign > 0)
                return Mathf.Max(0f, flexGrow);
            return Mathf.Max(0f, flexShrink);
        }

        public bool ShrinkEnabled()
        {
            return (flexShrink > 0f);
        }

        public bool GrowEnabled()
        {
            return (flexGrow > 0f);
        }
        /// <summary>
        /// Item transform rect in global scope.
        /// </summary>
        /// <returns></returns>
        public Rect TransformRect()
        {
            Rect rect;
            Vector2 pos;

            rect = transform.rect;
            pos = (Vector2)transform.position - transform.pivot * rect.size;
            rect.position = pos;
            return (rect);
        }

        /// <summary>
        /// Item size including margins. Percentage margins are omitted when item is not within a container.
        /// </summary>
        /// <returns></returns>
        public Vector2 MarginSize()
        {
            Vector2 contentSize;
            Vector2 size;

            contentSize = Vector2.zero;
            if (HasContainer())
                contentSize = container.ContentSize();
            size = transform.rect.size;
            size.x += margin.SizeAxis(0, contentSize.x);
            size.y += margin.SizeAxis(1, contentSize.y);
            size.x = Mathf.Max(0f, size.x);
            size.y = Mathf.Max(0f, size.y);
            return (size);
        }

        /// <summary>
        /// Item rect in global scope including margins. Percentage margins are omitted when item is not within a container.
        /// </summary>
        /// <returns></returns>
        public Rect MarginRect()
        {
            Vector2 contentSize;
            Rect rect;
            Vector2 pos;
            Vector2 size;

            contentSize = Vector2.zero;
            if (HasContainer())
                contentSize = container.ContentSize();

            rect = transform.rect;
            pos = (Vector2)transform.position - transform.pivot * rect.size;
            pos.x -= margin.SizeLeft(contentSize.x);
            pos.y -= margin.SizeBottom(contentSize.y);
            rect.position = pos;

            size = rect.size;
            size.x += margin.SizeAxis(0, contentSize.x);
            size.y += margin.SizeAxis(1, contentSize.y);
            size.x = Mathf.Max(0f, size.x);
            size.y = Mathf.Max(0f, size.y);
            rect.size = size;

            return (rect);
        }

        /// <summary>
        /// The flex container the item is within. Null if not within an active container.
        /// </summary>
        /// <returns></returns>
        public UIFlexContainer GetContainer()
        {
            return (container);
        }

        public int CompareTo(UIFlexItem other)
        {
            int compare;

            compare = order.CompareTo(other.order);
            if (compare != 0)
                return (compare);
            return (transform.GetSiblingIndex().CompareTo(other.transform.GetSiblingIndex()));
        }

        /// <summary>
        /// Marks its container for rebuilding in the next layout pass. Should be called whenever a public variable has changed to update layout.
        /// </summary>
        public void Revalidate()
        {
            if (onEvaluateRunning)
                return;
            onEvaluateRunning = true;
            onEvaluate?.Invoke();
            onEvaluateRunning = false;
            if (!HasContainer())
                return;
            if (order != orderApplied)
            {
                orderApplied = order;
                container.NotifyOrderChange();
            }
            container.MarkRebuild();
        }
        /// <summary>
        /// Checks whether or not this item is within an active container.
        /// </summary>
        /// <returns></returns>
        public bool HasContainer()
        {
            return (container != null);
        }

        public void NotifyRebuilding()
        {
            onContainerRebuild?.Invoke();
        }

        /// <summary>
        /// Checks and updates the container this item is in.
        /// </summary>
        private void CheckContainer()
        {
            UIFlexContainer container;

            container = null;
            if (transform.parent != null)
                container = transform.parent.GetComponent<UIFlexContainer>();
            if (container != null && !container.isActiveAndEnabled)
                container = null;
            if (this.container != container)
            {
                if (this.container != null)
                    this.container.NotifyItemChange();
                this.container = container;
                onChangeContainer?.Invoke();
                if (container != null)
                    container.NotifyItemChange();
            }
        }

        protected override void Start()
        {
            CheckContainer();
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            CheckContainer();
            Revalidate();
        }
#endif
        protected override void OnEnable()
        {
            if (container != null)
                container.NotifyItemChange();
        }

        protected override void OnDisable()
        {
            if (container != null)
                container.NotifyItemChange();
        }

        protected override void OnTransformParentChanged()
        {
            CheckContainer();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            Revalidate();
        }
#endif
        protected override void OnDidApplyAnimationProperties()
        {
            Revalidate();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            Revalidate();
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!UnityEditor.Selection.gameObjects.Contains(gameObject))
                return;
            UIGizmos.SetColorMargin();
            UIGizmos.DrawRect(MarginRect(), transform);
        }
#endif
    }
}