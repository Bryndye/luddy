using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FlexLayout.Edit
{
    [CustomEditor(typeof(UIFlexLinker))]
    [CanEditMultipleObjects]
    public class UIFlexLinkerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, new string[] { "m_Script" });
            serializedObject.ApplyModifiedProperties();
        }
    }
}
