using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FlexLayout.Edit
{
    [CustomEditor(typeof(UIFlexContainer))]
    [CanEditMultipleObjects]
    public class UIFlexContainerEditor : Editor
    {
        private static readonly GUIContent contentDirection     = new GUIContent("Direction", "Main direction axis.");
        private static readonly GUIContent contentReversed      = new GUIContent("Reversed", "Reverse main direction.");
        private static readonly GUIContent contentWrap          = new GUIContent("Wrap", "Enables wrapping to new lines instead of shrinking items.");
        private static readonly GUIContent contentWrapReversed  = new GUIContent("Wrap Reversed", "Reverses the wrap direction.");
        private static readonly GUIContent contentJustifyContent = new GUIContent("Justify Content", "Alignment of items in main axis lines. Determines what to do with spacing when flex-grow is limited.");
        private static readonly GUIContent contentSpacingDistribution = new GUIContent("Spacing Distribution", "Custom distribution for Justify Content.");
        private static readonly GUIContent contentAlignContent  = new GUIContent("Align Content", "Behavior or Positioning of lines in the cross axis.");
        private static readonly GUIContent contentAlignItems    = new GUIContent("Align Items", "Item alignment in the cross axis within lines. Items can override their alignment with the Align-Self property.");
        private static readonly GUIContent contentGap           = new GUIContent("Minimum Gap", "Minimum required gap between items.");
        private static readonly GUIContent contentGapWidth      = new GUIContent("Width", "Minimum Gap width.");
        private static readonly GUIContent contentGapHeight     = new GUIContent("Height", "Minimum Gap height.");
        private static readonly GUIContent contentPadding       = new GUIContent("Padding", "Reduces Content Size inwards from edges.");

        private const string pathDirectionIcon = "Assets/UI Flex Layout/Icons/Direction Icon.png";

        private Texture2D directionTextureRight;
        private Texture2D directionTextureLeft;
        private Texture2D directionTextureDown;
        private Texture2D directionTextureUp;


        private new UIFlexContainer target;

        private SerializedProperty propertyDirection;
        private SerializedProperty propertyReversed;
        private SerializedProperty propertyWrap;
        private SerializedProperty propertyWrapReversed;
        private SerializedProperty propertyJustifyContent;
        private SerializedProperty propertyAlignContent;
        private SerializedProperty propertySpacingDistribution;
        private SerializedProperty propertyFitCross;
        private SerializedProperty propertyAlignItems;
        private SerializedProperty propertyMinimumGap;
        private SerializedProperty propertyPadding;

        private static bool expandPadding = false;

        private void OnEnable()
        {
            directionTextureRight   = (Texture2D)AssetDatabase.LoadAssetAtPath(pathDirectionIcon, typeof(Texture2D));
            directionTextureLeft    = TextureUtil.Flip(directionTextureRight, true);
            directionTextureDown    = TextureUtil.Rotate90(directionTextureRight, true);
            directionTextureUp      = TextureUtil.Rotate90(directionTextureRight, false);

            target = (UIFlexContainer)base.target;

            propertyDirection       = serializedObject.FindProperty(nameof(UIFlexContainer.direction));
            propertyReversed        = serializedObject.FindProperty(nameof(UIFlexContainer.reversed));
            propertyWrap            = serializedObject.FindProperty(nameof(UIFlexContainer.wrap));
            propertyWrapReversed    = serializedObject.FindProperty(nameof(UIFlexContainer.wrapReversed));

            propertyJustifyContent  = serializedObject.FindProperty(nameof(UIFlexContainer.justifyContent));
            propertySpacingDistribution = serializedObject.FindProperty(nameof(UIFlexContainer.spacingDistribution));

            propertyAlignContent    = serializedObject.FindProperty(nameof(UIFlexContainer.alignContent));
            propertyAlignItems      = serializedObject.FindProperty(nameof(UIFlexContainer.alignItems));
            propertyMinimumGap      = serializedObject.FindProperty(nameof(UIFlexContainer.minimumGap));
            propertyPadding         = serializedObject.FindProperty(nameof(UIFlexContainer.padding));
        }

        public override void OnInspectorGUI()
        {
            LayoutUtil util;

            serializedObject.Update();

            Header();

            util = new LayoutUtil(serializedObject);

            util.ResetIndent();
            util.LayoutEnum<UIFlexContainer.Direction>(propertyDirection, contentDirection);
            util.LayoutBool(propertyReversed, contentReversed);
            util.LayoutBool(propertyWrap, contentWrap);
            util.BeginDisabledGroup(!propertyWrap.boolValue);
            util.LayoutBool(propertyWrapReversed, contentWrapReversed);
            util.EndDisabledGroup();
            
            util.LayoutEnum<UIFlexContainer.JustifyContent>(propertyJustifyContent, contentJustifyContent);
            util.BeginDisabledGroup(propertyJustifyContent.enumValueIndex != (int)UIFlexContainer.JustifyContent.Custom);
            util.IncreaseIndent();
            util.LayoutSpacingWeights(propertySpacingDistribution, contentSpacingDistribution);
            util.EndDisabledGroup();

            util.Space(4f);
            util.ResetIndent();
            util.LayoutEnum<UIFlexContainer.AlignContent>(propertyAlignContent, contentAlignContent);

            util.Space(4f);
            util.ResetIndent();
            util.LayoutEnum<UIFlexItem.Alignment>(propertyAlignItems, contentAlignItems);

            util.Space(4f);
            util.ResetIndent();
            util.SetRelativeName("Content Size");
            util.Label(contentGap);
            util.IncreaseIndent();
            util.LayoutSize(propertyMinimumGap.FindPropertyRelative(nameof(UISize2D.width)), contentGapWidth);
            util.LayoutSize(propertyMinimumGap.FindPropertyRelative(nameof(UISize2D.height)), contentGapHeight);

            util.ResetIndent();
            util.SetRelativeName("Transform Size");
            util.LayoutBorderSize(propertyPadding, contentPadding, "Padding", ref expandPadding);

            util.ResetIndent();
            /*);

            if (util.Changed())
                target.MarkRebuild();
            */
            if (serializedObject.ApplyModifiedProperties())
                foreach (Object target in targets)
                    (target as UIFlexContainer).MarkRebuild();
        }

        private void Header()
        {
            GUIStyle style;
            GUIStyleState state;

            style = new GUIStyle();
            style.fontSize = 24;
            state = style.normal;
            state.textColor = EditorStyles.label.normal.textColor;
            style.normal = state;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (target.direction == UIFlexContainer.Direction.Vertical)
            {
                GUILayout.BeginVertical();
                if (!target.reversed)
                    GUILayout.Space(-2f);
                GUILayout.Box((target.reversed ? directionTextureUp : directionTextureDown));
                if (target.reversed)
                    GUILayout.Space(-2f);
                GUILayout.EndVertical();
            }
            else
                GUILayout.Space(27f);
            GUILayout.BeginVertical();
            GUILayout.Label(new GUIContent("Flex Container"), style);
            if (target.direction == UIFlexContainer.Direction.Horizontal)
            {
                GUILayout.BeginHorizontal();
                if (target.reversed)
                    GUILayout.Space(10f);
                GUILayout.Box((target.reversed ? directionTextureLeft : directionTextureRight));
                GUILayout.EndHorizontal();
                GUILayout.Space(11f);
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
        }

        
    }
}