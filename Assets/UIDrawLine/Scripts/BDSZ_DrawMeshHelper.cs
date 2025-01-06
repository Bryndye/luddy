using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BDSZ_2020
{

    public class BDSZ_DrawMeshHelper
    {

        /// <summary>
        /// ���ƻص�
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="iDrawType"></param>
        /// <param name="iVecBegin"></param>
        public delegate void DrawMeshHelperAction(BDSZ_DrawMeshHelper helper, int iDrawType, int iVecBegin);


        //֧�ֵ���󶥵����//
        public const int c_iMaxVertexCount = 64000;
        /// <summary>
        /// //���涥����Ϣ//
        /// </summary>
        public List<Vector3> m_Vertices = new List<Vector3>();
        /// <summary>
        /// ��������1
        /// </summary>
        public List<Vector2> m_uv1 = new List<Vector2>();
        /// <summary>
        /// //������ɫ��Ϣ//
        /// </summary>
        public List<Color32> m_Colors32 = new List<Color32>();
        /// <summary>
        /// ����
        /// </summary>
        public List<Vector3> m_vNormals = new List<Vector3>();
        /// <summary>
        /// ��������2
        /// </summary>
        public List<Vector4> m_uv2 = new List<Vector4>();
        /// <summary>
        /// ��������2
        /// </summary>
        public List<Vector4> m_uv3 = new List<Vector4>();

        /// <summary>
        /// ����
        /// </summary>
        public List<Vector4> m_Tangents = new List<Vector4>();
        /// <summary>
        /// ����ζ�������
        /// </summary>
        public List<ushort> m_Triangles = new List<ushort>();


        private ushort m_usBeginDrawIndex = 0;

        private EMeshTopology m_Topology = EMeshTopology.Quadrangle;
        /// <summary>
        /// ģ�Ͷ������
        /// </summary>
        public int VertexCount { get { return m_Vertices.Count; } }

        /// <summary>
        /// ������Ϊ
        /// </summary>
        private List<DrawMeshHelperAction> m_DrawAction = new List<DrawMeshHelperAction>();

        /// <summary>
        /// ���һ��������Ϊ��
        /// </summary>
        /// <param name="ac"></param>
        public void AddDrawAction(DrawMeshHelperAction ac)
        {
            if (ac == null || m_DrawAction.Contains(ac))
                return;
            m_DrawAction.Add(ac);
        }
        /// <summary>
        /// �Ƴ�һ��������Ϊ��
        /// </summary>
        /// <param name="ac"></param>
        public void RemoveAction(DrawMeshHelperAction ac)
        {
            m_DrawAction.Remove(ac);
        }

        /// <summary>
        ///ִ����Ϊ��
        /// </summary>
        public void DoDrawAction(int iDrawType, int iVecBegin)
        {
            for (int i = 0; i < m_DrawAction.Count; i++)
            {
                m_DrawAction[i].Invoke(this, iDrawType, iVecBegin);
            }

        }
        /// <summary>
        /// �������ģ������
        /// </summary>
        public void Clear()
        {
            m_Vertices.Clear();
            m_vNormals.Clear();
            m_uv1.Clear();
            m_uv2.Clear();
            m_uv3.Clear();
            m_Tangents.Clear();
            m_Colors32.Clear();
            m_Triangles.Clear();

            m_usBeginDrawIndex = 0;
        }


        /// <summary>
        /// ���ƴ���=================
        /// </summary>
        /// <param name="lineSegments"></param>
        /// <param name="vXAxis"></param>
        /// <param name="fWidth"></param>
        /// <param name="verClr"></param>
        /// <returns></returns>

        public bool DrawRibbon(List<Vector3> lineSegments, List<Vector3> vXAxis, float fWidth, Color32 verClr)
        {
            if (m_Vertices.Count + lineSegments.Count * 2 > c_iMaxVertexCount)
                return false;
            BeginDraw(EMeshTopology.Ribbon);
            if (vXAxis.Count == 0)
            {
                BDSZ_MeshUtility.CalcYAxis(lineSegments, vXAxis);
            }
            float fHalfWidth = fWidth * 0.5f;
            float fV = 0.0f;
            int iBeginIndex = m_Vertices.Count;
            for (int i = 0; i < lineSegments.Count; i++)
            {
                if (i > 0)
                {
                    fV += (lineSegments[i] - lineSegments[i - 1]).magnitude;
                }
                m_Vertices.Add(lineSegments[i] + vXAxis[i] * fHalfWidth);
                m_Vertices.Add(lineSegments[i] - vXAxis[i] * fHalfWidth);
                m_uv1.Add(new Vector2(1.0f, fV));
                m_uv1.Add(new Vector2(0.0f, fV));
                m_uv2.Add(Vector2.zero);
                m_uv2.Add(Vector2.zero);
                m_Colors32.Add(verClr);
                m_Colors32.Add(verClr);
            }

            for (int i = iBeginIndex; i < m_Vertices.Count; i += 2)
            {
                Vector2 uv = m_uv1[i];
                uv.y /= fV;
                m_uv1[i] = uv;
                uv = m_uv1[i + 1];
                uv.y /= fV;
                m_uv1[i + 1] = uv;
            }
            return true;

        }

        /// <summary>
        /// ����һ������
        /// </summary>
        /// <param name="vCenter"></param>
        /// <param name="fWidth"></param>
        /// <param name="fHeight"></param>
        /// <param name="clr"></param>
        /// <param name="fRotateDegree"></param>
        /// <returns></returns>
        public bool DrawRectangle(Vector2 vCenter, float fWidth, float fHeight, Color32 clr, float fRotateDegree = 0.0f)
        {
            if (VertexCount + 4 > c_iMaxVertexCount)
                return false;
            if (fWidth < 1.0f || fHeight < 1.0f)
                return false;
            m_uv1.Add(new Vector2(0.0f, 1.0f));
            m_uv2.Add(new Vector2(-0.5f * fWidth, -0.5f * fHeight));

            m_uv1.Add(new Vector2(1.0f, 1.0f));
            m_uv2.Add(new Vector2(0.5f * fWidth, -0.5f * fHeight));

            m_uv1.Add(new Vector2(1.0f, 0.0f));
            m_uv2.Add(new Vector2(0.5f * fWidth, 0.5f * fHeight));
            m_uv1.Add(new Vector2(0.0f, 0.0f));
            m_uv2.Add(new Vector2(-0.5f * fWidth, 0.5f * fHeight));

            float fRotateAngle = fRotateDegree * Mathf.Deg2Rad;
            Vector2 vYAxis = new Vector2(Mathf.Sin(fRotateAngle), Mathf.Cos(fRotateAngle));
            Vector2 vXAxis = new Vector2(vYAxis.y, -vYAxis.x);

            m_Vertices.Add(vCenter - vXAxis * fWidth * 0.5f - vYAxis * fHeight * 0.5f);
            m_Vertices.Add(vCenter + vXAxis * fWidth * 0.5f - vYAxis * fHeight * 0.5f);
            m_Vertices.Add(vCenter + vXAxis * fWidth * 0.5f + vYAxis * fHeight * 0.5f);
            m_Vertices.Add(vCenter - vXAxis * fWidth * 0.5f + vYAxis * fHeight * 0.5f);

            m_Colors32.Add(clr);
            m_Colors32.Add(clr);
            m_Colors32.Add(clr);
            m_Colors32.Add(clr);
            return true;
        }

        /// <summary>
        /// ��ʼ����
        /// </summary>
        /// <param name="topology"></param>
        public void BeginDraw(EMeshTopology topology = EMeshTopology.Quadrangle)
        {
            if (m_Vertices.Count == 0)
            {
                m_Topology = topology;
            }
            else
            {
                if (NeedUpdateTriangle(topology))
                {
                    UpdateTriangle();

                    m_Topology = topology;
                }
            }
        }
        /// <summary>
        /// ���һ��ģ�Ͷ��㡣
        /// </summary>
        /// <param name="vPos"></param>
        /// <param name="uv"></param>
        /// <param name="clr"></param>
        public void AddVertex(Vector3 vPos, Vector2 uv, Color32 clr)
        {

            if (m_Vertices.Count >= c_iMaxVertexCount)
            {
                Debug.LogWarning($"Add vertex count {m_Vertices.Count} too more! ");
                return;
            }
            m_Vertices.Add(vPos);
            m_uv1.Add(uv);
            m_Colors32.Add(clr);
        }
        /// <summary>
        /// ����ģ����Ҫ�������������
        /// </summary>
        /// <param name="channels"></param>
        public void SetChannels(AdditionalCanvasShaderChannels channels)
        {
            if ((channels & AdditionalCanvasShaderChannels.TexCoord2) > 0)
            {
                for (int i = m_uv2.Count; i < m_Vertices.Count; i++)
                {
                    m_uv2.Add(Vector2.one);
                }
            }
            if ((channels & AdditionalCanvasShaderChannels.TexCoord3) > 0)
            {
                for (int i = m_uv3.Count; i < m_Vertices.Count; i++)
                {
                    m_uv3.Add(Vector2.one);
                }
            }
            if((channels & AdditionalCanvasShaderChannels.Tangent)>0)
            {
                Vector4 vclip = new Vector4(-1.0f, -1.0f, 1.0f, 1.0f);
                for (int i = m_Tangents.Count; i < m_Vertices.Count; i++)
                {
                    m_Tangents.Add(vclip);
                }
            }
        }
        /// <summary>
        /// ����ģ������������
        /// </summary>
        /// <param name="triangles"></param>
        public void SetTriangles(ushort[] triangles)
        {
            for (int i = 0; i < triangles.Length; i++)
            {
                m_Triangles.Add((ushort)(m_usBeginDrawIndex + triangles[i]));
            }
            m_usBeginDrawIndex = (ushort)m_Vertices.Count;
        }
        /// <summary>
        /// ���ģ��
        /// </summary>
        /// <param name="mesh"></param>
        public void FillMesh(Mesh mesh)
        {

            if (mesh.vertexCount > 0)
            {
                mesh.Clear();
            }
            if (m_Vertices.Count == 0)
                return;
            mesh.SetVertices(m_Vertices);
            mesh.SetUVs(0, m_uv1);
            mesh.SetColors(m_Colors32);
            if (m_uv2.Count == m_Vertices.Count)
            {
                mesh.SetUVs(1, m_uv2);
            }
            if (m_uv3.Count == m_Vertices.Count)
            {
                mesh.SetUVs(2, m_uv3);
            }
            if (m_Tangents.Count == m_Vertices.Count)
            {
                mesh.SetTangents(m_Tangents);
            }
            if (m_vNormals.Count == m_Vertices.Count)
            {
                mesh.SetNormals(m_vNormals);
            }

            if (m_usBeginDrawIndex == 0)
            {
                int iIndicesCount = 0;
                List<ushort> triangles = BDSZ_MeshUtility.Instance.GetTriangles(m_Topology, m_Vertices.Count, ref iIndicesCount);
                if (m_Topology == EMeshTopology.LineStrip)
                {
                    mesh.SetIndices(triangles, 0, iIndicesCount, MeshTopology.LineStrip, 0);
                }
                else if (m_Topology == EMeshTopology.Lines)
                {
                    mesh.SetIndices(triangles, 0, iIndicesCount, MeshTopology.Lines, 0);
                }
                else
                {
                    mesh.SetTriangles(triangles, 0, iIndicesCount, 0);
                }

            }
            else
            {
                if (m_usBeginDrawIndex != m_Vertices.Count)
                {
                    UpdateTriangle();
                }
                mesh.SetTriangles(m_Triangles, 0);
            }

        }
        /// <summary>
        /// ����ģ������
        /// </summary>
        public void UpdateTriangle()
        {
            int iAddVertexCount = m_Vertices.Count - m_usBeginDrawIndex;
            if (iAddVertexCount == 0)
                return;
            int iIndicesCount = 0;
            List<ushort> triangles = BDSZ_MeshUtility.Instance.GetTriangles(m_Topology, iAddVertexCount, ref iIndicesCount);
            for (int i = 0; i < iIndicesCount; i++)
            {
                m_Triangles.Add((ushort)(m_usBeginDrawIndex + triangles[i]));
            }
            m_usBeginDrawIndex = (ushort)m_Vertices.Count;

        }

        /// <summary>
        /// ���õ�һ��������������
        /// </summary>
        /// <param name="iIndex"></param>
        /// <param name="uv"></param>
        public void SetUV1(int iIndex, Vector4 uv)
        {
            if (m_uv1.Count <= iIndex)
                m_uv1.Add(new Vector2(uv.x, uv.y));
            else
                m_uv1[iIndex] = new Vector2(uv.x, uv.y);
        }
        /// <summary>
        /// ���õڶ���������������
        /// </summary>
        /// <param name="iIndex"></param>
        /// <param name="uv"></param>
        public void SetUV2(int iIndex, Vector4 uv)
        {
            if (m_uv2.Count <= iIndex)
                m_uv2.Add(uv);
            else
                m_uv2[iIndex] = uv;
        }
        /// <summary>
        /// ���õ�����������������
        /// </summary>
        /// <param name="iIndex"></param>
        /// <param name="uv"></param>
        public void SetUV3(int iIndex, Vector4 uv)
        {
            if (m_uv3.Count <= iIndex)
                m_uv3.Add(uv);
            else
                m_uv3[iIndex] = uv;
        }
        /// <summary>
        /// �Ƿ���Ҫ����������
        /// </summary>
        /// <param name="topology"></param>
        /// <returns></returns>
        bool NeedUpdateTriangle(EMeshTopology topology)
        {
            if (m_Topology != topology == true)
                return true;
            if (m_Topology == EMeshTopology.Circle ||
                m_Topology == EMeshTopology.Ribbon)
                return true;
            return false;
        }
    }

}