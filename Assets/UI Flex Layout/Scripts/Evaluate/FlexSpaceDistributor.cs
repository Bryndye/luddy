using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlexLayout.Evaluate
{
    public class FlexSpaceDistributor
    {
        private Vector3 spacingDistribution = new Vector3(0, 0, 1);
        private Vector3 computedDistribution = new Vector3(0, 0, 1);
        private Vector2 sideDistribution = new Vector2(0, 1);
        private float overflow = 0f;

        public void SetDistribution(Vector3 distribution)
        {
            spacingDistribution = new Vector3(
                    Mathf.Abs(distribution.x),
                    Mathf.Abs(distribution.y),
                    Mathf.Abs(distribution.z));
        }

        public Vector3 GetInitialDistribution()
        {
            return (spacingDistribution);
        }

        public void SetDistributionType(UIFlexContainer.JustifyContent justifyContent)
        {
            if (justifyContent == UIFlexContainer.JustifyContent.Custom)
                return;
            SetDistributionType((UIFlexContainer.AlignContent)justifyContent);
        }

        public void SetDistributionType(UIFlexContainer.AlignContent alignContent, bool wrapReverse = false)
        {
            float rev;

            rev = (wrapReverse ? 1f : 0f);
            switch (alignContent)
            {
                case UIFlexContainer.AlignContent.Start: spacingDistribution = new Vector3(0, 0, 1); break;
                case UIFlexContainer.AlignContent.Center: spacingDistribution = new Vector3(1, 0, 1); break;
                case UIFlexContainer.AlignContent.End: spacingDistribution = new Vector3(1, 0, 0); break;
                case UIFlexContainer.AlignContent.SpaceBetween: spacingDistribution = new Vector3(0, 1, 0); break;
                case UIFlexContainer.AlignContent.SpaceAround: spacingDistribution = new Vector3(0.5f, 1, 0.5f); break;
                case UIFlexContainer.AlignContent.SpaceEvenly: spacingDistribution = new Vector3(1, 1, 1); break;
                case UIFlexContainer.AlignContent.Stretch: spacingDistribution = new Vector3(rev, 0, (1f - rev)); break;
            }
        }

        public void Compute(float minimumgap, int gapCount, float spacing)
        {
            computedDistribution = ComputeWeights(minimumgap, gapCount, spacing);
        }

        private Vector3 ComputeWeights(float minimumgap, int gapCount, float spacing)
        {
            Vector3 weights;
            float total;
            float sidesTotal;
            float gap;
            float gapAdd;
            float gapAddRatio;

            sideDistribution = new Vector2(spacingDistribution.x, spacingDistribution.z);
            if (sideDistribution == Vector2.zero)
                sideDistribution = new Vector2(0.5f, 0.5f);
            else
                sideDistribution /= (Mathf.Abs(sideDistribution.x) + Mathf.Abs(sideDistribution.y));

            overflow = 0f;
            weights = spacingDistribution;
            gapCount = Mathf.Max(0, gapCount);
            weights.y *= gapCount;
            total = weights.x + weights.y + weights.z;
            if (total == 0f)
                return (new Vector3(0.5f, 0, 0.5f));
            weights /= total;
            if (gapCount != 0)
                weights.y /= gapCount;

            overflow = Mathf.Max(0f, minimumgap * gapCount - spacing);
            spacing += overflow;
            gap = weights.y * spacing;
            if (gap >= minimumgap)
                return (weights);
            gapAdd = minimumgap - gap;
            gapAddRatio = (spacing != 0f ? (gapAdd / spacing) : (1f - weights.y));
            weights.y += gapAddRatio;

            sidesTotal = weights.x + weights.z;
            if (sidesTotal == 0f)
                return (weights);
            weights.x -= (gapAddRatio * gapCount) * (weights.x / sidesTotal);
            weights.z -= (gapAddRatio * gapCount) * (weights.z / sidesTotal);
            return (weights);
        }

        public Vector3 GetComputedDistribution()
        {
            return (computedDistribution);
        }

        public Vector2 GetComputedSideDistribution()
        {
            return (sideDistribution);
        }

        public float GetOverflow()
        {
            return overflow;
        }
    }
}
