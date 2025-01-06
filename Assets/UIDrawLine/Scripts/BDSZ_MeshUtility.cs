using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BDSZ_2020
{
    /// <summary>
    /// 模型面组成方式。
    /// </summary>
    public enum EMeshTopology
    {
        /// <summary>
        /// 三角形
        /// </summary>
        Triangles,
        /// <summary>
        /// 四边形
        /// </summary>
        Quadrangle,
        /// <summary>
        /// 圆形
        /// </summary>
        Circle,
        /// <summary>
        /// 带子
        /// </summary>
        Ribbon,
        /// <summary>
        /// 线段
        /// </summary>
        Lines,
        /// <summary>
        /// 线条
        /// </summary>
        LineStrip,
        /// <summary>
        /// 模型拓扑个数
        /// </summary>
        Count,
    };
    /// <summary>
    /// 模型辅助模块
    /// </summary>
    public class BDSZ_MeshUtility
    {
        #region Instance
        private static BDSZ_MeshUtility s_instance = null;
        /// <summary>
        /// 模型辅助实例
        /// </summary>
        public static BDSZ_MeshUtility Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new BDSZ_MeshUtility();
                }
                return s_instance;
            }
        }
        #endregion

        List<ushort>[] m_MeshTriangles = new List<ushort>[(int)EMeshTopology.Count];
        List<BDSZ_DrawMeshHelper> m_MeshHelps = new List<BDSZ_DrawMeshHelper>();
 
        /// <summary>
        /// 返回一个模型绘制实例
        /// </summary>
        /// <returns></returns>
        public BDSZ_DrawMeshHelper GetMeshHelper()
        {
            BDSZ_DrawMeshHelper dm = null;
            if (m_MeshHelps.Count > 0)
            {
                dm = m_MeshHelps[m_MeshHelps.Count - 1];
                m_MeshHelps.RemoveAt(m_MeshHelps.Count - 1);
                dm.Clear();
            }
            else
            {
                dm = new BDSZ_DrawMeshHelper();
            }
            return dm;
        }
        /// <summary>
        /// 释放一个模型绘制实例
        /// </summary>
        /// <param name="mesh"></param>
        public void ReleaseMeshHelper(BDSZ_DrawMeshHelper mesh)
        {
            m_MeshHelps.Add(mesh);
        }
        /// <summary>
        /// 根据线段方向，计算横向方向。
        /// </summary>
        /// <param name="lineSegments"></param>
        /// <param name="vYAxis"></param>
        /// <returns></returns>
        static public bool CalcYAxis(List<Vector3> lineSegments, List<Vector3> vYAxis)
        {

            Vector3 vLastDir = lineSegments[1] - lineSegments[0];
            vLastDir.Normalize();
            if (lineSegments.Count == 2)
            {
                Vector3 vAxis = Vector3.Cross(vLastDir, Vector3.up);
                vYAxis.Add(vAxis);
                vYAxis.Add(vAxis);
                return true;
            }
            int iLast = lineSegments.Count - 1;
            bool bIsClosed = lineSegments[0] == lineSegments[iLast];
            Vector3 vTemp, vCurrentDir = Vector3.forward, vUp = Vector3.up;
            if (bIsClosed == true)
            {
                vYAxis.Add(Vector3.right);
                for (int i = 1; i < lineSegments.Count; i++)
                {
                    int iNext = i + 1;
                    if (iNext == lineSegments.Count)
                        iNext = 1;
                    vCurrentDir = lineSegments[iNext] - lineSegments[i];
                    vCurrentDir.Normalize();
                    Vector3 vAvarageDir = (vLastDir + vCurrentDir) * 0.5f;
                    vAvarageDir.Normalize();
                    float fDot = Vector3.Dot(vAvarageDir, vLastDir);
                    vTemp = Vector3.Cross(vUp, vAvarageDir);
                    vTemp.Normalize();
                    vTemp *= (1.0f / fDot);
                    vYAxis.Add(vTemp);
                    vLastDir = vCurrentDir;
                }
                vYAxis[0] = vYAxis[iLast];
            }
            else
            {

                for (int i = 1; i < lineSegments.Count; i++)
                {
                    if (i + 1 < lineSegments.Count)
                    {
                        vCurrentDir = lineSegments[i + 1] - lineSegments[i];
                        vCurrentDir.Normalize();
                    }
                    if (i == 1 || i + 1 == lineSegments.Count)
                    {
                        vTemp = Vector3.Cross(vUp, vLastDir);
                        vTemp.Normalize();
                        vYAxis.Add(vTemp);
                        if (i + 1 == lineSegments.Count)
                            break;
                    }
                    Vector3 vAvarageDir = (vLastDir + vCurrentDir) * 0.5f;
                    vAvarageDir.Normalize();
                    float fDot = Vector3.Dot(vAvarageDir, vLastDir);
                    vTemp = Vector3.Cross(vUp, vAvarageDir);
                    vTemp.Normalize();
                    vTemp *= (1.0f / fDot);
                    vYAxis.Add(vTemp);
                    vLastDir = vCurrentDir;
                }
            }
            return true;
        }
 
        /// <summary>
        /// 顶点变换
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="vSrc"></param>
        /// <returns></returns>
        public static Vector3 TransformPoint(Matrix4x4 matrix, Vector3 vSrc)
        {
            Vector3 vResult = Vector3.zero;
            vResult.x = vSrc.x * matrix.m00 + vSrc.y * matrix.m10 + vSrc.z * matrix.m20 + matrix.m30;
            vResult.y = vSrc.x * matrix.m01 + vSrc.y * matrix.m11 + vSrc.z * matrix.m21 + matrix.m31;
            vResult.z = vSrc.x * matrix.m02 + vSrc.y * matrix.m12 + vSrc.z * matrix.m22 + matrix.m32;

            //vResult.w = vSrc.x * matrix[3] + vSrc.y * matrix[7] + vSrc.z * matrix[11] +  matrix[15];
            return vResult;

        }
        int GetTriangleCount(EMeshTopology tp, int iVertexCount)
        {

            if (tp == EMeshTopology.Circle)
            {
                if (iVertexCount < 3)
                {
                    Debug.LogWarning($"vertex = {iVertexCount} error!");
                    return 0;
                }
                return (iVertexCount - 2) * 3;
            }
            else if (tp == EMeshTopology.Quadrangle)
            {

                if ((iVertexCount % 4) > 0)
                {
                    Debug.LogWarning($"vertex = {iVertexCount} error!");
                    return 0;
                }
                return (iVertexCount / 2) * 3;
            }
            else if (tp == EMeshTopology.Ribbon)
            {
                if (iVertexCount < 4 || (iVertexCount % 2) > 0)
                {
                    Debug.LogWarning($"vertex = {iVertexCount} error!");
                    return 0;
                }
                return (iVertexCount - 2) * 3;
            }
            else if (tp == EMeshTopology.Lines)
            {
                if (iVertexCount < 2 || (iVertexCount % 2) > 0)
                {

                    Debug.LogWarning($"vertex = {iVertexCount} error!");
                    return 0;
                }
                return iVertexCount;
            }
            else if (tp == EMeshTopology.LineStrip)
            {
                if (iVertexCount < 2)
                {

                    Debug.LogWarning($"vertex = {iVertexCount} error!");
                    return 0;
                }
                return (iVertexCount);
            }
            else
            {
                if (iVertexCount == 0 || (iVertexCount % 3) > 0)
                {

                    Debug.LogWarning($"vertex = {iVertexCount} error!");
                    return 0;
                }
                return iVertexCount;
            }
        }
        /// <summary>
        /// 三角形顶点索引
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="iVertexCount"></param>
        /// <param name="iIndicesCount"></param>
        /// <returns></returns>
        public List<ushort> GetTriangles(EMeshTopology tp, int iVertexCount, ref int iIndicesCount)
        {

            iIndicesCount = GetTriangleCount(tp, iVertexCount);
            if (m_MeshTriangles[(int)tp] != null && m_MeshTriangles[(int)tp].Count >= iIndicesCount)
                return m_MeshTriangles[(int)tp];
            if (m_MeshTriangles[(int)tp] == null)
                m_MeshTriangles[(int)tp] = new List<ushort>();
            else
                m_MeshTriangles[(int)tp].Clear();
            List<ushort> triangles = m_MeshTriangles[(int)tp];
            if (tp == EMeshTopology.Circle)
            {

                int it = Mathf.Max(iIndicesCount / 3, 360);
                for (int i = 0; i < it; i++)
                {
                    triangles.Add(0);
                    triangles.Add((ushort)(i + 1));
                    triangles.Add((ushort)(i + 2));
                }

            }
            else if (tp == EMeshTopology.Quadrangle)
            {
                ushort iVertexIndex = 0;
                int iQuadCount = Mathf.Max(1024, iIndicesCount / 6);
                for (int i = 0; i < iQuadCount; i++)
                {
                    triangles.Add((ushort)(iVertexIndex + 0));
                    triangles.Add((ushort)(iVertexIndex + 1));
                    triangles.Add((ushort)(iVertexIndex + 2));

                    triangles.Add((ushort)(iVertexIndex + 0));
                    triangles.Add((ushort)(iVertexIndex + 2));
                    triangles.Add((ushort)(iVertexIndex + 3));

                    iVertexIndex += 4;
                }
            }
            else if (tp == EMeshTopology.Ribbon)
            {

                ushort iVertexIndex = 0;
                int it = Mathf.Max(1024, iIndicesCount / 6);
                for (int i = 0; i < it; i++)
                {
                    triangles.Add(iVertexIndex);
                    triangles.Add((ushort)(iVertexIndex + 2));
                    triangles.Add((ushort)(iVertexIndex + 3));

                    triangles.Add(iVertexIndex);
                    triangles.Add((ushort)(iVertexIndex + 3));
                    triangles.Add((ushort)(iVertexIndex + 1));

                    iVertexIndex += 2;
                }
            }
            else if (tp == EMeshTopology.Lines || tp == EMeshTopology.LineStrip)
            {
                int it = Mathf.Max(1024 * 2, iIndicesCount);
                ushort iVertexIndex = 0;
                for (int i = 0; i < it; i++)
                {
                    triangles.Add(iVertexIndex);
                    iVertexIndex++;
                }
            }
            else
            {

                ushort iVertexIndex = 0;
                for (int i = 0; i < Mathf.Max(iIndicesCount / 3, 1024); i++)
                {
                    triangles.Add((ushort)iVertexIndex);
                    triangles.Add((ushort)(iVertexIndex + 1));
                    triangles.Add((ushort)(iVertexIndex + 2));
                    iVertexIndex += 3;
                }
            }
            return triangles;
        }
    }
   
}