 
using UnityEngine;
using UnityEngine.UI;

namespace BDSZ_2020
{
    [ExecuteInEditMode]
    public class DemoMaskClip : MonoBehaviour
    {
        BDSZ_UIShapeLine m_UIDrawLine = null;
        RawImage m_StencilMask = null;
        Vector2 m_vStencilOrginSize;

        MaskableGraphic FindUIComponent(string strName)
        {
            GameObject go = GameObject.Find(strName);
            if (go == null)
                return null;
            MaskableGraphic line = go.GetComponent<MaskableGraphic>();
            return line;
        }

        void Start()
        {
            m_UIDrawLine = FindUIComponent("RandLine") as BDSZ_UIShapeLine;
            m_StencilMask = FindUIComponent("Mask") as RawImage;
            m_vStencilOrginSize = m_StencilMask.rectTransform.sizeDelta;
            Vector2 vBegin = new Vector2(0.2f, 0.2f);
            Vector2 vEnd = new Vector2(0.8f, 0.8f);
            float fLineWidth = 40.0f;
            //  m_UIDrawLine.AddLineSegment(vBegin, vEnd, fLineWidth, Color.white);
            // m_UISDFLine.AddLineSegment(vBegin, vEnd, fLineWidth, Color.white);
            UnityEngine.Random.InitState(2020629);
            for (int i = 0; i < 50; i++)
            {

                vBegin.x = UnityEngine.Random.Range(0.0f, 1.0f);
                vBegin.y = UnityEngine.Random.Range(0.0f, 1.0f);
                vEnd.x = UnityEngine.Random.Range(0.0f, 1.0f);
                vEnd.y = UnityEngine.Random.Range(0.0f, 1.0f);
                fLineWidth = UnityEngine.Random.Range(1.0f, 10.0f);
                Color clr = Random.ColorHSV();
                m_UIDrawLine.AddLineSegment(vBegin, vEnd, fLineWidth, clr);


            }
            m_UIDrawLine.SetAllDirty();

        }

        // Update is called once per frame
        void Update()
        {
            float fScale = Mathf.Sin(Time.time * 5.0f) * 0.5f + 1.0f;
            Vector2 vNewSize = m_vStencilOrginSize * fScale;
            m_StencilMask.rectTransform.sizeDelta = vNewSize;
            Rect rcClip = m_StencilMask.GetPixelAdjustedRect();
            m_UIDrawLine.EnableRectClipping(rcClip);

        }
    }
}