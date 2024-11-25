using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FlexLayout.Edit
{
    [CustomEditor(typeof(UIFlexItem))]
    [CanEditMultipleObjects]
    public class UIFlexItemEditor : Editor
    {
        private static readonly GUIContent contentOrder     = new GUIContent("Order", "Order of items within the container. Hierarchy order is maintained with equal order value.");
        private static readonly GUIContent contentGrow      = new GUIContent("Grow", "Growth factor with respect to other items. Growth starts when there is more space than the Flex-Basis sizes in a line.");
        private static readonly GUIContent contentShrink    = new GUIContent("Shrink", "Shrink factor. Basis size is factored in to the final shrink factor. Shrinking starts when there is less space than the Flex-Basis sizes in a line.");
        private static readonly GUIContent contentBasis     = new GUIContent("Flex Basis", "Base size. Determines when items should shrink or grow in the container.");
        private static readonly GUIContent contentBasisWidth    = new GUIContent("Width", "Base size width.");
        private static readonly GUIContent contentBasisHeight   = new GUIContent("Height", "Base size height.");
        private static readonly GUIContent contentMinimum       = new GUIContent("Minimum Size", "Minimum size.");
        private static readonly GUIContent contentMinimumWidth  = new GUIContent("Width", "Minimum size width.");
        private static readonly GUIContent contentMinimumHeight = new GUIContent("Height", "Minimum size height.");
        private static readonly GUIContent contentMaximum       = new GUIContent("Maximum Size", "Maximum size. Minimum size takes precedence when it is larger.");
        private static readonly GUIContent contentMaximumWidth  = new GUIContent("Width", "Maximum size width.");
        private static readonly GUIContent contentMaximumHeight = new GUIContent("Height", "Maximum size height.");
        private static readonly GUIContent contentAlignSelf     = new GUIContent("Align Self", "Override the containers Align-Items property.");
        private static readonly GUIContent contentAlignSelfField = new GUIContent("Alignment", "Self alignment value.");
        private static readonly GUIContent contentMargin        = new GUIContent("Margin", "Required spacing around this item.");

        private enum SizeField
        {
            Basis,
            Minimum,
            Maximum
        }

        private SerializedProperty propertyOrder;
        private SerializedProperty propertyGrow;
        private SerializedProperty propertyShrink;

        private SerializedProperty propertyBasis;
        private SerializedProperty propertyMinimum;
        private SerializedProperty propertyMinimumEnabled;
        private SerializedProperty propertyMaximum;
        private SerializedProperty propertyMaximumEnabled;

        private SerializedProperty propertyAlignSelf;
        private SerializedProperty propertyAlignSelfField;

        private static bool foldoutMargin = false;
        private SerializedProperty propertyMargin;

        private void OnEnable()
        {
            
            propertyOrder   = serializedObject.FindProperty(nameof(UIFlexItem.order));
            propertyGrow    = serializedObject.FindProperty(nameof(UIFlexItem.flexGrow));
            propertyShrink  = serializedObject.FindProperty(nameof(UIFlexItem.flexShrink));
            propertyBasis   = serializedObject.FindProperty(nameof(UIFlexItem.flexBasis));
            propertyMinimum = serializedObject.FindProperty(nameof(UIFlexItem.flexMinimum));
            propertyMaximum = serializedObject.FindProperty(nameof(UIFlexItem.flexMaximum));
            propertyMinimumEnabled  = serializedObject.FindProperty(nameof(UIFlexItem.flexMinimumEnabled));
            propertyMaximumEnabled  = serializedObject.FindProperty(nameof(UIFlexItem.flexMaximumEnabled));
            propertyAlignSelf       = serializedObject.FindProperty(nameof(UIFlexItem.alignSelfEnabled));
            propertyAlignSelfField  = serializedObject.FindProperty(nameof(UIFlexItem.alignSelf));
            propertyMargin  = serializedObject.FindProperty(nameof(UIFlexItem.margin));
            
        }

        public override void OnInspectorGUI()
        {
            LayoutUtil util;

            serializedObject.Update();
            
            util = new LayoutUtil(serializedObject);

            util.LayoutInt(propertyOrder, contentOrder);

            util.LayoutFloat(propertyGrow, contentGrow);
            util.LayoutFloat(propertyShrink, contentShrink);

            // Basis
            util.Label(contentBasis);
            util.IncreaseIndent();
            util.SetRelativeName("Container Content Size");
            CheckDrivenField(util, SizeField.Basis, 0);
            util.LayoutSize(propertyBasis.FindPropertyRelative(nameof(UISize2D.width)), contentBasisWidth);
            CheckDrivenField(util, SizeField.Basis, 1);
            util.LayoutSize(propertyBasis.FindPropertyRelative(nameof(UISize2D.height)), contentBasisHeight);
            util.UnsetFieldDriven();

            // Minimum
            util.ResetIndent();
            util.Label(contentMinimum);
            util.IncreaseIndent();
            CheckDrivenField(util, SizeField.Minimum, 0);
            util.LayoutSize(propertyMinimum.FindPropertyRelative(nameof(UISize2D.width)), contentMinimumWidth,
                            propertyMinimumEnabled.FindPropertyRelative(nameof(Bool2D.width)));
            CheckDrivenField(util, SizeField.Minimum, 1);
            util.LayoutSize(propertyMinimum.FindPropertyRelative(nameof(UISize2D.height)), contentMinimumHeight,
                            propertyMinimumEnabled.FindPropertyRelative(nameof(Bool2D.height)));
            util.UnsetFieldDriven();

            // Maximum
            util.ResetIndent();
            util.Label(contentMaximum);
            util.IncreaseIndent();
            CheckDrivenField(util, SizeField.Maximum, 0);
            util.LayoutSize(propertyMaximum.FindPropertyRelative(nameof(UISize2D.width)), contentMaximumWidth,
                            propertyMaximumEnabled.FindPropertyRelative(nameof(Bool2D.width)));
            CheckDrivenField(util, SizeField.Maximum, 1);
            util.LayoutSize(propertyMaximum.FindPropertyRelative(nameof(UISize2D.height)), contentMaximumHeight,
                            propertyMaximumEnabled.FindPropertyRelative(nameof(Bool2D.height)));
            util.UnsetFieldDriven();

            // Alignment
            util.Space(8f);
            util.ResetIndent();
            util.LayoutBool(propertyAlignSelf, contentAlignSelf);
            util.IncreaseIndent();
            util.BeginDisabledGroup(!propertyAlignSelf.boolValue);
            util.LayoutEnum<UIFlexItem.Alignment>(propertyAlignSelfField, contentAlignSelfField);
            util.EndDisabledGroup();

            // Margin
            util.Space(8f);
            util.ResetIndent();
            util.LayoutBorderSize(propertyMargin, contentMargin, "Margin", ref foldoutMargin);
            
            if (serializedObject.ApplyModifiedProperties())
                foreach (Object target in targets)
                    (target as UIFlexItem).Revalidate();
        }

        private void CheckDrivenField(LayoutUtil util, SizeField field, int axis)
        {
            UIFlexLinker linker;

            util.UnsetFieldDriven();
            if (targets != null && targets.Length >= 2)
                return;
            linker = ((UIFlexItem)target).GetComponent<UIFlexLinker>();
            if (linker == null)
                return;
            if (linker.drivenAxis != axis)
                return;

            if (field == SizeField.Basis)
                util.SetFieldDriven(linker.drivingBasis, linker.drivingBasisValid);
            else if(field == SizeField.Minimum)
                util.SetFieldDriven(linker.drivingMinimum, linker.drivingMinimumValid);
            else
                util.SetFieldDriven(linker.drivingMaximum, linker.drivingMaximumValid);
        }
    }
}