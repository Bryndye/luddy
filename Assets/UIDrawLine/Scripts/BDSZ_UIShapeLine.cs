using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace BDSZ_2020
{
    
    [RequireComponent(typeof(CanvasRenderer))]
     public class BDSZ_UIShapeLine : MaskableGraphic
	{
        //曲线采样距离，默认为8个像素。
        const float c_fSamplingDistance = 4.0f;
        //允许最大顶点数。
        const int c_iMaxLineVecCount = 10000;

        #region InspectorFields

        [SerializeField]
        //曲线采样模式。
        protected EUILineSplineType m_LineSpline = EUILineSplineType.Line;
        [SerializeField]
        [Range(0, 10)]
        //边缘抗锯齿像素个数//
        protected int m_iSoftness = 2;
        [SerializeField]
        private bool m_bSequential = false;
        [SerializeField]
        private bool m_bRoundend = true;
        [SerializeField]
        private bool m_bDrawSelf = true;
        [SerializeField]
        protected bool m_bGradientColor = false;
        [SerializeField]
        protected Color m_GradientColor = Color.black;
        [SerializeField]
        protected bool m_bDashed = false;
        [SerializeField]
        [Range(1, 50)]
        //虚线中，实线长度//
        protected int m_iSolidLength = 8;
        [SerializeField]
        [Range(1, 50)]
        //虚线中，空白长度//
        protected int m_iSpaceLength = 4;

        [FormerlySerializedAs("m_Tex")]
        [SerializeField] Texture m_Texture;

    
        #endregion
        #region Public properties
        /// <summary>
        /// Texture used by the line
        /// </summary>
        public override Texture mainTexture
        {
            get
            {
                if (material != null && material.mainTexture != null)
                {
                    return material.mainTexture;
                }
                return s_WhiteTexture;
            }
        }
        public bool isGradientColor { get { return m_bGradientColor; } }
        public bool isDashed { get { return m_bDashed; } }
        public bool sequential { get { return m_bSequential; } set { sequential = value; } }

        public int softness {  get { return m_iSoftness; } set { m_iSoftness = Mathf.Clamp(value, 0, 50); } }
        #endregion

        //线段数据================================================

        private List<Vector2> m_LineLocalVecs = new List<Vector2>();
        private List<Color32> m_LineColors = new List<Color32>();
        private List<float> m_LineWidths = new List<float>();
        protected List<Vector2> m_SplineLines = new List<Vector2>();
        protected List<Vector2> m_SplineYAxis = new List<Vector2>();
        protected List<Vector2> m_LineVecs = new List<Vector2>();
        //线段数据================================================
        public void Clean()
        {
            m_LineVecs.Clear();
            m_LineColors.Clear();
            m_LineWidths.Clear();
            m_SplineLines.Clear();
            m_SplineYAxis.Clear();
            SetVerticesDirty();

        }
        /// <summary>
        /// 添加一条线段，坐标为pixelRect 的比率。
        /// </summary>
        /// <param name="vStart"></param>
        /// <param name="vEnd"></param>
        /// <param name="fWidth"></param>
        /// <param name="clr"></param>
        public void AddLineSegment(Vector2 vStart, Vector2 vEnd, float fWidth, Color32 clr)
        {
            if(m_LineVecs.Count>c_iMaxLineVecCount)
            {
                Debug.LogWarning("too many vertex!");
                return;
            }
            m_LineVecs.Add(vStart);
            m_LineVecs.Add(vEnd);
            m_LineColors.Add(clr);
            m_LineColors.Add(clr);
            m_LineWidths.Add(fWidth);
        }
        /// <summary>
        /// 允许矩形裁剪
        /// </summary>
        /// <param name="rcClip"></param>
        public void EnableRectClipping(Rect rcClip)
        {
            canvasRenderer.EnableRectClipping(rcClip);
        }
        /// <summary>
        /// 禁止矩形裁剪。
        /// </summary>
        public void DisableRectClipping()
        {
            canvasRenderer.DisableRectClipping();
        }

        protected override void Awake ()
        {
            base.Awake();
            BuildMaterial();
        }
        protected virtual void BuildMaterial()
        {
            m_Material = BDSZ_UILineUtility.GetMaterial(m_Texture, m_iSoftness, 0, Color.black, m_bGradientColor, m_GradientColor);
             
        }
        void DrawClient(BDSZ_DrawMeshHelper drawMeshHelper, Rect rcClient)
        {

            float fr;
            Vector2 vBegin, vEnd;
            if (rcClient.width > rcClient.height)
            {
                fr = rcClient.height * 0.5f;
                vBegin = new Vector2(rcClient.xMin, rcClient.yMin + fr);
                vEnd = new Vector2(rcClient.xMax, rcClient.yMin + fr);
            }
            else
            {
                fr = rcClient.width * 0.5f;
                vBegin = new Vector2(rcClient.xMin + fr, rcClient.yMin);
                vEnd = new Vector2(rcClient.xMin + fr, rcClient.yMax);
            }
            if (m_bDashed == true)
            {
                BDSZ_UILineUtility.DrawLine(drawMeshHelper, vBegin, vEnd, fr * 2.0f, color, color, m_bRoundend, 1.0f, m_iSolidLength, m_iSpaceLength);
            }
            else
            {
                BDSZ_UILineUtility.DrawLine(drawMeshHelper, vBegin, vEnd, fr * 2.0f, color, m_bRoundend);
            }
        }
    
        Vector2 LocalToCanvas (Vector2 vl,Rect rcClient)
        {
            float fx = rcClient.xMin + rcClient.width * vl.x;
            float fy = rcClient.yMin + rcClient.height * vl.y;
            return new Vector2(fx, fy);
        }
        void DrawLineSegments(BDSZ_DrawMeshHelper drawMeshHelper, Rect rcClient)
        {

            m_LineLocalVecs.Clear();
        
            if (m_LineSpline == EUILineSplineType.Line)
            {
                for (int i = 0; i < m_LineVecs.Count; i++)
                {
                    m_LineLocalVecs.Add(LocalToCanvas(m_LineVecs[i],rcClient));
                }
            }
            else
            {

                for (int i = 0; i < m_LineVecs.Count; i+=2)
                {
                    m_LineLocalVecs.Add(LocalToCanvas(m_LineVecs[i], rcClient));
                    
                }
                m_LineLocalVecs.Add(LocalToCanvas(m_LineVecs[m_LineVecs.Count - 1], rcClient));
            }
            int iSolidLength = 0, iSpaceLength = 0;
            if(m_bDashed)
            {
                iSolidLength = m_iSolidLength;
                iSpaceLength = m_iSpaceLength;
            }
            if (m_LineLocalVecs.Count>2 && m_LineSpline != EUILineSplineType.Line)
            {
 
                BDSZ_UISplineCurve splineCurve = new BDSZ_UISplineCurve(m_LineLocalVecs, false, m_LineSpline);
                splineCurve.Sampling(m_SplineLines, m_SplineYAxis, c_fSamplingDistance);
                List<Color32> splineColor = new List<Color32>();
                Vector2 vLineDir = m_LineLocalVecs[0] - m_LineLocalVecs[1]; 
                int iBegin = 0;
                for(int i=0;i<m_SplineLines.Count;i++)
                {
                    Vector2 vCurveDir = (m_SplineLines[i] - m_LineLocalVecs[iBegin+1]);
                    float fd2 = Vector2.Dot(vCurveDir, vLineDir);
                    if(fd2<0.0f)
                    {
                        iBegin++;
                        if (iBegin >= m_LineLocalVecs.Count - 1)
                        {
                            iBegin = m_LineColors.Count - 1;
                            break;
                        }
                        vLineDir = m_LineLocalVecs[iBegin] - m_LineLocalVecs[iBegin + 1];
                    }
                    splineColor.Add(m_LineColors[iBegin*2]);
                }
                for(int i=splineColor.Count;i<m_SplineLines.Count;i++)
                {
                    splineColor.Add(m_LineColors[iBegin]);
                }
                BDSZ_UILineUtility.DrawLineStrips(drawMeshHelper, m_SplineLines,m_SplineYAxis, splineColor, m_LineWidths[0], m_bRoundend, iSolidLength, iSpaceLength);

            }
            else
            {
                if (m_bSequential == true)
                {
                    BDSZ_UILineUtility.DrawLines(drawMeshHelper, m_LineLocalVecs, m_LineColors, m_LineWidths[0], m_bRoundend,iSolidLength, iSpaceLength);
                }
                else
                {
                    for (int i = 0; i < m_LineLocalVecs.Count; i += 2)
                    {
                        BDSZ_UILineUtility.DrawLine(drawMeshHelper, m_LineLocalVecs[i], m_LineLocalVecs[i + 1], m_LineWidths[i/2], m_LineColors[i],m_LineColors[i+1], m_bRoundend,1.0f,iSolidLength,iSpaceLength);
                    }
                }
            }
        }
        protected override void UpdateGeometry()
        {
            if (m_Material == null)
            {
                BuildMaterial();
            }

            BDSZ_DrawMeshHelper drawHelper = BDSZ_MeshUtility.Instance.GetMeshHelper();
            
            int iVecBegin = drawHelper.VertexCount;
            Rect rcClient = GetPixelAdjustedRect();
            if (m_LineVecs.Count >= 2)
            {
                DrawLineSegments(drawHelper,rcClient);
            }
            else
            {
                if (m_bDrawSelf == true)
                {
                    DrawClient(drawHelper, rcClient);
                }
            }

            drawHelper.FillMesh(workerMesh);

            BDSZ_MeshUtility.Instance.ReleaseMeshHelper(drawHelper);
            canvasRenderer.SetMesh(workerMesh);
        }
     
      
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            BuildMaterial();
            SetAllDirty();
        }
#endif
    }

}
