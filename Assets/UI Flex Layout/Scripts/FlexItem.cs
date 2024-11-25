using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlexLayout
{
    public interface FlexItem
    {
        public float BaseSize(int axis, float relativeSize);
        public float MinimumSize(int axis, float relativeSize);
        public float MaximumSize(int axis, float relativeSize);
        public float FlexScale(int flexSign);
    }
}
