using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace BDSZ_2020
{
    [RequireComponent(typeof(CanvasRenderer))]
    [Obsolete("This overload is obsolete from current version  and will be removed.")]
    public class BDSZ_UIDrawLineEx : BDSZ_UIDrawLine
    {
        public enum TextureType
        {
            Tiled = 0,
            UseCoord0,
        }
        [FormerlySerializedAs("m_Tex")]
        [SerializeField] Texture m_Texture;
        [SerializeField] Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);
        [SerializeField] TextureType m_TextureType = TextureType.Tiled;
        protected BDSZ_UIDrawLineEx()
        {
            
        }
       
        public Texture texture
        {
            get
            {
                return m_Texture;
            }
            set
            {
                if (m_Texture == value)
                    return;
                m_Texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }
        public Rect uvRect
        {
            get
            {
                return m_UVRect;
            }
            set
            {
                if (m_UVRect == value)
                    return;
                m_UVRect = value;
                SetVerticesDirty();
            }
        }
        protected override void BuildMaterial()
        {
            m_Material = new Material(Shader.Find("UIShader/DrawLineEx"));
            if (m_bAntiAliased == false)
            {
                m_Material.mainTexture = Texture2D.whiteTexture;
                
            }
            else
            {
                if (m_bRoundEnd)
                    m_Material.mainTexture = CircleTexture;
                else
                    m_Material.mainTexture = RectTexture;
            }
            m_Material.SetTexture("_DetailTex", m_Texture);
            m_Material.SetColor("_Color", color);
        }
        protected override void PostDrawLine(VertexHelper vf, int iBegin, Vector2 vBegin, float fRadius, Vector2 u, Vector2 v)
        {
          
            float fvScale = fRadius * 2.0f;
            float fuScale = fvScale;
            if(m_Texture!=null)
            {
                fuScale = fvScale * m_Texture.width / m_Texture.height;
            }

            UIVertex vert = new UIVertex();
            if (m_TextureType == TextureType.Tiled)
            {
                float fuBegin = 0.0f;
                if (m_bSequential && iBegin > 0)
                {
                    UIVertex vLast = new UIVertex();
                    for (int i = iBegin; i < iBegin + 2; i++)
                    {
                        vf.PopulateUIVertex(ref vLast, i - 2);
                        vf.PopulateUIVertex(ref vert, i);
                        vert.uv1 = vLast.uv1;
                        vf.SetUIVertex(vert, i);
                    }

                    fuBegin = vLast.uv1.x;

                    iBegin += 2;
                }
                for (int i = iBegin; i < vf.currentVertCount; i++)
                {
                    vf.PopulateUIVertex(ref vert, i);
                    Vector2 vSub = new Vector2(vert.position.x, vert.position.y) - vBegin;
                    float fu = Vector2.Dot(vSub, u) / fuScale * m_UVRect.width + m_UVRect.xMin + fuBegin;
                    float fv = (Vector2.Dot(vSub, v) / fvScale - 0.5f) * m_UVRect.height + m_UVRect.yMin;

                    vert.uv1 = new Vector2(fu, fv);
                    vf.SetUIVertex(vert, i);
                }
            }
            else if(m_TextureType == TextureType.UseCoord0)
            {
                for (int i = iBegin; i < vf.currentVertCount; i++)
                {
                    vf.PopulateUIVertex(ref vert, i);
                    vert.uv1 = vert.uv0;
                    vf.SetUIVertex(vert, i);
                }
            }
        }

    }
}
