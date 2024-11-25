using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FlexLayout
{

    [Serializable]
    public struct UISize
    {
        public float percentage;
        public float pixels;

        public float fraction { get { return percentage / 100f; } set { percentage = value * 100f; } }

        public UISize(float percentage, float pixels)
        {
            this.percentage = percentage;
            this.pixels = pixels;
        }

        public static UISize operator +(UISize a, UISize b)
        {
            a.percentage += b.percentage;
            a.pixels += b.pixels;
            return (a);
        }

        public static UISize operator -(UISize a, UISize b)
        {
            a.percentage -= b.percentage;
            a.pixels -= b.pixels;
            return (a);
        }

        public static UISize operator *(UISize a, float mul)
        {
            a.percentage *= mul;
            a.pixels *= mul;
            return (a);
        }

        public static UISize operator /(UISize a, float div)
        {
            a.percentage /= div;
            a.pixels /= div;
            return (a);
        }

        public float GetSize(float relative)
        {
            return (Mathf.Max(0f, pixels + fraction * relative));
        }

        public UISize Combine(UISize other)
        {
            return (this + other);
        }

        public static readonly UISize pixels100 = new UISize() { pixels = 100 };
        public static readonly UISize percentFull = new UISize() { percentage = 100 };
    }

    [Serializable]
    public struct UISize2D
    {
        public UISize width;
        public UISize height;

        public UISize this[int i]
        {
            get { return (i == 0 ? width : height); }
            set { if (i == 0) width = value; else height = value; }
        }
    }

    [Serializable]
    public struct Bool2D
    {
        public bool width;
        public bool height;

        public bool this[int i]
        {
            get { return (i == 0 ? width : height); }
            set { if (i == 0) width = value; else height = value; }
        }
    }

    [Serializable]
    public struct UIBorder
    {
        public UISize uniform;
        public UISize top;
        public UISize right;
        public UISize bottom;
        public UISize left;

        private float GetSize(UISize side, float relative)
        {
            return ((side + uniform).GetSize(relative));
        }

        public UISize this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return (top);
                    case 1: return (right);
                    case 2: return (bottom);
                    case 3: return (left);
                    default: throw new IndexOutOfRangeException(index.ToString());
                }
            }
            set
            {
                switch (index)
                {
                    case 0: top     = value; break;
                    case 1: right   = value; break;
                    case 2: bottom  = value; break;
                    case 3: left    = value; break;
                    default: throw new IndexOutOfRangeException(index.ToString());
                }
            }
        }

        private int AxisIndex1(int axis) { return (3 - axis * 3); }
        private int AxisIndex2(int axis) { return (1 + axis); }

        public float SizeAxis(int axis, float relative)
        {
            float size;

            size = SizeSide(AxisIndex1(axis), relative);
            size += SizeSide(AxisIndex2(axis), relative);
            return (size);
        }

        public UISize Axis(int axis)
        {
            if (axis == 0)
                return (left + right + uniform * 2f);
            return (bottom + top + uniform * 2f);
        }

        public float SizeSide(int sideIndex, float relative) { return GetSize(this[sideIndex], relative); }
        public float SizeTop(float relative) { return GetSize(top, relative); }
        public float SizeRight(float relative) { return GetSize(right, relative); }
        public float SizeBottom(float relative) { return GetSize(bottom, relative); }
        public float SizeLeft(float relative) { return GetSize(left, relative); }

    }
}