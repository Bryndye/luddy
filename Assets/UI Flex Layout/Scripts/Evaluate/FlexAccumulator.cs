using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlexLayout.Evaluate
{

    public class FlexAccumulator
    {
        private readonly FlexItem item;

        private float basis;
        private float min;
        private float minInitial;
        private float max;
        private float maxInitial;
        private float flex = 0f;
        private float accumulatedSize = 0f;
        private float accumulatedUnits = 0f;
        private bool isFixed = false;


        public FlexAccumulator(FlexItem item, int axis, float relativeSize)
        {
            bool canGrow;
            bool canShrink;

            this.item = item;

            basis   = item.BaseSize(axis, relativeSize);
            min     = item.MinimumSize(axis, relativeSize);
            max     = item.MaximumSize(axis, relativeSize);
            canGrow     = item.FlexScale(1)  != 0f;
            canShrink   = item.FlexScale(-1) != 0f;


            max = Mathf.Max(min, max);
            if (!canGrow)
                max = Mathf.Max(min, Mathf.Min(max, basis));
            if (!canShrink)
                min = Mathf.Max(min, Mathf.Min(max, basis));

            minInitial = min;
            maxInitial = max;
        }

        public float RequiredShrink()
        {
            return (Mathf.Max(0f, basis - max));
        }

        public float RequiredGrow()
        {
            return (Mathf.Max(0f, min - basis));
        }

        public float BasisFactor()
        {
            return (Mathf.Max(0f, basis));
        }

        public void InitializeFlex(int flexSign, bool includeBasisFactor)
        {
            if (flexSign == 0)
                return;
            flexSign = (int)Mathf.Sign(flexSign);
            min = (min - basis) * flexSign;
            max = (max - basis) * flexSign;
            if (flexSign == -1)
                SwapMinMax();
            isFixed = (max <= 0f);
            if (isFixed)
            {
                accumulatedSize = max;
                return;
            }
            flex = Mathf.Abs(item.FlexScale(flexSign));
            if (includeBasisFactor)
                flex *= BasisFactor();
            isFixed = (flex == 0f);
            if (isFixed)
                accumulatedSize = Mathf.Max(0f, min);
        }

        private void SwapMinMax()
        {
            float v;

            v = min;
            min = max;
            max = v;
        }
        
        public bool IsFixed()
        {
            return (isFixed);
        }

        public float FlexFactor()
        {
            if (isFixed)
                return (0f);
            return (Mathf.Max(0f, flex));
        }

        public void NormalizeFlex(float total)
        {
            if (total <= 0f)
                return;
            flex /= total;
        }

        public float UnitsToMax()
        {
            return (max / flex);
        }

        public float UnitsToMin()
        {
            return (min / flex);
        }

        public void AddUnits(float units)
        {
            accumulatedUnits += units;
        }

        public float RequiredSize()
        {
            return (Mathf.Max(0f, UnitsToMin() - accumulatedUnits) * flex);
        }

        public void FinalizeSize(int flexSign)
        {
            if (!isFixed)
                accumulatedSize = accumulatedUnits * flex;
            if (flexSign < 0)
                accumulatedSize = -accumulatedSize;
        }

        public float GetSize()
        {
            return (accumulatedSize + basis);
        }

        public void SetFixedSize(float size)
        {
            accumulatedSize = size - basis;
        }

        public void SetFixedSizeMin()
        {
            SetFixedSize(minInitial);
        }

        public void SetFixedSizeMax()
        {
            SetFixedSize(maxInitial);
        }
    }
}