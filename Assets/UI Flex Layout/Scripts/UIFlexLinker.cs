using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FlexLayout
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIFlexItem), typeof(UIFlexContainer))]
    [AddComponentMenu("UI/Flex Layout/UI Flex Linker")]
    [ExecuteInEditMode]
    public class UIFlexLinker : UIBehaviour
    {
        [Header("Computes Flex-Item sizes based of items within the Flex-Container.")]
        [Space(10f, order = 2)]
        [SerializeField] private bool flexBasis = false;
        [SerializeField] private bool minimumSize = false;
        [SerializeField] private bool maximumSize = false;
        
        private UIFlexItem item;
        private UIFlexContainer container;

        private bool initialized = false;
        private bool basisValid = true;
        private float evaluateTime = 0f;

        public int drivenAxis { get { return container.mainAxisIndex; } }

        public bool drivingBasis { get { return flexBasis && enabled; } set { flexBasis = value; } }
        public bool drivingMinimum { get { return minimumSize && enabled; } set { minimumSize = value; } }
        public bool drivingMaximum { get { return maximumSize && enabled; } set { maximumSize = value; } }

        public bool drivingBasisValid { get { return (drivingBasis && basisValid); } }
        public bool drivingMinimumValid { get { return (drivingMinimum && item.flexMinimumEnabled[drivenAxis]); } }
        public bool drivingMaximumValid { get { return (drivingMaximum && item.flexMaximumEnabled[drivenAxis]); } }


        private void Initialize()
        {
            item = GetComponent<UIFlexItem>();
            container = GetComponent<UIFlexContainer>();
            initialized = true;
        }

        public bool DrivingAny()
        {
            return (drivingBasis || drivingMaximum || drivingMinimum);
        }

        protected override void OnEnable()
        {
            Initialize();
            item.onContainerRebuild += Evaluate;
            container.onRebuilding  += Evaluate;
            container.onMarkRebuild += MarkParentRebuild;
        }

        protected override void OnDisable()
        {
            item.onContainerRebuild -= Evaluate;
            container.onRebuilding  -= Evaluate;
            container.onMarkRebuild -= MarkParentRebuild;
        }

        private void MarkParentRebuild()
        {
            if (item.HasContainer())
                item.GetContainer().MarkRebuild();
        }

        private void Evaluate()
        {
            if (!DrivingAny())
                return;
            if (!initialized)
                Initialize();
            if (evaluateTime == Time.time)
                return;
            evaluateTime = Time.time;
            EvaluateChildLinkers();
            if (flexBasis)
                EvaluateBasisSize();
            if (minimumSize)
                EvaluateMinimumSize();
            if (maximumSize)
                EvaluateMaximumSize();
        }

        private void EvaluateChildLinkers()
        {
            foreach(UIFlexItem item in container)
            {
                if (item.TryGetComponent(out UIFlexLinker childLinker))
                    childLinker.Evaluate();
            }
        }

        private void EvaluateBasisSize()
        {
            UISize totalSizes;
            int axis;
            bool valid;

            axis = container.mainAxisIndex;
            totalSizes = GetBasisSize(axis) + GetGapSize(axis);
            valid = ComputeTransformSize(axis, totalSizes, out UISize computed);
            basisValid = valid;
            if (valid)
                item.flexBasis[axis] = computed;
        }

        private void EvaluateMinimumSize()
        {
            UISize totalSizes;
            int axis;
            bool valid;

            axis = container.mainAxisIndex;
            totalSizes = GetMinimumSize(axis) + GetGapSize(axis);
            valid = ComputeTransformSize(axis, totalSizes, out UISize computed);
            item.flexMinimumEnabled[axis] = valid;
            if (valid)
                item.flexMinimum[axis] = computed;
        }

        private void EvaluateMaximumSize()
        {
            UISize totalSizes;
            int axis;
            bool valid;

            axis = container.mainAxisIndex;
            totalSizes = GetMaximumSize(axis) + GetGapSize(axis);
            if (totalSizes.pixels == float.PositiveInfinity)
            {
                item.flexMaximumEnabled[axis] = false;
                return;
            }
            valid = ComputeTransformSize(axis, totalSizes, out UISize computed);
            item.flexMaximumEnabled[axis] = valid;
            if (valid)
                item.flexMaximum[axis] = computed;
        }

        private bool ComputeTransformSize(int axis, UISize totalSizeContent, out UISize computed)
        {
            UISize padding;
            float contentSize;

            computed = default;
            if (!IsComputableSize(totalSizeContent))
                return (false);
            contentSize = ComputeSize(totalSizeContent);
            padding = container.padding.Axis(axis);
            if (!CanInversePadding(padding))
                return (false);
            computed.pixels = InversePadding(padding, contentSize);
            return (true);
        }

        private UISize GetBasisSize(int axis)
        {
            UISize size;

            size = default;
            foreach (UIFlexItem item in container)
            {
                size += item.margin.Axis(axis);
                size += item.flexBasis[axis];
            }
            return (size);
        }

        private UISize GetMinimumSize(int axis)
        {
            UISize size;

            size = default;
            foreach(UIFlexItem item in container)
            {
                size += item.margin.Axis(axis);
                if (item.flexMinimumEnabled[axis])
                    size += item.flexMinimum[axis];
                else if (!item.ShrinkEnabled())
                    size += item.flexBasis[axis];
            }
            return (size);
        }

        private UISize GetMaximumSize(int axis)
        {
            UISize size;

            size = default;
            foreach (UIFlexItem item in container)
            {
                size += item.margin.Axis(axis);
                if (item.flexMaximumEnabled[axis])
                    size += item.flexMaximum[axis];
                else if (!item.GrowEnabled())
                    size += item.flexBasis[axis];
                else
                    size.pixels = float.PositiveInfinity;
            }
            return (size);
        }

        private UISize GetGapSize(int axis)
        {
            int gapCount;

            gapCount = Mathf.Max(0, container.ItemCount - 1);
            if (gapCount == 0)
                return default;
            return (container.minimumGap[axis] * gapCount);
        }

        private static bool IsComputableSize(UISize size)
        {
            if (size.pixels < 0f)
                return (false);
            if (size.pixels == 0f)
                return (true);
            if (size.percentage >= 100f)
                return (false);
            return (true);
        }

        private static float ComputeSize(UISize size)
        {
            float pixelRatio;

            if (size.pixels == 0f)
                return (0f);
            pixelRatio = 1f - (size.percentage / 100f);
            return (size.pixels / pixelRatio);
        }

        private static bool CanInversePadding(UISize padding)
        {
            if ((padding.percentage / 100f) >= 1f)
                return (false);
            return (true);
        }

        private static float InversePadding(UISize padding, float contentSize)
        {
            return (contentSize + padding.pixels) / (1f - (padding.percentage / 100f));
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            Evaluate();
            item.Revalidate();
        }
#endif
    }
}