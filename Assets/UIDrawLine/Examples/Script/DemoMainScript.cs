using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
namespace BDSZ_2020
{
    [ExecuteAlways]
    public class DemoMainScript : MonoBehaviour
    {
        // Start is called before the first frame update
        List<BDSZ_UIShapeLine> m_DrawLines = new List<BDSZ_UIShapeLine>();
        Color[] m_LineClrs = { Color.white, Color.green, Color.yellow, Color.cyan };


        MaskableGraphic FindUIComponent(string strName)
        {
            GameObject go = GameObject.Find(strName);
            if (go == null)
                return null;
            MaskableGraphic line = go.GetComponent<BDSZ_UIShapeLine>();
            return line;
        }


        // FontEngine.


        void Start()
        {
            m_DrawLines.Add(FindUIComponent("ShapeLine") as BDSZ_UIShapeLine);
            m_DrawLines.Add(FindUIComponent("AntiShapeLine") as BDSZ_UIShapeLine);
            m_DrawLines.Add(FindUIComponent("GradientLine") as BDSZ_UIShapeLine);
            m_DrawLines.Add(FindUIComponent("RoundEndAntiDrawLine") as BDSZ_UIShapeLine);
            m_DrawLines.Add(FindUIComponent("DotLine") as BDSZ_UIShapeLine);
            m_DrawLines.Add(FindUIComponent("TextureLine") as BDSZ_UIShapeLine);
            Rect vb = m_DrawLines[0].GetPixelAdjustedRect();


            float fLeftSpace = 0.05f;
            float fRightSpace = 0.05f;
            float fTopSpace = 0.1f;
            float fBotSpace = 0.02f;
            float fSepSpace = 0.02f;

            int iCount = 4;
            float fBeginWidth = 5.0f;
            float fWidthStep = 11.0f;


            float fHorStep = (1.0f - (fLeftSpace + fRightSpace)) / iCount;
            float fVerStep = (1.0f - (fTopSpace + fBotSpace)) / m_DrawLines.Count;
            float fHorLength = fHorStep - fSepSpace;
            float fVerLength = fVerStep - fSepSpace;
            Vector2 vExtent = Vector2.zero;
            Vector2 vCenter = Vector2.zero;
            for (int i = 0; i < iCount; i++)
            {
                vCenter.x = fLeftSpace + fHorStep * i + fHorLength * 0.5f;
                vCenter.y = 1.0f - (fTopSpace + fVerLength * 0.5f);
                if (i >= 1)
                    vCenter.x -= fHorLength * 0.5f;
                if (i == iCount - 1)
                {
                    vCenter.x += 0.25f * fHorLength;
                    fHorLength += 0.25f * fHorLength;
                }
                float fAngle = i * 90.0f * Mathf.Deg2Rad / (iCount - 1.0f);
                vExtent.x = Mathf.Sin(fAngle) * fHorLength * 0.5f;
                vExtent.y = Mathf.Cos(fAngle) * fVerLength * 0.5f;
                for (int it = 0; it < m_DrawLines.Count; it++)
                {
                    m_DrawLines[it].AddLineSegment(vCenter - vExtent, vCenter + vExtent, fBeginWidth + i * fWidthStep, m_LineClrs[i % m_LineClrs.Length]);
                    if (i == 0)
                    {
                        Text[] texts = m_DrawLines[it].GetComponentsInChildren<Text>();
                        for (int ic = 0; ic < texts.Length; ic++)
                        {
                            Vector3 vPos = texts[ic].rectTransform.anchoredPosition;
                            vPos.y = vCenter.y * vb.height + vb.yMin;
                            texts[ic].rectTransform.anchoredPosition = vPos;
                        }

                    }
                    vCenter.y -= fVerStep;
                }
            }
            for (int i = 0; i < m_DrawLines.Count; i++)
            {
                m_DrawLines[i].SetAllDirty();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
