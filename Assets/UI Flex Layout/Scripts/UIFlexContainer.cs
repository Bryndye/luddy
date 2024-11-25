using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FlexLayout.Evaluate;
using System.Linq;
using System;

namespace FlexLayout
{
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/Flex Layout/UI Flex Container")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class UIFlexContainer : UIBehaviour, ILayoutGroup, IEnumerable<UIFlexItem>
    {
        public enum Direction
        {
            Horizontal,
            Vertical
        }
        public enum JustifyContent
        {
            Start,
            Center,
            End,
            SpaceBetween,
            SpaceAround,
            SpaceEvenly,
            Custom
        }
        public enum AlignContent
        {
            Start,
            Center,
            End,
            SpaceBetween,
            SpaceAround,
            SpaceEvenly,
            Stretch
        }


        public Direction direction  = Direction.Horizontal;
        public bool reversed        = false;
        public bool wrap            = false;
        public bool wrapReversed    = false;

        public JustifyContent justifyContent    = JustifyContent.Start;
        public Vector3 spacingDistribution      = new Vector3(0, 0, 1);
        public AlignContent alignContent        = AlignContent.Stretch;
        public UIFlexItem.Alignment alignItems  = UIFlexItem.Alignment.Stretch;
        public UISize2D minimumGap              = default;
        public UIBorder padding                 = default;

        public event Action<UIFlexItem> onItemAcquire = default;
        public event Action<UIFlexItem> onItemRelease = default;
        public event Action onItemListChange = default;
        public event Action onRebuilding     = default;
        public event Action onMarkRebuild    = default;
        private bool onMarkRebuildInvoked    = false;

        private Vector2Int mainAxis         = Vector2Int.right;
        private Vector2Int crossAxis        = Vector2Int.down;
        private int[] mainAxisSides         = new int[2] { 3, 1 };
        private int[] crossAxisSides        = new int[2] { 0, 2 };
        private List<UIFlexItem> items      = new List<UIFlexItem>();

        private bool itemChange     = true;
        private bool shouldSort     = true;
        private bool shouldRebuild  = true;

        private DrivenRectTransformTracker tracker = new DrivenRectTransformTracker();
        private int[] lineItemCount;
        private float[] lineCrossSize;
        private float crossSpacing;

        private new RectTransform transform { get { return (base.transform as RectTransform); } }
        public int mainAxisIndex { get { return (int)direction; } }
        public int crossAxisIndex { get { return (1 - (int)direction); } }
        private float mainAxisSize { get { return transform.rect.size[mainAxisIndex]; } }
        private float crossAxisSize { get { return transform.rect.size[crossAxisIndex]; } }
        private float mainAxisContentSize { get { return Mathf.Max(0f, mainAxisSize - padding.SizeAxis(mainAxisIndex, mainAxisSize)); } }
        private float crossAxisContentSize { get { return Mathf.Max(0f, crossAxisSize - padding.SizeAxis(crossAxisIndex, crossAxisSize)); } }

        public int ItemCount { get { return items.Count; } }

        private static int layoutPasses = 0;

        /// <summary>
        /// Size of container area with padding applied.
        /// </summary>
        /// <returns></returns>
        public Vector2 ContentSize()
        {
            Vector2 size;

            size = transform.rect.size;
            size.x -= padding.SizeAxis(0, size.x);
            size.y -= padding.SizeAxis(1, size.y);
            size.x = Mathf.Max(0f, size.x);
            size.y = Mathf.Max(0f, size.y);
            return (size);
        }
        /// <summary>
        /// Rect of container in global scope with padding applied.
        /// </summary>
        /// <returns></returns>
        public Rect ContentRect()
        {
            Rect rect;
            Vector2 pos;

            rect = transform.rect;

            pos = (Vector2)transform.position - transform.pivot * rect.size;
            pos.x += padding.SizeLeft(rect.size.x);
            pos.y += padding.SizeBottom(rect.size.y);
            
            rect.position = pos;
            rect.size = ContentSize();
            return (rect);
        }
        /// <summary>
        /// A list of all active items within the container sorted by order.
        /// </summary>
        /// <returns></returns>
        public List<UIFlexItem> Items()
        {
            UpdateItems();
            SortItems();
            return (new List<UIFlexItem>(items));
        }

        public IEnumerator<UIFlexItem> GetEnumerator()
        {
            UpdateItems();
            SortItems();
            return ((IEnumerable<UIFlexItem>)items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            UpdateItems();
            SortItems();
            return ((IEnumerable)items).GetEnumerator();
        }

        public void SetLayoutHorizontal()
        {
            layoutPasses += 1;
            Rebuild();
        }

        public void SetLayoutVertical()
        { }

        /// <summary>
        /// Marks the container to rebuild its items positions and sizes in the next or current layout pass.
        /// </summary>
        public void MarkRebuild()
        {
            shouldRebuild = true;
            if (!onMarkRebuildInvoked)
            {
                onMarkRebuildInvoked = true;
                onMarkRebuild?.Invoke();
                onMarkRebuildInvoked = false;
            }
            LayoutRebuilder.MarkLayoutForRebuild(transform);
        }

        /// <summary>
        /// Marks the container to sort and rebuild in the next layout pass.
        /// </summary>
        public void NotifyOrderChange()
        {
            shouldSort = true;
            MarkRebuild();
        }

        /// <summary>
        /// Marks the container to check for new or deleted active items and rebuilding in the next layout pass. 
        /// </summary>
        public void NotifyItemChange()
        {
            itemChange = true;
            shouldSort = true;
            MarkRebuild();
        }

        private void Rebuild()
        {
            spacingDistribution = GetSpacingWeights();
            if (!IsActive())
                return;
            UpdateItems();
            SortItems();
            if (!shouldRebuild)
                return;
            NotifyRebuilding();
            ClearTracker();
            if (items.Count == 0)
                return;
            SetAxes();
            ComputeLayoutLineItemCount();
            ComputeLineCrossSize();
            if (alignContent == AlignContent.Stretch)
                StretchLineSize();

            
            PlaceItems();
            shouldRebuild = false;
        }

        private void UpdateItems()
        {
            UIFlexItem[] items;
            bool change;

            if (!itemChange)
                return;
            itemChange = false;

            change = false;
            items = FindItems().ToArray();
            foreach(UIFlexItem item in items)
            {
                if (!this.items.Contains(item))
                {
                    change = true;
                    onItemRelease?.Invoke(item);
                }
            }
            if (!change)
            {
                foreach(UIFlexItem item in this.items)
                {
                    if (!items.Contains(item))
                    {
                        change = true;
                        onItemAcquire?.Invoke(item);
                    }
                }
            }
            if (!change)
                return;
            this.items.Clear();
            this.items.AddRange(items);
            onItemListChange?.Invoke();
        }

        private IEnumerable<UIFlexItem> FindItems()
        {
            Transform child;
            UIFlexItem flexItemComponent;
            int index;

            index = 0;
            while (index < transform.childCount)
            {
                child = transform.GetChild(index);
                flexItemComponent = child.GetComponent<UIFlexItem>();
                if (child.gameObject.activeSelf && flexItemComponent != null && flexItemComponent.enabled)
                    yield return (flexItemComponent);
                ++index;
            }
        }
        private void SortItems()
        {
            if (!shouldSort)
                return;
            items.Sort();
        }

        private void NotifyRebuilding()
        {
            onRebuilding?.Invoke();
            foreach (UIFlexItem item in this)
                item.NotifyRebuilding();
        }

        private void SetAxes()
        {
            int dir;
            int rev;
            int revWrap;

            dir = (int)direction;
            rev = (!reversed ? 1 : -1);
            revWrap = ((wrap && wrapReversed) ? -1 : 1);

            mainAxis.x = (1 - dir) * rev;
            mainAxis.y = -dir * rev;
            crossAxis.x = dir * revWrap;
            crossAxis.y = -(1 - dir) * revWrap;

            mainAxisSides[0] = 2 - dir + mainAxis[0] + mainAxis[1];
            mainAxisSides[1] = 4 - 2 * dir - mainAxisSides[0];
            crossAxisSides[0] = 1 + dir + crossAxis[0] + crossAxis[1];
            crossAxisSides[1] = 2 + 2 * dir - crossAxisSides[0];
        }

        private void ComputeLayoutLineItemCount()
        {
            List<int> itemCount;
            int axis;
            float axisSize;
            float size;
            float curSize;
            float gap;
            int count;

            if (!wrap || items.Count <= 1)
            {
                lineItemCount = new int[1] { items.Count };
                return;
            }
            axis = mainAxisIndex;
            axisSize = mainAxisContentSize;
            gap = minimumGap[axis].GetSize(axisSize);
            itemCount = new List<int>(System.Math.Max(10, items.Count / 2));
            curSize = 0f;
            count = 0;
            foreach (UIFlexItem item in items)
            {
                ++count;
                size = item.BaseSizeMargin(axis, axisSize);
                curSize += size;
                if (count > 1)
                    curSize += gap;
                if (curSize > axisSize)
                {
                    if (count > 1)
                    {
                        itemCount.Add(count - 1);
                        curSize = size;
                        count = 1;
                    }
                    else
                    {
                        itemCount.Add(1);
                        curSize = 0f;
                        count = 0;
                    }
                }
            }
            if (count != 0)
                itemCount.Add(count);
            lineItemCount = itemCount.ToArray();
        }

        private void ComputeLineCrossSize()
        {
            int lineIndex;
            int itemIndex;
            int count;
            float size;
            float axisSize;
            float totalCrossSize;

            lineCrossSize = new float[lineItemCount.Length];
            totalCrossSize = 0f;
            axisSize = crossAxisContentSize;
            itemIndex = 0;
            lineIndex = 0;
            while (lineIndex < lineCrossSize.Length)
            {
                count = lineItemCount[lineIndex];
                size = 0f;
                while (count > 0)
                {
                    size = Mathf.Max(size, items[itemIndex].BaseSizeMargin(crossAxisIndex, axisSize));
                    ++itemIndex;
                    --count;
                }
                lineCrossSize[lineIndex] = size;
                totalCrossSize += size;
                ++lineIndex;
            }
            crossSpacing = crossAxisContentSize - totalCrossSize;
        }

        private void StretchLineSize()
        {
            float stretch;
            float gap;

            gap = minimumGap[crossAxisIndex].GetSize(crossAxisContentSize);
            gap *= (lineItemCount.Length - 1);
            if (crossSpacing <= gap)
                return;
            crossSpacing -= gap;
            stretch = crossSpacing / lineItemCount.Length;
            for (int i = 0; i < lineCrossSize.Length; ++i)
                lineCrossSize[i] += stretch;
            crossSpacing = gap;
        }

        private Vector3 GetSpacingWeights()
        {
            FlexSpaceDistributor distributor;

            if (justifyContent == JustifyContent.Custom)
                return new Vector3(
                    Mathf.Abs(spacingDistribution.x),
                    Mathf.Abs(spacingDistribution.y),
                    Mathf.Abs(spacingDistribution.z));
            distributor = new FlexSpaceDistributor();
            distributor.SetDistributionType(justifyContent);
            return (distributor.GetInitialDistribution());
        }
        private void PlaceItems()
        {
            FlexSpaceDistributor distributor;
            Vector3 computedWeights;
            float crossGap;
            float crossPosition;
            float spacing;
            int wrapRev;
            int itemIndex;
            int line;

            crossGap = minimumGap[crossAxisIndex].GetSize(crossAxisContentSize);

            spacing = crossSpacing;
            distributor = new FlexSpaceDistributor();
            distributor.SetDistributionType(alignContent, wrap && wrapReversed);
            distributor.Compute(crossGap, lineItemCount.Length - 1, spacing);
            computedWeights = distributor.GetComputedDistribution();
            spacing += distributor.GetOverflow();
            crossGap = computedWeights.y * spacing;

            wrapRev = (wrap && wrapReversed ? 1 : 0);

            crossPosition = padding.SizeSide(crossAxisSides[0], crossAxisSize);
            crossPosition += computedWeights[wrapRev * 2] * spacing;
            crossPosition -= distributor.GetOverflow() * distributor.GetComputedSideDistribution()[wrapRev];
            itemIndex = 0;
            line = 0;
            while (line < lineItemCount.Length)
            {
                PlaceItemsInLine(line, itemIndex, crossPosition);
                crossPosition += lineCrossSize[line];
                crossPosition += crossGap;
                itemIndex += lineItemCount[line];
                ++line;
            }
        }

        
        private float LineSize(int line, int itemIndex, int mask)
        {
            float contentSize;
            float size;
            int count;

            if ((mask & 0b11) == 0)
                return (0f);
            contentSize = mainAxisContentSize;
            size = 0f;
            count = lineItemCount[line];
            while (count > 0)
            {
                if ((mask & 0b01) != 0)
                    size += items[itemIndex].BaseSize(mainAxisIndex, contentSize);
                if ((mask & 0b10) != 0)
                    size += items[itemIndex].MarginSize(mainAxisIndex, contentSize);
                --count;
                ++itemIndex;
            }
            return (size);
        }

        private float LineSizeBasis(int line, int itemIndex)
        {
            return LineSize(line, itemIndex, 0b01);
        }

        private float LineSizeMargins(int line, int itemIndex)
        {
            return (LineSize(line, itemIndex, 0b10));
        }

        private void PlaceItemsInLine(int line, int itemIndex, float crossPos)
        {
            FlexSpaceDistributor spaceDistributor;
            FlexEvaluator evaluator;
            UIFlexItem item;
            Vector3 spacingWeights;
            float mainContentSize;
            float gap;
            float marginSize;
            float flex;
            float cursor;
            float spacing;
            float itemSize;
            int count;
            int index;

            mainContentSize = mainAxisContentSize;
            gap             = minimumGap[mainAxisIndex].GetSize(mainContentSize);
            marginSize      = LineSizeMargins(line, itemIndex);
            flex            = mainContentSize - LineSizeBasis(line, itemIndex) - marginSize;
            flex            -= gap * (lineItemCount[line] - 1);

            count = lineItemCount[line];
            evaluator = new FlexEvaluator(count);
            index = 0;
            while (index < count)
            {
                evaluator.AddItem(items[itemIndex]);
                ++index;
                ++itemIndex;
            }
            itemIndex -= count;
            evaluator.EvaluateSizes(mainAxisIndex, mainContentSize, flex);

            spacing = mainContentSize - evaluator.GetTotalSize() - marginSize;
            spaceDistributor = new FlexSpaceDistributor();
            spaceDistributor.SetDistribution(spacingDistribution);
            spaceDistributor.Compute(gap, count - 1, spacing);
            spacingWeights = spaceDistributor.GetComputedDistribution();

            cursor = padding.SizeSide(mainAxisSides[0], mainAxisSize);
            index = 0;
            cursor += spacing * spacingWeights[0];
            while (index < count)
            {
                item = items[itemIndex];
                itemSize = evaluator.GetSize(index);
                itemSize += item.MarginSize(mainAxisIndex, mainContentSize);
                PlaceItem(item, cursor, crossPos, itemSize, lineCrossSize[line]);
                cursor += itemSize;
                cursor += Mathf.Max(gap, spacing * spacingWeights[1]);
                ++itemIndex;
                ++index;
            }

        }

        private void PlaceItem(UIFlexItem item, float main, float cross, float mainSize, float crossSize)
        {
            Vector2 pos;
            Vector2 size;
            float mainContentSize;
            float crossContentSize;
            float itemCrossBase;
            float itemCrossDeltaMax;
            float spacing;
            float preSpacingRatio;
            UIFlexItem.Alignment alignment;

            mainContentSize = mainAxisContentSize;
            crossContentSize = crossAxisContentSize;
            itemCrossBase = item.BaseSizeMargin(crossAxisIndex, crossContentSize);
            spacing = crossSize - itemCrossBase;
            preSpacingRatio = SelfAlignPrefixSpacing(item);
            alignment = (item.alignSelfEnabled ? item.alignSelf : alignItems);
            if (alignment == UIFlexItem.Alignment.Stretch)
            {
                itemCrossDeltaMax = Mathf.Max(0f, item.MaximumSizeDelta(crossAxisIndex, crossContentSize));
                spacing = Mathf.Max(0f, spacing - itemCrossDeltaMax);
                preSpacingRatio = 0f;
            }
            cross += spacing * preSpacingRatio;
            crossSize -= spacing;

            pos = Origin();
            pos += (Vector2)mainAxis * main + (Vector2)crossAxis * cross;
            pos += (Vector2)mainAxis * item.margin.SizeSide(mainAxisSides[0], mainContentSize);
            pos += (Vector2)crossAxis * item.margin.SizeSide(crossAxisSides[0], crossContentSize);

            size = (Vector2)mainAxis * mainSize + (Vector2)crossAxis * crossSize;
            size -= (Vector2)mainAxis * item.margin.SizeAxis(mainAxisIndex, mainContentSize);
            size -= (Vector2)crossAxis * item.margin.SizeAxis(crossAxisIndex, crossContentSize);
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Abs(size.y);

            pos += ItemPivotDisplacement(item, size);

            item.rectTransform.anchorMin = Vector2.up;
            item.rectTransform.anchorMax = Vector2.up;
            item.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            item.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            item.rectTransform.localPosition = pos;

            ApplyTracker(item.rectTransform);
        }

        
        private Vector2 Pivot()
        {
            Vector2 pivot;

            pivot = Vector2Int.one - mainAxis - crossAxis;
            return (pivot / 2f);
        }

        private Vector2 Origin()
        {
            Vector2 origin;

            origin = transform.rect.position + Pivot() * transform.rect.size;
            return (origin);
        }

        private Vector2 ItemPivotDisplacement(UIFlexItem item, Vector2 size)
        {
            Vector2 toItemPivot;

            toItemPivot = (item.rectTransform.pivot - Pivot()) * size;
            return (toItemPivot);
        }

        private float SelfAlignPrefixSpacing(UIFlexItem item)
        {
            UIFlexItem.Alignment alignment;
            float ratio;

            alignment = (item.alignSelfEnabled? item.alignSelf : alignItems);
            switch (alignment)
            {
                case UIFlexItem.Alignment.Start: ratio = 0f; break;
                case UIFlexItem.Alignment.Center: ratio = 0.5f; break;
                case UIFlexItem.Alignment.End: ratio = 1f; break;
                default: ratio = 0f; break;
            }
            if (wrap && wrapReversed)
                return (1f - ratio);
            return (ratio);
        }

        private void ClearTracker()
        {
            tracker.Clear();
        }

        private void ApplyTracker(RectTransform transform)
        {
            tracker.Add(this, transform,
                DrivenTransformProperties.SizeDelta |
                DrivenTransformProperties.Anchors |
                DrivenTransformProperties.AnchoredPosition);
        }
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            MarkRebuild();
        }
#endif
        protected void OnTransformChildrenChanged()
        {
            NotifyItemChange();
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            MarkRebuild();
        }
#endif
        protected override void OnDidApplyAnimationProperties()
        {
            MarkRebuild();
        }

        protected override void OnEnable()
        {
            MarkRebuild();
        }

        protected override void OnDisable()
        {
            MarkRebuild();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            MarkRebuild();
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!UnityEditor.Selection.gameObjects.Contains(gameObject))
                return;

            UIGizmos.SetColorPadding();
            UIGizmos.DrawRect(ContentRect(), transform);
        }
#endif


    }

}
