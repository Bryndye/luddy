using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlexLayout.Evaluate
{
    public class FlexEvaluator
    {
        private FlexItem[] items;
        private int count;

        private FlexAccumulator[] accumulators;
        private FlexAccumulator[] activeAccumulators;
        private int flexSign;

        public FlexEvaluator(int capacity)
        {
            items = new FlexItem[capacity];
            count = 0;
        }

        public void AddItem(FlexItem item)
        {
            items[count] = item;
            ++count;
        }

        public void EvaluateSizes(int axis, float relativeSize, float spacing)
        {
            if (count == 0)
                return;
            CreateAccumulators(axis, relativeSize);
            DefineFlexDirection(ref spacing);
            spacing = Mathf.Abs(spacing);
            SetupAccumulatorsFlex();
            GatherActiveAccumulators();
            if (!NormalizeFlexFactors())
            {
                SetFixedSizes();
                return;
            }
            Distribute(spacing);
            FinalizeSizes();
        }

        private void CreateAccumulators(int axis, float relativeSize)
        {
            int index;

            accumulators = new FlexAccumulator[count];
            index = 0;
            while (index < count)
            {
                accumulators[index] = new FlexAccumulator(items[index], axis, relativeSize);
                ++index;
            }
        }

        private void DefineFlexDirection(ref float spacing)
        {
            float requiredShrink;
            float requiredGrow;

            requiredShrink = 0f;
            requiredGrow = 0f;
            foreach( FlexAccumulator acc in accumulators)
            {
                requiredShrink += acc.RequiredShrink();
                requiredGrow += acc.RequiredGrow();
            }
            flexSign = (int)Mathf.Sign(spacing + requiredShrink - requiredGrow);
            if (flexSign == 0)
            {
                spacing = 0f;
                return;
            }
            spacing = spacing + requiredShrink - requiredGrow;
        }

        private bool Shrinking()
        {
            return (flexSign == -1);
        }

        private bool Growing()
        {
            return (flexSign == 1);
        }

        private void SetupAccumulatorsFlex()
        {
            bool useBasisFactor;

            useBasisFactor = Shrinking();
            foreach (FlexAccumulator a in accumulators)
                a.InitializeFlex(flexSign, useBasisFactor);
        }

        private void GatherActiveAccumulators()
        {
            List<FlexAccumulator> actives;

            actives = new List<FlexAccumulator>(count);
            foreach (FlexAccumulator a in accumulators)
                if (!a.IsFixed())
                    actives.Add(a);
            activeAccumulators = actives.ToArray();
        }

        private bool NormalizeFlexFactors()
        {
            float totalFlex;

            totalFlex = 0f;
            foreach (FlexAccumulator a in activeAccumulators)
                totalFlex += a.FlexFactor();

            if (totalFlex == 0f)
                return (false);

            foreach (FlexAccumulator a in activeAccumulators)
                a.NormalizeFlex(totalFlex);
            return (true);
        }


        private void Distribute(float spacing)
        {
            FlexSegmenter segmenter;
            int frameCount;
            float factor;
            int count;
            float divided;

            segmenter = new FlexSegmenter();
            foreach (FlexAccumulator acc in activeAccumulators)
                segmenter.Add(acc);
            frameCount = segmenter.FrameCount();
            for(int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
            {
                if (spacing <= 0f)
                    return;
                count = segmenter.FrameEntryCount(frameIndex);
                if (count == 0)
                    continue;
                factor = 0f;
                foreach (FlexAccumulator acc in segmenter.GetFrameEntries(frameIndex))
                    factor += acc.FlexFactor();
                spacing *= count;
                spacing /= factor;
                divided = Mathf.Min(segmenter.FrameSize(frameIndex), spacing / count);
                spacing -= divided * count;
                spacing *= factor;
                spacing /= count;
                foreach (FlexAccumulator acc in segmenter.GetFrameEntries(frameIndex))
                    acc.AddUnits(divided);
            }   
        }

        private void FinalizeSizes()
        {
            foreach (FlexAccumulator a in accumulators)
                a.FinalizeSize(flexSign);
        }

        private void SetFixedSizes()
        {
            if (flexSign == 0)
                return;

            foreach(FlexAccumulator a in accumulators)
            {
                if (flexSign < 0)
                    a.SetFixedSizeMin();
                else
                    a.SetFixedSizeMax();
            }
        }

        public int Count()
        {
            return count;
        }

        public float GetSize(int index)
        {
            float size;

            size = accumulators[index].GetSize();
            return (size);
        }

        public float GetTotalSize()
        {
            float size;
            int index;

            size = 0f;
            index = 0;
            while (index < count)
            {
                size += accumulators[index].GetSize();
                ++index;
            }
            return (size);

        }

    }
}
