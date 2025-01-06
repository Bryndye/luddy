using System.Collections.Generic;
using UnityEngine;
namespace BDSZ_2020
{
    /// <summary>
    /// ui 曲线
    /// </summary>
    public class BDSZ_UICurveBase  
    {
        /// <summary>
        /// 曲线关键点
        /// </summary>
        protected List<Vector2> m_vKeyPoints;
        /// <summary>
        /// 曲线长度。
        /// </summary>
        private float[] m_fCurveLengths;

        /// <summary>
        /// 关键点最小值和最大值。
        /// </summary>
        private Vector2 m_vKeyPointsMin = Vector2.zero;
        private Vector2 m_vKeyPointsMax = Vector2.zero;

        /// <summary>
        /// 返回曲线比率坐标。
        /// </summary>
        /// <param name="u">取值范围，0-1.0f</param>
        /// <returns></returns>
        public Vector2 GetPointAt(float u)
        {

            var t = this.GetUtoTmapping(u);
            return this.GetPoint(t);

        }
        /// <summary>
        /// 返回曲线比率 切线。
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public Vector2 GetTangentAt(float u)
        {

            var t = this.GetUtoTmapping(u);
            return this.GetTangent(t);

        }
        /// <summary>
        /// 曲线采样
        /// </summary>
        /// <param name="vLineVecs">返回采样的线段顶点</param>
        /// <param name="fMinDistance">采样2点之间的最小距离。</param>
        /// <returns></returns>      
        public bool Sampling(List<Vector2> vLineVecs,float fMinDistance = 4.0f)
        {
            fMinDistance = Mathf.Max(2.0f, fMinDistance);
            int iSamplingCount = Mathf.RoundToInt(GetLength() / fMinDistance);
            Vector2 vLast = m_vKeyPoints[0];
            vLineVecs.Add(vLast);
            for (int i = 1; i <= iSamplingCount; i++)
            {
                float u = (float)i / iSamplingCount;
                Vector2 vTemp = GetPoint(u);
                float fd = (vTemp - vLast).sqrMagnitude;
                if(fd>= fMinDistance * fMinDistance || i== iSamplingCount)
                { 
                    vLast = vTemp;
                    vLineVecs.Add(vLast);
                }
            }
            return true;
        }

        /// <summary>
        /// 曲线采样
        /// </summary>
        /// <param name="vLineVecs">返回采样的线段顶点。</param>
        /// <param name="vLineYAxis">线段顶点Y轴，垂直于线段方向。</param>
        /// <param name="fMinDistance">采样2点之间的最小距离。</param>
        /// <returns></returns>      
        public bool Sampling(List<Vector2> vLineVecs,List<Vector2> vLineYAxis, float fMinDistance = 4.0f)
        {
            vLineVecs.Clear();
            vLineYAxis.Clear();
            fMinDistance = Mathf.Max(2.0f, fMinDistance);
            int iSamplingCount = Mathf.RoundToInt(GetLength() / fMinDistance);
            Vector2 vLast = m_vKeyPoints[0];
            vLineVecs.Add(vLast);
            Vector2 vLastLineDir = GetTangent(0.0f);
            vLineYAxis.Add(new Vector2(vLastLineDir.y, -vLastLineDir.x));
            for (int i = 1; i <= iSamplingCount; i++)
            {
                float u = (float)i / iSamplingCount;
                Vector2 vTemp = GetPoint(u);
                float fd = (vTemp - vLast).sqrMagnitude;
                if (fd >= fMinDistance * fMinDistance || i == iSamplingCount)
                {
                    vLast = vTemp;
                    Vector2 vCurDir = GetTangent(u);

                    Vector2 vAvarageDir = (vLastLineDir + vCurDir) * 0.5f;
                    vAvarageDir.Normalize();
                    float fDot = Vector3.Dot(vAvarageDir, vLastLineDir);
                    vTemp = new Vector2(vAvarageDir.y, -vAvarageDir.x);
                    vTemp *= (1.0f / fDot);
                    vLineYAxis.Add(vTemp);
                    vLastLineDir = vCurDir;
                    vLineVecs.Add(vLast);
                }
            }
            return true;
        }
        /// <summary>
        /// 返回曲线长度。
        /// </summary>
        /// <returns></returns>
        public float GetLength()
        {
            return m_fCurveLengths[m_fCurveLengths.Length - 1];
        }

        /// <summary>
        /// 返回插值位置。
        /// </summary>
        /// <param name="t">t取值范围0-1.0f</param>
        /// <returns></returns>
        protected virtual Vector2 GetPoint(float t)
        {
            return Vector2.zero;
        }
     
        /// <summary>
        /// 更新曲线段长度。
        /// </summary>
        /// <param name="divisions"></param>
        protected void UpdateCurveData(int divisions = 200)
        {
            m_vKeyPointsMin = m_vKeyPoints[0];
            m_vKeyPointsMax = m_vKeyPoints[0];
            for(int i=1;i<m_vKeyPoints.Count;i++)
            {
                m_vKeyPointsMin = Vector2.Min(m_vKeyPointsMin, m_vKeyPoints[i]);
                m_vKeyPointsMax = Vector2.Max(m_vKeyPointsMax, m_vKeyPoints[i]);
            }

        
            float[] cache = new float[divisions + 1];
            Vector2 current, last = this.GetPoint(0);
            float sum = 0;
            cache[0] = 0;
            for (int p = 1; p <= divisions; p++)
            {

                current = this.GetPoint((float)p / divisions);
                sum += Vector2.Distance(current, last);
                cache[p] = (sum);
                last = current;

            }
            m_fCurveLengths = cache;

        }
         

        // 给定一个曲线长度比率，返回映射到曲线插值比率。
        float GetUtoTmapping(float u, float distance = 0)
        {
             
           
            int i, il= m_fCurveLengths.Length;
            float targetArcLength; // The targeted u distance value to get
            if (distance != 0)
            {
                targetArcLength = distance;
            }
            else
            {
                targetArcLength = u * m_fCurveLengths[il - 1];
            }

            // binary search for the index with largest value smaller than target u distance
            int low = 0, high = m_fCurveLengths.Length - 1;
            float comparison;

            while (low <= high)
            {

                i = Mathf.FloorToInt((float)(low + (high - low)) / 2f);

                comparison = m_fCurveLengths[i] - targetArcLength;

                if (comparison < 0)
                {
                    low = i + 1;
                }
                else if (comparison > 0)
                {
                    high = i - 1;
                }
                else
                {

                    high = i;
                    break;
                    // DONE
                }
            }
            i = high;
            if (m_fCurveLengths[i] == targetArcLength)
            {
                return i / (il - 1);
            }
            var lengthBefore = m_fCurveLengths[i];
            var lengthAfter = m_fCurveLengths[i + 1];
            var segmentLength = lengthAfter - lengthBefore;
            var segmentFraction = (targetArcLength - lengthBefore) / segmentLength;
            var t = (i + segmentFraction) / (il - 1);
            return t;
        }
        /// <summary>
        /// 计算曲线切线
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Vector2 GetTangent(float t)
        {

            var delta = 0.001f;
            var t1 = t - delta;
            var t2 = t + delta;

            // Capping in case of danger

            if (t1 < 0) t1 = 0;
            if (t2 > 1) t2 = 1;

            var pt1 = this.GetPoint(t1);
            var pt2 = this.GetPoint(t2);

            var tangent = (pt2 - pt1).normalized;

            return tangent;
        }

     

    }
}