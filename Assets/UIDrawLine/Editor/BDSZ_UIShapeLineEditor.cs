using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.UI;
using UnityEditor;
namespace BDSZ_2020
{
    /// <summary>
    /// Custom editor for UIDrawLine component
    /// </summary>
    [CustomEditor(typeof(BDSZ_UIShapeLine))]
    public class BDSZ_UIShapeLineEditor : GraphicEditor
    {

        protected SerializedProperty m_LineSpline;

        protected SerializedProperty m_iSoftness;

        private SerializedProperty m_bSequential;

        private SerializedProperty m_bRoundend;

        protected SerializedProperty m_bGradientColor;
        protected SerializedProperty m_GradientColor;

        protected SerializedProperty m_iSolidLength;
        protected SerializedProperty m_iSpaceLength;

        SerializedProperty m_Texture;
       

        protected override void OnEnable()
        {

            base.OnEnable();
            m_LineSpline = serializedObject.FindProperty("m_LineSpline");
            m_GradientColor = serializedObject.FindProperty("m_GradientColor");
            m_iSoftness = serializedObject.FindProperty("m_iSoftness");
            m_bSequential = serializedObject.FindProperty("m_bSequential");
            m_bRoundend = serializedObject.FindProperty("m_bRoundend");

            m_iSolidLength = serializedObject.FindProperty("m_iSolidLength");
            m_iSpaceLength = serializedObject.FindProperty("m_iSpaceLength");
            m_Texture = serializedObject.FindProperty("m_Texture");
 
        }

        protected void LayoutGUI()
        {
            BDSZ_UIShapeLine shapeControl = serializedObject.targetObject as BDSZ_UIShapeLine;

            EditorGUILayout.PropertyField(m_LineSpline, new GUIContent("CurveType"));
            if (m_LineSpline.intValue == 0)
            {
                EditorGUILayout.PropertyField(m_bSequential, new GUIContent("Sequential"));
            }
            DrawProperty("DrawSelf", serializedObject, "m_bDrawSelf", GUILayout.Width(95f));
            EditorGUILayout.PropertyField(m_iSoftness, new GUIContent("Softness"));
            EditorGUILayout.PropertyField(m_bRoundend, new GUIContent("RoundEnd"));
            DrawProperty("Dashed", serializedObject, "m_bDashed", GUILayout.Width(95f));
            if (shapeControl.isDashed)
            {
                EditorGUILayout.PropertyField(m_iSolidLength, new GUIContent("SolidLength"));
                EditorGUILayout.PropertyField(m_iSpaceLength, new GUIContent("SpaceLength"));
            }

            DrawProperty("Gradient", serializedObject, "m_bGradientColor", GUILayout.Width(95f));
            if (shapeControl.isGradientColor)
            {
                EditorGUILayout.PropertyField(m_GradientColor, new GUIContent("Gradient"));
            }

            
            EditorGUILayout.PropertyField(m_Texture);
            serializedObject.ApplyModifiedProperties();
           

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();


            if (DrawHeader("ShapeLine"))
            {
                BeginContents();
                LayoutGUI();
                EndContents();
            }

            // AppearanceControlsGUI();

            serializedObject.ApplyModifiedProperties();
        }
        public bool DrawHeader(string text) { return DrawHeader(text, text, false, false); }
        public bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
        {
            bool state = EditorPrefs.GetBool(key, true);

            if (!minimalistic) GUILayout.Space(3f);
            if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUI.changed = false;

            if (minimalistic)
            {
                if (state) text = "\u25BC" + (char)0x200a + text;
                else text = "\u25BA" + (char)0x200a + text;

                GUILayout.BeginHorizontal();
                GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
                if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
                GUI.contentColor = Color.white;
                GUILayout.EndHorizontal();
            }
            else
            {
                text = "<b><size=11>" + text + "</size></b>";
                if (state) text = "\u25BC " + text;
                else text = "\u25BA " + text;
                if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
            }

            if (GUI.changed) EditorPrefs.SetBool(key, state);

            if (!minimalistic) GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if (!forceOn && !state) GUILayout.Space(3f);
            return state;
        }

        public void BeginContents() { BeginContents(false); }

        static bool mEndHorizontal = false;


        public void BeginContents(bool minimalistic)
        {
            if (!minimalistic)
            {
                mEndHorizontal = true;
                GUILayout.BeginHorizontal();

                //EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
                EditorGUILayout.BeginHorizontal("TabWindowBackground", GUILayout.MinHeight(10f));
            }
            else
            {
                mEndHorizontal = false;
                EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
                GUILayout.Space(10f);
            }
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
        }

        public void EndContents()
        {
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (mEndHorizontal)
            {
                GUILayout.Space(3f);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(3f);
        }
        public SerializedProperty DrawProperty(string label, SerializedObject serializedObject, string property, params GUILayoutOption[] options)
        {
            SerializedProperty sp = serializedObject.FindProperty(property);

            if (sp != null)
            {


                if (label != null) EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
                else EditorGUILayout.PropertyField(sp, options);

            }
            return sp;
        }
    }
}
