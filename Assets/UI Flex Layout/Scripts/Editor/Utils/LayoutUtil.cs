using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FlexLayout.Edit
{
    public class LayoutUtil
    {
        private static readonly GUIContent contentBorderUniform = new GUIContent("Uniform", "applied from all edges.");
        private static readonly GUIContent contentBorderTop = new GUIContent("Top", "applied from top edge.");
        private static readonly GUIContent contentBorderRight = new GUIContent("Right", "applied from right edge.");
        private static readonly GUIContent contentBorderBottom = new GUIContent("Bottom", "applied from bottom edge.");
        private static readonly GUIContent contentBorderLeft = new GUIContent("Left", "applied from left edge.");

        private static readonly Color drivenColorText   = new Color32(140, 170, 255,255);
        private static readonly Color drivenColorBG     = new Color32(220, 220, 255,255);

        private static readonly string imagePathLink = "Assets/UI Flex Layout/Icons/flexlayout_inspector_icon_linker_link.png";
        private static readonly string imagePathLinkInvalid = "Assets/UI Flex Layout/Icons/flexlayout_inspector_icon_linker_link_invalid.png";
        private Texture2D linkImage;
        private Texture2D linkInvalidImage;

        private Color backgroundColor;
        private SerializedObject target;
        private int indentLevel = 0;
        private string relativeName = "";
        private bool drivenField = false;
        private bool drivenValid = false;

        public LayoutUtil(SerializedObject target)
        {
            this.target = target;

            backgroundColor = GUI.backgroundColor;
            linkImage        = (Texture2D)AssetDatabase.LoadAssetAtPath(imagePathLink, typeof(Texture2D));
            linkInvalidImage = (Texture2D)AssetDatabase.LoadAssetAtPath(imagePathLinkInvalid, typeof(Texture2D));
        }

        public void ResetIndent()
        {
            indentLevel = 0;
            EditorGUI.indentLevel = 0;
        }

        public void IncreaseIndent()
        {
            indentLevel++;
        }

        public void DecreaseIndent()
        {
            indentLevel = Mathf.Max(0, indentLevel - 1);
        }

        public void SetFieldDriven(bool driven, bool valid)
        {
            drivenField = driven;
            drivenValid = valid;
        }

        public void UnsetFieldDriven()
        {
            drivenField = false;
        }

        public void SetBackgroundOverlayColor(Color color)
        {
            GUI.backgroundColor = color;
        }

        public void ResetBackgroundColor()
        {
            GUI.backgroundColor = backgroundColor;
        }

        public void SetRelativeName(string relativeName)
        {
            this.relativeName = relativeName;
        }

        public void BeginDisabledGroup(bool disabled)
        {
            EditorGUI.BeginDisabledGroup(disabled);
        }

        public void EndDisabledGroup()
        {
            EditorGUI.EndDisabledGroup();
        }

        public void Label(GUIContent content)
        {
            EditorGUI.indentLevel = indentLevel;
            GUILayout.Label(content);
        }

        public bool FoldoutLabel(bool foldout, GUIContent content)
        {
            EditorGUI.indentLevel = indentLevel;
            return (EditorGUILayout.Foldout(foldout, content, true));
        }

        public void Space(float space)
        {
            GUILayout.Space(space);
        }

        public void LayoutEnum<E>(SerializedProperty property, GUIContent content) where E : System.Enum
        {
            E value;

            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel = indentLevel;
            EditorGUILayout.PrefixLabel(content);
            EditorGUI.indentLevel = 0;
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            value = (E)EditorGUILayout.EnumPopup((E)(object)property.enumValueIndex, GUILayout.MaxWidth(150f));
            EditorGUI.showMixedValue = false;

            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                property.enumValueIndex = (int)(object)value;
        }

        public void LayoutFloat(SerializedProperty property, GUIContent content)
        {
            float value;

            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel = indentLevel;
            EditorGUILayout.PrefixLabel(content);
            EditorGUI.indentLevel = 0;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.FloatField(property.floatValue);
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                property.floatValue = value;
        }

        public void LayoutInt(SerializedProperty property, GUIContent content)
        {
            int value;

            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel = indentLevel;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PrefixLabel(content);
            EditorGUI.indentLevel = 0;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            value = EditorGUILayout.IntField(property.intValue);
            EditorGUI.showMixedValue = false;
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                property.intValue = value;
        }

        public void LayoutBool(SerializedProperty property, GUIContent content)
        {
            bool value;

            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel = indentLevel;
            EditorGUILayout.PrefixLabel(content);
            EditorGUI.indentLevel = 0;
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            value = EditorGUILayout.Toggle(property.boolValue);
            EditorGUI.showMixedValue = false;
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                property.boolValue = value;
        }

        public void LayoutSpacingWeights(SerializedProperty property, GUIContent content)
        {
            float labelWidth;
            float value;
            SerializedProperty propX;
            SerializedProperty propY;
            SerializedProperty propZ;

            propX = property.FindPropertyRelative(nameof(Vector3.x));
            propY = property.FindPropertyRelative(nameof(Vector3.y));
            propZ = property.FindPropertyRelative(nameof(Vector3.z));

            labelWidth = EditorGUIUtility.labelWidth;

            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel = indentLevel;
            EditorGUILayout.PrefixLabel(content);
            EditorGUI.indentLevel = 0;

            EditorGUIUtility.labelWidth = 30f;
            content = new GUIContent("Start", "");
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = propX.hasMultipleDifferentValues;
            value = EditorGUILayout.FloatField(content, propX.floatValue, GUILayout.MaxWidth(70f));
            if (EditorGUI.EndChangeCheck())
                propX.floatValue = value;

            EditorGUIUtility.labelWidth = 25f;
            content = new GUIContent("Gaps", "");
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = propY.hasMultipleDifferentValues;
            value = EditorGUILayout.FloatField(content, propY.floatValue, GUILayout.MaxWidth(65f));
            if (EditorGUI.EndChangeCheck())
                propY.floatValue = value;

            EditorGUIUtility.labelWidth = 25f;
            content = new GUIContent("End", "");
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = propZ.hasMultipleDifferentValues;
            value = EditorGUILayout.FloatField(content, propZ.floatValue, GUILayout.MaxWidth(65f));
            if (EditorGUI.EndChangeCheck())
                propZ.floatValue = value;

            EditorGUI.showMixedValue = false;
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = labelWidth;
        }

        public void LayoutSize(SerializedProperty property, GUIContent content, SerializedProperty activate = null, float preSpace = 0f)
        {
            SerializedProperty percentage;
            SerializedProperty pixels;
            GUIStyle style;
            string percentageNotice;
            float labelWidth;
            bool active;
            float value;

            percentage  = property.FindPropertyRelative(nameof(UISize.percentage));
            pixels      = property.FindPropertyRelative(nameof(UISize.pixels));
            percentageNotice = $"Percentage relative to {relativeName}.";
            labelWidth = EditorGUIUtility.labelWidth;
            if (activate != null)
                preSpace -= 22f;

            EditorGUI.indentLevel = indentLevel;

            content = new GUIContent(content.text, $"{content.tooltip} {percentageNotice}");
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(content);

            EditorGUI.indentLevel = 0;
            if (preSpace != 0f)
                GUILayout.Space(preSpace);

            active = true;
            if (activate != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = activate.hasMultipleDifferentValues;
                active = EditorGUILayout.Toggle(activate.boolValue, GUILayout.Width(20f));
                if (EditorGUI.EndChangeCheck())
                    activate.boolValue = active;
            }
            BeginDisabledGroup(!active);

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = percentage.hasMultipleDifferentValues;
            EditorGUIUtility.labelWidth = 15f;
            content = new GUIContent("%", percentageNotice);
            value = EditorGUILayout.FloatField(content, percentage.floatValue, GUILayout.MaxWidth(80f));
            if (EditorGUI.EndChangeCheck())
                percentage.floatValue = value;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = pixels.hasMultipleDifferentValues;
            EditorGUIUtility.labelWidth = 20f;

            if (drivenField && drivenValid)
            {
                style = CreateDrivenFieldStyle();
                SetBackgroundOverlayColor(drivenColorBG);
                content = new GUIContent("px", "Constant in pixels.");
                value = EditorGUILayout.FloatField(content, pixels.floatValue, style, GUILayout.MaxWidth(80f));
                ResetBackgroundColor();
            }
            else
            {
                content = new GUIContent("px", "Constant in pixels.");
                value = EditorGUILayout.FloatField(content, pixels.floatValue, GUILayout.MaxWidth(80f));
            }
            if (drivenField)
            {
                SetBackgroundOverlayColor(new Color32(0, 0, 0, 0));
                GUILayout.BeginVertical();
                GUILayout.Box((drivenValid ? linkImage : linkInvalidImage));
                Space(-3f);
                GUILayout.EndVertical();
                ResetBackgroundColor();
            }

            if (EditorGUI.EndChangeCheck())
                pixels.floatValue = value;

            EndDisabledGroup();
            GUILayout.EndHorizontal();
            EditorGUI.showMixedValue = false;
            EditorGUIUtility.labelWidth = labelWidth;
        }
        
        public void LayoutBorderSize(SerializedProperty property, GUIContent content, string name, ref bool foldout)
        {
            GUIContent edgeContent;
            string baseRelativeName;

            foldout = FoldoutLabel(foldout, content);
            if (!foldout)
                return;

            IncreaseIndent();
            edgeContent = new GUIContent(contentBorderUniform.text, $"{name} {contentBorderUniform.tooltip}");
            LayoutSize(property.FindPropertyRelative(nameof(UIBorder.uniform)), edgeContent);

            Space(3f);
            IncreaseIndent();
            baseRelativeName = this.relativeName;

            edgeContent = new GUIContent(contentBorderTop.text, $"{name} {contentBorderTop.tooltip}");
            SetRelativeName($"{baseRelativeName} Height");
            LayoutSize(property.FindPropertyRelative(nameof(UIBorder.top)), edgeContent, null, 12f);

            edgeContent = new GUIContent(contentBorderRight.text, $"{name} {contentBorderRight.tooltip}");
            SetRelativeName($"{baseRelativeName} Width");
            LayoutSize(property.FindPropertyRelative(nameof(UIBorder.right)), edgeContent, null, 12f);

            edgeContent = new GUIContent(contentBorderBottom.text, $"{name} {contentBorderBottom.tooltip}");
            SetRelativeName($"{baseRelativeName} Height");
            LayoutSize(property.FindPropertyRelative(nameof(UIBorder.bottom)), edgeContent, null, 12f);

            edgeContent = new GUIContent(contentBorderLeft.text, $"{name} {contentBorderLeft.tooltip}");
            SetRelativeName($"{baseRelativeName} Width");
            LayoutSize(property.FindPropertyRelative(nameof(UIBorder.left)), edgeContent, null, 12f);

            SetRelativeName(baseRelativeName);
        }

        private GUIStyle CreateDrivenFieldStyle()
        {
            GUIStyleState state;
            GUIStyle style;

            style = new GUIStyle(EditorStyles.numberField);
            state = style.normal;
            state.textColor = drivenColorText;
            style.normal = state;

            return (style);
        }
    }
}
