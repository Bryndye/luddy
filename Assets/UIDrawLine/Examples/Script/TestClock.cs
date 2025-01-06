 
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BDSZ_2020
{
    [ExecuteAlways]
    public class TestClock : MonoBehaviour
    {
        // Start is called before the first frame update
        const float c_fMaxRadius = 0.5f;
        const float c_fMinRadius = 0.45f;
        BDSZ_UIShapeLine m_ClockPointer = null;
        BDSZ_UIShapeLine m_Axis = null;
        BDSZ_UIShapeLine m_Background = null;
        Text m_TextTime = null;

        MaskableGraphic FindUIComponent(string strName)
        {
            GameObject go = GameObject.Find(strName);
            if (go == null)
                return null;
            MaskableGraphic line = go.GetComponent<MaskableGraphic>();

            return line;
        }

        void UpdateShowInfo()
        {
            string strTime = DateTime.Now.ToLongTimeString();
            m_TextTime.text = strTime;

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
        void Start()
        {
            m_ClockPointer = FindUIComponent("Pointer") as BDSZ_UIShapeLine;
            m_Axis = FindUIComponent("Axis") as BDSZ_UIShapeLine;
            m_Background = FindUIComponent("Circle") as BDSZ_UIShapeLine;
            m_TextTime = FindUIText("Time", 32, Color.white, new Vector2(240.0f, 44.0f), new Vector2(0.0f, -30.0f)) as Text;
            m_TextTime.alignment = TextAnchor.MiddleCenter;

            Vector2 vBegin;
            Vector2 vEnd;

            for (int i = 0; i < 60; i++)
            {
                float fAngle = i * 6 * Mathf.Deg2Rad;
                vBegin.x = Mathf.Sin(fAngle) * c_fMaxRadius + 0.5f;
                vBegin.y = Mathf.Cos(fAngle) * c_fMaxRadius + 0.5f;
                vEnd.x = Mathf.Sin(fAngle) * c_fMinRadius + 0.5f;
                vEnd.y = Mathf.Cos(fAngle) * c_fMinRadius + 0.5f;
                float fWidth = (i % 5 == 0) ? 7.0f : 5.0f;
                Color32 lineColor = (i % 5 == 0) ? new Color32(0, 0, 0, 255) : new Color32(192, 192, 192, 255);
                m_Axis.AddLineSegment(vBegin, vEnd, fWidth, lineColor);

            }
            m_Axis.SetVerticesDirty();

            Rect rc = m_ClockPointer.GetPixelAdjustedRect();


            for (int i = 1; i <= 12; i++)
            {
                string strName = string.Format("Number{0}", i);
                Text numberText = FindUIText(strName, 32, Color.black, new Vector2(44.0f, 44.0f), new Vector2(200.0f, 0.0f)) as Text;
                numberText.text = string.Format("{0}", i);
                Rect rcText = numberText.GetPixelAdjustedRect();
                float fNumberR = c_fMinRadius * rc.width - rcText.width * 0.5f;

                float fAngle = i * 30 * Mathf.Deg2Rad;
                vBegin.x = Mathf.Sin(fAngle) * fNumberR;
                vBegin.y = Mathf.Cos(fAngle) * fNumberR;
                numberText.rectTransform.anchoredPosition = new Vector3(vBegin.x, vBegin.y, 0.0f);

            }


        }
        void DrawClockPointer(float fAngle, float fWidth, float fFront, float fBack, Color clr)
        {
            Vector2 vBegin;
            Vector2 vEnd;
            vBegin.x = 0.5f + Mathf.Sin(fAngle) * fFront;
            vBegin.y = 0.5f + Mathf.Cos(fAngle) * fFront;
            vEnd.x = 0.5f - Mathf.Sin(fAngle) * fBack;
            vEnd.y = 0.5f - Mathf.Cos(fAngle) * fBack;

            m_ClockPointer.AddLineSegment(vBegin, vEnd, fWidth, clr);

        }
        // Update is called once per frame
        void Update()
        {


            UpdateShowInfo();

            m_ClockPointer.Clean();

            float fSecondAngle = (DateTime.Now.Second * 1000 + DateTime.Now.Millisecond) * 0.001f * 6 * Mathf.Deg2Rad;

            float fMinuteAngle = DateTime.Now.Minute * 6 * Mathf.Deg2Rad + fSecondAngle / 60.0f;
            int iHour = DateTime.Now.Hour % 12;
            float fHourAngle = iHour * 30.0f * Mathf.Deg2Rad + fMinuteAngle / 12.0f;

            DrawClockPointer(fHourAngle, 9.0f, 0.3f, 0.05f, Color.black);
            DrawClockPointer(fMinuteAngle, 7.0f, 0.35f, 0.05f, Color.black);
            // DrawClockPointer(fSecondAngle, 2.0f, 0.42f, 0.1f,new Color(1.0f,0.5f,0.0f,1.0f));
            DrawClockPointer(fSecondAngle, 5.0f, 0.42f, 0.1f, new Color(1.0f, 0.5f, 0.0f, 1.0f));
            m_ClockPointer.SetVerticesDirty();
        }
    }
}