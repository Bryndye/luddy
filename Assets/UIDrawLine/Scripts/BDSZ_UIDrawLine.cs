using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace BDSZ_2020
{
    [RequireComponent(typeof(CanvasRenderer))]
    [Obsolete("This overload is obsolete from current version  and will be removed.")]
    public class BDSZ_UIDrawLine : MaskableGraphic
	{

        const int c_iMaxLineVecCount = 10000;
        const int c_iLineTextRadius = 32;
        const int c_iSmoothScale = 8;
        #region Material 
        protected static Material s_RoundEndMaterial=null;
        protected static Material s_RectEndMaterial = null;
        protected static Material s_LineMaterial = null;
        protected static Texture2D s_CircleTexture = null;
        protected static Texture2D s_RectTexture = null;
        protected static Texture2D RectTexture
        {
            get
            {
                if (s_RectTexture == null)
                {
                    int iw = c_iLineTextRadius * 2 + 1;

                    s_RectTexture = new Texture2D(iw, iw, TextureFormat.ARGB32, false);
                    for (int i = 0; i <= c_iLineTextRadius * 2; i++)
                    {
                        for (int j = 0; j <= c_iLineTextRadius * 2; j++)
                        {
                            float fx = Mathf.Abs(i - c_iLineTextRadius);
                            float fy = Mathf.Abs(j - c_iLineTextRadius);
                            float fd = 1.0f - Mathf.Max(fx, fy) / c_iLineTextRadius;
                            s_RectTexture.SetPixel(j, i, new Color(1.0f, 1.0f, 1.0f, fd));
                        }
                    }
                    s_RectTexture.wrapMode = TextureWrapMode.Clamp;
                    s_RectTexture.Apply();
                }
                return s_RectTexture;
            }
        }
        protected static Texture2D CircleTexture
        {
            get
            {
                if (s_CircleTexture == null)
                {
                    int iw = c_iLineTextRadius * 2 + 1 ;
                    s_CircleTexture = new Texture2D(iw, iw, TextureFormat.ARGB32, false);
                    for (int i = 0; i <= c_iLineTextRadius * 2; i++)
                    {
                        for (int j = 0; j <= c_iLineTextRadius * 2; j++)
                        {
                            float fx = Mathf.Abs(i - c_iLineTextRadius);
                            float fy = Mathf.Abs(j - c_iLineTextRadius);
                            float fd = Mathf.Sqrt(fx * fx + fy * fy) / c_iLineTextRadius;
                            float t = 1.0f - fd; 
                            s_CircleTexture.SetPixel(j, i, new Color(1.0f, 1.0f, 1.0f, t));
                        }
                    }
                    s_CircleTexture.wrapMode = TextureWrapMode.Clamp;
                    s_CircleTexture.Apply();
                }
                return s_CircleTexture;
            }
        }
        static Material CreateMaterial(Texture2D text)
        {
            Shader shader = Shader.Find("UIShader/DrawLine");
            Material mat  = new Material(shader)
            {
                mainTexture = text
            };
            return mat;
        }
        static Material RoundEndLineMaterial
        {
            get
            {
                if (s_RoundEndMaterial == null)
                {
                    s_RoundEndMaterial = CreateMaterial(CircleTexture);
                }
                return s_RoundEndMaterial;
            }
        }
        static Material RectEndLineMaterial
        {
            get
            {
                if (s_RectEndMaterial == null)
                {
                    s_RectEndMaterial = CreateMaterial(RectTexture);
                }
                return s_RectEndMaterial;
            }
        }
        static Material LineMaterial
        {
            get
            {
                if (s_LineMaterial == null)
                {
                    s_LineMaterial = CreateMaterial(Texture2D.whiteTexture);
                }
                return s_LineMaterial;
            }
        }
        #endregion
        #region InspectorFields
        [SerializeField]
        protected bool m_bAntiAliased = true;
        [SerializeField]
        protected bool m_bSequential = false;
        [SerializeField]
        protected bool m_bBestAntialiased = true;
        [SerializeField]
        protected bool m_bRoundEnd = true;
        [SerializeField]
        protected bool m_bStencilMask = false;
        [SerializeField]
        protected int m_iStencilComparison = 2;
        [SerializeField]
        protected int m_iStencilID = 1;
        [SerializeField]
        protected int m_iStencilOperation = 0;
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
        public bool AntiAliased
        {
            get { return m_bAntiAliased; }
            set
            {
                if(m_bAntiAliased!=value)
                {
                    m_bAntiAliased = value;
                    BuildMaterial();
                    SetVerticesDirty();
                }
            }
        }
        public bool Sequential
        {
            get { return m_bSequential; }
            set
            {
                if (m_bSequential != value)
                {
                    m_bSequential = value;
                    SetVerticesDirty();
                }
            }
        }
        #endregion

        //线段数据================================================
        Vector2 m_vLastVec1;
        Vector2 m_vLastVec2;
        private List<Vector2> m_LineVecs = new List<Vector2>();
        private List<Color32> m_LineColors = new List<Color32>();
        private List<float>   m_LineWidths = new List<float>();
        //线段数据================================================
        public void Clean()
        {
            m_LineVecs.Clear();
            m_LineColors.Clear();
            m_LineWidths.Clear();
            SetVerticesDirty();

        }
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
            m_LineWidths.Add(fWidth);
        }
        
        protected override void Awake ()
        {
            base.Awake();
            BuildMaterial();
        }
        protected virtual void BuildMaterial()
        {
            Material matSrc = null;
            if (m_bAntiAliased == false)
            {
                matSrc = LineMaterial;
            }
            else
            {
                if (m_bRoundEnd)
                    matSrc = RoundEndLineMaterial;
                else
                    matSrc = RectEndLineMaterial;
            }
            m_Material = new Material(matSrc);
            m_Material.SetColor("_Color", color);
            if (m_bStencilMask == true)
            {
                m_Material.SetInt("_StencilComp", m_iStencilComparison);
                m_Material.SetInt("_Stencil", m_iStencilID);
                m_Material.SetInt("_StencilOp", m_iStencilOperation);
              
            }
        }
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            var r = GetPixelAdjustedRect();
            toFill.Clear();
            if (m_LineVecs.Count == 0)
            {
                int iv = 0;
                Vector2 vBegin, vEnd,vYAxis;
                float fr;
                if(r.width>r.height)
                {
                    fr = r.height * 0.5f;
                    vBegin = new Vector2(r.xMin, r.yMin + fr);
                    vEnd = new Vector2(r.xMax, r.yMin + fr);
                }
                else
                {
                    fr = r.width * 0.5f;
                    vBegin = new Vector2(r.xMin + fr, r.yMin);
                    vEnd = new Vector2(r.xMin + fr, r.yMax);
                }
                Vector2 vDir = SafeNormalLineDir(vBegin, vEnd);
                vYAxis = new Vector2(vDir.y, -vDir.x);
                Color32 lineClr =   CalcLineColor(Color.white, fr);
                if (m_bAntiAliased==true)
                {
                
                    AddVertex(toFill, vBegin - vYAxis * fr, lineClr, new Vector2(0.0f, 0.0f));
                    AddVertex(toFill, vBegin + vYAxis * fr, lineClr, new Vector2(1.0f, 0.0f));

                    if(r.width!=r.height)
                    {
                        Vector2 vTop = vBegin + vDir * fr;
                        Vector2 vBot = vEnd - vDir * fr;
                        AddVertex(toFill, vTop - vYAxis * fr, lineClr, new Vector2(0.0f, 0.5f));
                        AddVertex(toFill, vTop + vYAxis * fr, lineClr, new Vector2(1.0f, 0.5f));
                        AddVertex(toFill, vBot - vYAxis * fr, lineClr, new Vector2(0.0f, 0.5f));
                        AddVertex(toFill, vBot + vYAxis * fr, lineClr, new Vector2(1.0f, 0.5f));
                    }
                    AddVertex(toFill, vEnd - vYAxis * fr, lineClr, new Vector2(0.0f, 1.0f));
                    AddVertex(toFill, vEnd + vYAxis * fr, lineClr, new Vector2(1.0f, 1.0f));
                    toFill.AddTriangle(iv + 0, iv + 1, iv + 2);
                    toFill.AddTriangle(iv + 1, iv + 3, iv + 2);
                    if(r.width!=r.height)
                    {
                        iv += 2;
                        toFill.AddTriangle(iv + 0, iv + 1, iv + 2);
                        toFill.AddTriangle(iv + 1, iv + 3, iv + 2);
                        iv += 2;
                        toFill.AddTriangle(iv + 0, iv + 1, iv + 2);
                        toFill.AddTriangle(iv + 1, iv + 3, iv + 2);
                    }
                }
                else
                {
                    AddVertex(toFill, vBegin - vYAxis * fr, lineClr, new Vector2(0.0f, 1.0f));
                    AddVertex(toFill, vBegin + vYAxis * fr, lineClr, new Vector2(1.0f, 1.0f));
                    AddVertex(toFill, vEnd - vYAxis * fr, lineClr, new Vector2(0.0f, 1.0f));
                    AddVertex(toFill, vEnd + vYAxis * fr, lineClr, new Vector2(1.0f, 1.0f));
                    toFill.AddTriangle(iv + 0, iv + 1, iv + 2);
                    toFill.AddTriangle(iv + 1, iv + 3, iv + 2);
                }
                PostDrawLine(toFill, 0, vBegin, fr, vDir, vYAxis);
            }
            else
            {
                if (m_bAntiAliased == true)
                {
                    for (int i = 0; i < m_LineVecs.Count; i += 2)
                    {
                        if (m_LineVecs[i].x == m_LineVecs[i + 1].x &&
                            m_LineVecs[i].y == m_LineVecs[i + 1].y)
                        {
                            DrawAntiAliasedDot(toFill, i, r);
                        }
                        else
                        {
                            DrawAntiAliasedLine(toFill, i, r);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < m_LineVecs.Count; i += 2)
                    {

                        DrawLineWidth(toFill, i, r);
                    }
                }
            }
        }
       
        void AddVertex(VertexHelper toFill,Vector2 v,Color clr,Vector2 uv)
        {
            //Vector2 vNormal;
            //vNormal.x = Mathf.RoundToInt(v.x);
            //vNormal.y = Mathf.RoundToInt(v.y);
            toFill.AddVert(v, clr, uv);
        }
        Color32 CalcLineColor(Color32 src,float fr)
        {
            src.a /= c_iSmoothScale;
            float fd = Mathf.Sqrt(fr);
            if (m_bAntiAliased && m_bBestAntialiased)
            {
                src.a = (byte)Mathf.Min(src.a * fd, 255);
            }
            return src;
        }
        void DrawAntiAliasedDot(VertexHelper toFill, int iPos, Rect r)
        {
            Vector2 vBegin = new Vector2(m_LineVecs[iPos + 0].x * r.width + r.xMin, r.yMin + m_LineVecs[iPos + 0].y * r.height);
            float fRadius = m_LineWidths[iPos / 2] * 1.0f;
            Color32 lineClr = CalcLineColor(m_LineColors[iPos / 2],fRadius);
          
            int iv = toFill.currentVertCount;
            Vector2 vTop = vBegin - fRadius * Vector2.up;
            AddVertex(toFill,vTop - Vector2.right * fRadius, lineClr, new Vector2(0.0f, 0.0f));
            AddVertex(toFill,vTop + Vector2.right * fRadius, lineClr, new Vector2(1.0f, 0.0f));
            Vector2 vBot = vBegin + fRadius * Vector2.up;

            AddVertex(toFill,vBot - Vector2.right * fRadius, lineClr, new Vector2(0.0f,  1.0f));
            AddVertex(toFill,vBot + Vector2.right * fRadius, lineClr, new Vector2(1.0f, 1.0f));
            toFill.AddTriangle(iv + 0, iv + 1, iv + 2);
            toFill.AddTriangle(iv + 1, iv + 3, iv + 2);
            PostDrawLine(toFill, iv, vBegin, fRadius, Vector2.right,Vector2.up);
        }

        void DrawAntiAliasedLine(VertexHelper toFill, int iPos, Rect r)
        {
            Vector2 vBegin = new Vector2(m_LineVecs[iPos + 0].x * r.width + r.xMin, r.yMin + m_LineVecs[iPos + 0].y * r.height);
            Vector2 vEnd = new Vector2(m_LineVecs[iPos + 1].x * r.width + r.xMin, r.yMin + m_LineVecs[iPos + 1].y * r.height);
            float fLineLength = (vEnd - vBegin).magnitude;

            Vector2 vStartToEnd = SafeNormalLineDir(vBegin, vEnd);
            float fRadius = m_LineWidths[iPos / 2];
            Vector2 v = new Vector2(vStartToEnd.y, -vStartToEnd.x);
            if (fLineLength < fRadius * 2.0f)
            {

                Vector2 vCenter = (vBegin + vEnd) * 0.5f;
                vBegin = vCenter - v * fRadius;
                vEnd = vCenter + v * fRadius;
                v = vStartToEnd;
                vStartToEnd = new Vector2(v.y, -v.x);
                fRadius = fLineLength * 0.5f;
            }
            Color32 lineClr = CalcLineColor(m_LineColors[iPos / 2], fRadius);
            int iBeginv = toFill.currentVertCount;
            int iv = toFill.currentVertCount;
            if (m_bSequential == false || (iPos == 0))
            {
                 
                AddVertex(toFill,vBegin - v * fRadius, lineClr, new Vector2(0.0f, 0.0f));
                AddVertex(toFill,vBegin + v * fRadius, lineClr, new Vector2(1.0f, 0.0f));
                Vector2 v1 = vBegin + fRadius * vStartToEnd;
                AddVertex(toFill,v1 - v * fRadius, lineClr, new Vector2(0.0f, 0.5f));
                AddVertex(toFill,v1 + v * fRadius, lineClr, new Vector2(1.0f, 0.5f));
                toFill.AddTriangle(iv + 0, iv + 1, iv + 2);
                toFill.AddTriangle(iv + 1, iv + 3, iv + 2);
                iv += 2;
            }
            else
            {
                AddVertex(toFill,m_vLastVec1, lineClr, new Vector2(0.0f, 0.5f));
                AddVertex(toFill,m_vLastVec2, lineClr, new Vector2(1.0f, 0.5f));

            }
            float fvY = 1.0f;
            if (m_bSequential == false || (iPos + 2 == m_LineVecs.Count))
            {
                Vector2 v2 = vEnd - fRadius * vStartToEnd;
                AddVertex(toFill,v2 - v * fRadius, lineClr, new Vector2(0.0f, 0.5f));
                AddVertex(toFill,v2 + v * fRadius, lineClr, new Vector2(1.0f, 0.5f));
                toFill.AddTriangle(iv + 0, iv + 1, iv + 2);
                toFill.AddTriangle(iv + 1, iv + 3, iv + 2);
                iv += 2;
                m_vLastVec1 = vEnd - v * fRadius;
                m_vLastVec2 = vEnd + v * fRadius;
            }
            else
            {

                Vector2 vNextBegin = new Vector2(m_LineVecs[iPos + 2].x * r.width + r.xMin, r.yMin + m_LineVecs[iPos + 2].y * r.height);
                Vector2 vNextEnd = new Vector2(m_LineVecs[iPos + 3].x * r.width + r.xMin, r.yMin + m_LineVecs[iPos + 3].y * r.height);
                Vector2 vNextDir = SafeNormalLineDir(vNextBegin, vNextEnd);
                Vector2 vNextV = new Vector2(vNextDir.y, -vNextDir.x);

                Vector2 vBendV = (v + vNextV);
                vBendV.Normalize();
                float fDot = Vector2.Dot(vBendV, v);
                float fScale = 1.0f / fDot;
                m_vLastVec1 = vEnd - vBendV * fRadius * fScale;
                m_vLastVec2 = vEnd + vBendV * fRadius * fScale;
                fvY = 0.5f;


            }
            AddVertex(toFill,m_vLastVec1, lineClr, new Vector2(0.0f, fvY));
            AddVertex(toFill,m_vLastVec2, lineClr, new Vector2(1.0f, fvY));
            toFill.AddTriangle(iv + 0, iv + 1, iv + 2);
            toFill.AddTriangle(iv + 1, iv + 3, iv + 2);
            PostDrawLine(toFill, iBeginv, vBegin, fRadius, vStartToEnd, v);
        }
        Vector2 SafeNormalLineDir(Vector2 vBegin,Vector2 vEnd)
        {
            Vector2 vStartToEnd = vEnd - vBegin;
            if (vStartToEnd.x == 0 && vStartToEnd.y == 0.0f)
            {
                vStartToEnd = Vector2.up;
            }
            else
            {
                vStartToEnd.Normalize();
            }
            return vStartToEnd;
        }
        void DrawLineWidth(VertexHelper toFill,int iPos, Rect r)
        {

            Vector2 vBegin = new Vector2(m_LineVecs[iPos + 0].x * r.width + r.xMin, r.yMin + m_LineVecs[iPos + 0].y * r.height);
            Vector2 vEnd = new Vector2(m_LineVecs[iPos + 1].x * r.width + r.xMin, r.yMin + m_LineVecs[iPos + 1].y * r.height);
            Vector2 vStartToEnd = SafeNormalLineDir(vBegin,vEnd );
            int iv = toFill.currentVertCount;
            int iBeginV = iv;
            Vector2 v = new Vector2(vStartToEnd.y, -vStartToEnd.x);
            float fRadius = m_LineWidths[iPos/2]*1.0f;
            Color32 lineClr = m_LineColors[iPos / 2];
            if (m_bSequential == false || (iPos==0))
            {
                AddVertex(toFill,vBegin - v * fRadius, lineClr, new Vector2(0.0f, 0.0f));
                AddVertex(toFill,vBegin + v * fRadius, lineClr, new Vector2(1.0f, 0.0f));
            }
            else
            {
                AddVertex(toFill,m_vLastVec1, lineClr, new Vector2(0.0f, 0.0f));
                AddVertex(toFill,m_vLastVec2, lineClr, new Vector2(1.0f, 0.0f));

            }
            if (m_bSequential == false || (iPos+2==m_LineVecs.Count))
            {
                m_vLastVec1 = vEnd - v * fRadius;
                m_vLastVec2 = vEnd + v * fRadius;
            }
            else
            {
                Vector2 vNextBegin = new Vector2(m_LineVecs[iPos + 2].x * r.width + r.xMin, r.yMin + m_LineVecs[iPos + 2].y * r.height);
                Vector2 vNextEnd = new Vector2(m_LineVecs[iPos + 3].x * r.width + r.xMin, r.yMin + m_LineVecs[iPos + 3].y * r.height);
                Vector2 vNextDir = SafeNormalLineDir(vNextBegin, vNextEnd);
                Vector2 vNextV = new Vector2(vNextDir.y, -vNextDir.x);

                Vector2 vBendV = (v + vNextV);
                vBendV.Normalize();
                float fDot = Vector2.Dot(vBendV, v);
                float fScale = 1.0f / fDot;
                m_vLastVec1 = vEnd - vBendV * fRadius*fScale;
                m_vLastVec2 = vEnd + vBendV * fRadius*fScale;
            }
            AddVertex(toFill,m_vLastVec1, lineClr, new Vector2(0.0f, 1.0f));
            AddVertex(toFill,m_vLastVec2, lineClr, new Vector2(1.0f, 1.0f));

            toFill.AddTriangle(iv+0, iv+1, iv+2);
            toFill.AddTriangle(iv+1, iv+3, iv+2);
            PostDrawLine(toFill, iBeginV, vBegin, fRadius, vStartToEnd, v);
        }
        protected virtual void PostDrawLine(VertexHelper vf,int iBegin, Vector2 vBegin,float fY,Vector2 u,Vector2 v)
        {

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
