using System.Collections.Generic;
using UnityEngine;
namespace BDSZ_2020
{
   
    public enum EUILineSplineType
    {
        Line,
        Centripetal,
        Chordal,
        Catmullrom
    }
    /// <summary>
    /// ui ÏßÌõÇúÏß¡£
    /// </summary>
    public class BDSZ_UISplineCurve : BDSZ_UICurveBase
    {
     
        private bool m_bClosed;
        private EUILineSplineType m_CurveType;
        private float m_fTension;
        private CubicPoly1D m_pX = new CubicPoly1D();
        private CubicPoly1D m_pY = new CubicPoly1D();
        public BDSZ_UISplineCurve(List<Vector2> points, bool closed = false, EUILineSplineType curveType = EUILineSplineType.Centripetal, float tension = 0.5f)
        {
            this.m_vKeyPoints = points;
            this.m_bClosed = closed;
            this.m_CurveType = curveType;
            this.m_fTension = tension;
            UpdateCurveData();
        }

        protected override Vector2 GetPoint(float t)
        {
            var l = m_vKeyPoints.Count;

            var p = (l - (this.m_bClosed ? 0 : 1)) * t;
            var intPoint = Mathf.FloorToInt(p);
            var weight = p - intPoint;

            if (this.m_bClosed)
            {
                intPoint += intPoint > 0 ? 0 : (Mathf.FloorToInt(Mathf.Abs(intPoint) / l) + 1) * l;
            }
            else if (weight == 0 && intPoint == l - 1)
            {
                intPoint = l - 2;
                weight = 1;
            }

            Vector2 p0, p3; // 4 points (p1 & p2 defined below)

            if (this.m_bClosed || intPoint > 0)
            {
                p0 = m_vKeyPoints[(intPoint - 1) % l];
            }
            else
            {
                // extrapolate first point
                p0 = m_vKeyPoints[0] - m_vKeyPoints[1] + m_vKeyPoints[0];
            }

            var p1 = m_vKeyPoints[intPoint % l];
            var p2 = m_vKeyPoints[(intPoint + 1) % l];

            if (this.m_bClosed || intPoint + 2 < l)
            {
                p3 = m_vKeyPoints[(intPoint + 2) % l];
            }
            else
            {
                // extrapolate last point
                p3 = m_vKeyPoints[l - 1] - m_vKeyPoints[l - 2] + m_vKeyPoints[l - 1];
            }

            
            if (this.m_CurveType == EUILineSplineType.Centripetal || this.m_CurveType == EUILineSplineType.Chordal)
            {

                // init Centripetal / Chordal Catmull-Rom
                var pow = this.m_CurveType == EUILineSplineType.Chordal ? 0.5f : 0.25f;
                var dt0 = Mathf.Pow((p0 - p1).sqrMagnitude, pow);
                var dt1 = Mathf.Pow((p1 - p2).sqrMagnitude, pow);
                var dt2 = Mathf.Pow((p2 - p3).sqrMagnitude, pow);

                // safety check for repeated points
                var delta = 0.0001f;
                if (dt1 < delta) dt1 = 1.0f;
                if (dt0 < delta) dt0 = dt1;
                if (dt2 < delta) dt2 = dt1;

                m_pX.InitNonuniformCatmullRom(p0.x, p1.x, p2.x, p3.x, dt0, dt1, dt2);
                m_pY.InitNonuniformCatmullRom(p0.y, p1.y, p2.y, p3.y, dt0, dt1, dt2);
              

            }
            else if (this.m_CurveType == EUILineSplineType.Catmullrom)
            {
                m_pX.InitCatmullRom(p0.x, p1.x, p2.x, p3.x, this.m_fTension);
                m_pY.InitCatmullRom(p0.y, p1.y, p2.y, p3.y, this.m_fTension);
            }

            var point = new Vector2(
                m_pX.Calc(weight),
                m_pY.Calc(weight) 
                
            );

            return point;

        }
    }

    public class CubicPoly1D
    {
        float c0;
        float c1;
        float c2;
        float c3;

      
        public void Init(float x0, float x1, float t0, float t1)
        {

            c0 = x0;
            c1 = t0;
            c2 = -3 * x0 + 3 * x1 - 2 * t0 - t1;
            c3 = 2 * x0 - 2 * x1 + t0 + t1;

        }

        public void InitCatmullRom(float x0, float x1, float x2, float x3, float tension)
        {

            Init(x1, x2, tension * (x2 - x0), tension * (x3 - x1));

        }

        public void InitNonuniformCatmullRom(float x0, float x1, float x2, float x3, float dt0, float dt1, float dt2)
        {

            // compute tangents when parameterized in [t1,t2]
            var t1 = (x1 - x0) / dt0 - (x2 - x0) / (dt0 + dt1) + (x2 - x1) / dt1;
            var t2 = (x2 - x1) / dt1 - (x3 - x1) / (dt1 + dt2) + (x3 - x2) / dt2;

            // rescale tangents for parametrization in [0,1]
            t1 *= dt1;
            t2 *= dt1;

            Init(x1, x2, t1, t2);

        }

        public float Calc(float t)
        {

            float t2 = t * t;
            float t3 = t2 * t;
            return c0 + c1 * t + c2 * t2 + c3 * t3;

        }

    }
     
}