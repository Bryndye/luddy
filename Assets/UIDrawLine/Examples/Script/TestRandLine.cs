 
using UnityEngine;
using UnityEngine.UI;

namespace BDSZ_2020
{
    [ExecuteInEditMode]
    public class TestRandLine : MonoBehaviour
    {
        // Start is called before the first frame update

        BDSZ_UIShapeLine m_UIDrawLine = null;

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
                fLineWidth = UnityEngine.Random.Range(3.0f, 15.0f);
                Color clr = Random.ColorHSV();
                m_UIDrawLine.AddLineSegment(vBegin, vEnd, fLineWidth, clr);


            }
            m_UIDrawLine.SetAllDirty();

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}