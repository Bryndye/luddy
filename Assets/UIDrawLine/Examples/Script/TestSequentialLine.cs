 
using UnityEngine;
using UnityEngine.UI;

namespace BDSZ_2020
{
    [ExecuteInEditMode]
    public class TestSequentialLine : MonoBehaviour
    {
        // Start is called before the first frame update

        BDSZ_UIShapeLine m_SequentialLines = null;
        BDSZ_UIShapeLine m_CurveLines = null;
        BDSZ_UIShapeLine m_Axis = null;
        Text m_TextEnglish = null;
        Text m_TextChinese = null;
        MaskableGraphic FindUIComponent(string strName)
        {
            GameObject go = GameObject.Find(strName);
            if (go == null)
                return null;
            MaskableGraphic line = go.GetComponent<MaskableGraphic>();
            return line;
        }
        Text FindUIText(string strName, int iFontSize, Color textClr, Vector2 vSize, Vector2 vAnchoredPos)
        {
            GameObject go = GameObject.Find(strName);
            if (go == null)
                return null;
            Text tx = go.GetComponent<Text>();
            if (tx == null)
            {
                tx = go.AddComponent<Text>();
                tx.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                tx.fontSize = iFontSize;
                tx.color = textClr;
                tx.rectTransform.sizeDelta = vSize;
                tx.rectTransform.anchoredPosition = vAnchoredPos;
            }
            return tx;
        }
        void UpdateShowInfo()
        {
            string strEnglish = string.Empty;
            string strChinese = string.Empty;
            if (m_SequentialLines.softness > 0)
            {
                strEnglish += "AntiAliased ";
                strChinese += "抗锯齿 ";
            }
            if (m_SequentialLines.sequential)
            {
                strEnglish += " Sequential ";
                strChinese += "连续的 ";
            }
            m_TextChinese.text = strChinese;
            m_TextEnglish.text = strEnglish;
        }
        void Start()
        {
            m_SequentialLines = FindUIComponent("SequentialLines") as BDSZ_UIShapeLine;
            m_CurveLines = FindUIComponent("CurveLines") as BDSZ_UIShapeLine;
            m_Axis = FindUIComponent("Axis") as BDSZ_UIShapeLine;
            m_TextEnglish = FindUIText("TextEnglish", 24, new Color(1.0f, 168.0f / 255.0f, 0.0f, 1.0f), new Vector2(240.0f, 30.0f), new Vector2(0.0f, -40.0f)) as Text;
            m_TextChinese = FindUIText("TextChinese", 24, new Color(1.0f, 168.0f / 255.0f, 0.0f, 1.0f), new Vector2(240.0f, 30.0f), new Vector2(0.0f, -80.0f)) as Text;
            m_TextChinese.alignment = TextAnchor.MiddleCenter;
            m_TextEnglish.alignment = TextAnchor.MiddleCenter;


            Vector2 vBegin = new Vector2(0.2f, 0.2f);
            Vector2 vEnd = new Vector2(0.8f, 0.8f);

            //   m_DrawLine.AddLineSegment(vBegin, vEnd, fLineWidth, Color.white);

            m_Axis.AddLineSegment(new Vector2(0.1f, 0.0f), new Vector2(0.1f, 1.0f), 2.0f, Color.white);
            m_Axis.AddLineSegment(new Vector2(0.0f, 0.1f), new Vector2(1.0f, 0.1f), 2, Color.white);

            UnityEngine.Random.InitState(2020629);
            vBegin = new Vector2(0.1f, 0.5f);
            int iStep = 20;
            float fXStep = 0.8f / iStep;
            float fLineWidth = 7.0f;
            float fyDamping = 0.2f;
            if (m_SequentialLines != null)
            {
                for (int i = 0; i < iStep; i++)
                {

                    vEnd.x = vBegin.x + fXStep;
                    float fYRand = Random.Range(-fyDamping, fyDamping);
                    vEnd.y = vBegin.y + fYRand;
                    if (vEnd.y < 0.1f || vEnd.y >= 0.9f)
                        vEnd.y = vBegin.y - fYRand;
                    if (vEnd.y > vBegin.y)
                    {
                        m_SequentialLines.AddLineSegment(vBegin, vEnd, fLineWidth, Color.red);
                    }
                    else
                    {
                        m_SequentialLines.AddLineSegment(vBegin, vEnd, fLineWidth, Color.green);
                    }
                    vBegin = vEnd;
                }
                m_SequentialLines.SetAllDirty();
            }

            if (m_CurveLines != null)
            {
                vBegin = new Vector2(0.1f, 0.5f);
                fyDamping = 0.1f;
                for (int i = 0; i < iStep; i++)
                {

                    vEnd.x = vBegin.x + fXStep;
                    float fYRand = Random.Range(-fyDamping, fyDamping);
                    vEnd.y = vBegin.y + fYRand;
                    if (vEnd.y < 0.1f || vEnd.y >= 0.9f)
                        vEnd.y = vBegin.y - fYRand;
                    if (vEnd.y > vBegin.y)
                    {
                        m_CurveLines.AddLineSegment(vBegin, vEnd, fLineWidth, Color.red);
                    }
                    else
                    {
                        m_CurveLines.AddLineSegment(vBegin, vEnd, fLineWidth, Color.green);
                    }
                    vBegin = vEnd;
                }
                m_CurveLines.SetAllDirty();
            }


        }
        private void Update()
        {
            UpdateShowInfo();
        }
        // Update is called once per frame
    }
}