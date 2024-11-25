using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlexLayout.Evaluate
{
    public class FlexSegmenter
    {
        private class Frame
        {
            public List<FlexAccumulator> accumulators = null;
            public float startAt;

            public void Add(FlexAccumulator acc)
            {
                if (accumulators == null)
                    accumulators = new List<FlexAccumulator>();
                accumulators.Add(acc);
            }

            public void AddAll(Frame frame)
            {
                if (frame.accumulators == null)
                    return;
                if (accumulators == null)
                    accumulators = new List<FlexAccumulator>(Mathf.Min(10, frame.accumulators.Count));
                accumulators.AddRange(frame.accumulators);
            }

            public bool HasEntries()
            {
                return (accumulators != null && accumulators.Count != 0);
            }

            public int EntryCount()
            {
                if (accumulators == null)
                    return (0);
                return (accumulators.Count);
            }
        }

        private List<Frame> frames = new List<Frame>();

        public FlexSegmenter()
        {
            Frame baseFrame;

            baseFrame = new Frame();
            baseFrame.startAt = float.NegativeInfinity;
            frames.Add(baseFrame);
        }

        public void Add(FlexAccumulator accumulator)
        {
            float min;
            float max;

            min = Mathf.Max(0f, accumulator.UnitsToMin());
            max = accumulator.UnitsToMax();
            accumulator.AddUnits(min);

            if (min >= max)
                return;
            FindIndices(min, max, out int iStart, out int iEnd);
            if (min > frames[iStart].startAt)
            {
                SplitFrame(iStart, min);
                ++iStart;
                ++iEnd;
            }
            if (iEnd == (frames.Count - 1) || max < frames[frames.Count - 1].startAt)
                SplitFrame(iEnd, max);
            for (int i = iStart; i <= iEnd; ++i)
                frames[i].Add(accumulator);
        }

        private void FindIndices(float min, float max, out int iStart, out int iEnd)
        {
            iStart = 0;
            while (iStart < (frames.Count - 1))
            {
                if (min < frames[iStart + 1].startAt)
                    break;
                ++iStart;
            }
            iEnd = iStart;
            while (iEnd < (frames.Count - 1))
            {
                if (max <= frames[iEnd + 1].startAt)
                    break;
                ++iEnd;
            }
        }

        private void SplitFrame(int index, float at)
        {
            Frame copy;

            copy = new Frame();
            copy.AddAll(frames[index]);
            copy.startAt = at;
            frames.Insert(index + 1, copy);
        }

        public int FrameCount()
        {
            return (frames.Count);
        }

        public bool FrameHasEntries(int frameIndex)
        {
            return (frames[frameIndex].HasEntries());
        }

        public List<FlexAccumulator> GetFrameEntries(int frameIndex)
        {
            return (frames[frameIndex].accumulators);
        }

        public int FrameEntryCount(int frameIndex) 
        {
            return (frames[frameIndex].EntryCount());
        }

        public float FrameSize(int frameIndex)
        {
            if (frameIndex == frames.Count - 1)
                return (float.PositiveInfinity);
            return (frames[frameIndex + 1].startAt - frames[frameIndex].startAt);
        }
    }
}
