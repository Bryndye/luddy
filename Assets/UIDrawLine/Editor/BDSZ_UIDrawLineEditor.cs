using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using BDSZ_2020;
namespace BDSZ_2020
{
    /// <summary>
    /// Custom editor for UIDrawLine component
    /// </summary>
    [CustomEditor(typeof(BDSZ_UIDrawLine))]
    public class BDSZ_UIDrawLineEditor : GraphicEditor
    {

        private SerializedProperty m_bAntiAliased;
        private SerializedProperty m_bSequential;
        private SerializedProperty m_bBestAntialiased;
        private SerializedProperty m_bRoundEnd;

        private SerializedProperty m_bStencilMask;
        private SerializedProperty m_iStencilComparison;
        private SerializedProperty m_iStencilID;
        private SerializedProperty m_iStencilOperation;

        

        protected override void OnEnable()
        {
            
            base.OnEnable();

            m_bAntiAliased = serializedObject.FindProperty("m_bAntiAliased");
            m_bSequential = serializedObject.FindProperty("m_bSequential");
            m_bBestAntialiased = serializedObject.FindProperty("m_bBestAntialiased");
            m_bRoundEnd = serializedObject.FindProperty("m_bRoundEnd");

            m_bStencilMask = serializedObject.FindProperty("m_bStencilMask");
            m_iStencilComparison = serializedObject.FindProperty("m_iStencilComparison");
            m_iStencilID = serializedObject.FindProperty("m_iStencilID");
            m_iStencilOperation = serializedObject.FindProperty("m_iStencilOperation");
        }

        protected void LinePropertyField()
        {
            EditorGUILayout.PropertyField(m_bAntiAliased, new GUIContent("AntiAliased"));
            EditorGUILayout.PropertyField(m_bSequential, new GUIContent("Sequential"));
            if (m_bAntiAliased.boolValue == true)
            {
                EditorGUILayout.PropertyField(m_bBestAntialiased, new GUIContent("Best Anti-aliasing"));
                EditorGUILayout.PropertyField(m_bRoundEnd, new GUIContent("RoundEnd"));
            }
            EditorGUILayout.PropertyField(m_bStencilMask, new GUIContent("StencilMask"));
            if(m_bStencilMask.boolValue ==true)
            {
                EditorGUILayout.PropertyField(m_iStencilComparison, new GUIContent("Stencil Comparison"));
                EditorGUILayout.PropertyField(m_iStencilID, new GUIContent("Stencil ID"));
                EditorGUILayout.PropertyField(m_iStencilOperation, new GUIContent("Stencil Operation"));

            }

        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            LinePropertyField();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
