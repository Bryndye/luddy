using System.Collections.Generic;
using UnityEngine;
namespace BDSZ_2020
{
    /// <summary>
    /// ui ����
    /// </summary>
    public class BDSZ_UICurveBase  
    {
        /// <summary>
        /// ���߹ؼ���
        /// </summary>
        protected List<Vector2> m_vKeyPoints;
        /// <summary>
        /// ���߳��ȡ�
        /// </summary>
        private float[] m_fCurveLengths;

        /// <summary>
        /// �ؼ�����Сֵ�����ֵ��
        /// </summary>
        private Vector2 m_vKeyPointsMin = Vector2.zero;
        private Vector2 m_vKeyPointsMax = Vector2.zero;

        /// <summary>
        /// �������߱������ꡣ
        /// </summary>
        /// <param name="u">ȡֵ��Χ��0-1.0f</param>
        /// <returns></returns>
        public Vector2 GetPointAt(float u)
        {

            var t = this.GetUtoTmapping(u);
            return this.GetPoint(t);

        }
        /// <summary>
        /// �������߱��� ���ߡ�
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public Vector2 GetTangentAt(float u)
        {

            var t = this.GetUtoTmapping(u);
            return this.GetTangent(t);

        }
        /// <summary>
        /// ���߲���
        /// </summary>
        /// <param name="vLineVecs">���ز������߶ζ���</param>
        /// <param name="fMinDistance">����2��֮�����С���롣</param>
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
        /// ���߲���
        /// </summary>
        /// <param name="vLineVecs">���ز������߶ζ��㡣</param>
        /// <param name="vLineYAxis">�߶ζ���Y�ᣬ��ֱ���߶η���</param>
        /// <param name="fMinDistance">����2��֮�����С���롣</param>
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
        /// �������߳��ȡ�
        /// </summary>
        /// <returns></returns>
        public float GetLength()
        {
            return m_fCurveLengths[m_fCurveLengths.Length - 1];
        }

        /// <summary>
        /// ���ز�ֵλ�á�
        /// </summary>
        /// <param name="t">tȡֵ��Χ0-1.0f</param>
        /// <returns></returns>
        protected virtual Vector2 GetPoint(float t)
        {
            return Vector2.zero;
        }
     
        /// <summary>
        /// �������߶γ��ȡ�
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
         

        // ����һ�����߳��ȱ��ʣ�����ӳ�䵽���߲�ֵ���ʡ�
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
        /// ������������
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