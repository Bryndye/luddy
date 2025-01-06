using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using BDSZ_2020;
namespace BDSZ_2020
{
    /// <summary> 
    /// Custom editor for UIDrawLine component
    /// </summary>
    [CustomEditor(typeof(BDSZ_UIDrawLineEx))]
    public class BDSZ_UIDrawLineExEditor : BDSZ_UIDrawLineEditor
    {
        SerializedProperty m_Texture;
        SerializedProperty m_UVRect;
        //[SerializeField] TextureType m_TextureType = TextureType.Tiled;
        SerializedProperty m_TextureType;
        GUIContent m_TextureTypeContent;
        GUIContent m_UVRectContent;
        protected override void OnEnable()
        {
            base.OnEnable();

            // Note we have precedence for calling rectangle for just rect, even in the Inspector.
            // For example in the Camera component's Viewport Rect.
            // Hence sticking with Rect here to be consistent with corresponding property in the API.
            m_UVRectContent = EditorGUIUtility.TrTextContent("UV Rect");
            m_Texture = serializedObject.FindProperty("m_Texture");
            m_UVRect = serializedObject.FindProperty("m_UVRect");
            m_TextureType = serializedObject.FindProperty("m_TextureType");
            m_TextureTypeContent = EditorGUIUtility.TrTextContent("Texture Type");
            SetShowNativeSize(true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Texture);
            EditorGUILayout.PropertyField(m_TextureType, m_TextureTypeContent);
            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            EditorGUILayout.PropertyField(m_UVRect, m_UVRectContent);
            SetShowNativeSize(false);
            NativeSizeButtonGUI();
            LinePropertyField();
            serializedObject.ApplyModifiedProperties();
        }

        void SetShowNativeSize(bool instant)
        {
            base.SetShowNativeSize(m_Texture.objectReferenceValue != null, instant);
        }

        private static Rect Outer(BDSZ_UIDrawLineEx rawImage)
        {
            Rect outer = rawImage.uvRect;
            outer.xMin *= rawImage.rectTransform.rect.width;
            outer.xMax *= rawImage.rectTransform.rect.width;
            outer.yMin *= rawImage.rectTransform.rect.height;
            outer.yMax *= rawImage.rectTransform.rect.height;
            return outer;
        }

        /// <summary>
        /// Allow the texture to be previewed.
        /// </summary>

        public override bool HasPreviewGUI()
        {
            BDSZ_UIDrawLineEx rawImage = target as BDSZ_UIDrawLineEx;
            if (rawImage == null)
                return false;

            var outer = Outer(rawImage);
            return outer.width > 0 && outer.height > 0;
        }

        /// <summary>
        /// Draw the Image preview.
        /// </summary>

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            BDSZ_UIDrawLineEx rawImage = target as BDSZ_UIDrawLineEx;
            Texture tex = rawImage.mainTexture;

            if (tex == null)
                return;

            var outer = Outer(rawImage);
            DrawSprite(tex, rect, outer, rawImage.uvRect, rawImage.canvasRenderer.GetColor());
        }

        /// <summary>
        /// Info String drawn at the bottom of the Preview
        /// </summary>

        public override string GetInfoString()
        {
            BDSZ_UIDrawLineEx rawImage = target as BDSZ_UIDrawLineEx;

            // Image size Text
            string text = string.Format("RawImage Size: {0}x{1}",
                Mathf.RoundToInt(Mathf.Abs(rawImage.rectTransform.rect.width)),
                Mathf.RoundToInt(Mathf.Abs(rawImage.rectTransform.rect.height)));

            return text;
        }
        public static void DrawSprite(Texture tex, Rect drawArea, Rect outer, Rect uv, Color color)
        {
            DrawSprite(tex, drawArea, Vector4.zero, outer, outer, uv, color, null);
        }

        // Draw the specified Image.
        private static void DrawSprite(Texture tex, Rect drawArea, Vector4 padding, Rect outer, Rect inner, Rect uv, Color color, Material mat)
        {
            // Create the texture rectangle that is centered inside rect.
            Rect outerRect = drawArea;
            outerRect.width = Mathf.Abs(outer.width);
            outerRect.height = Mathf.Abs(outer.height);

            if (outerRect.width > 0f)
            {
                float f = drawArea.width / outerRect.width;
                outerRect.width *= f;
                outerRect.height *= f;
            }

            if (drawArea.height > outerRect.height)
            {
                outerRect.y += (drawArea.height - outerRect.height) * 0.5f;
            }
            else if (outerRect.height > drawArea.height)
            {
                float f = drawArea.height / outerRect.height;
                outerRect.width *= f;
                outerRect.height *= f;
            }

            if (drawArea.width > outerRect.width)
                outerRect.x += (drawArea.width - outerRect.width) * 0.5f;

            // Draw the background
            EditorGUI.DrawTextureTransparent(outerRect, null, ScaleMode.ScaleToFit, outer.width / outer.height);

            // Draw the Image
            GUI.color = color;

            Rect paddedTexArea = new Rect(
                outerRect.x + outerRect.width * padding.x,
                outerRect.y + outerRect.height * padding.w,
                outerRect.width - (outerRect.width * (padding.z + padding.x)),
                outerRect.height - (outerRect.height * (padding.w + padding.y))
            );

            if (mat == null)
            {
                GUI.DrawTextureWithTexCoords(paddedTexArea, tex, uv, true);
            }
            else
            {
                // NOTE: There is an issue in Unity that prevents it from clipping the drawn preview
                // using BeginGroup/EndGroup, and there is no way to specify a UV rect...
                EditorGUI.DrawPreviewTexture(paddedTexArea, tex, mat);
            }

            //// Draw the border indicator lines
            //GUI.BeginGroup(outerRect);
            //{
            //    tex = contrastTexture;
            //    GUI.color = Color.white;

            //    if (inner.xMin != outer.xMin)
            //    {
            //        float x = (inner.xMin - outer.xMin) / outer.width * outerRect.width - 1;
            //        DrawTiledTexture(new Rect(x, 0f, 1f, outerRect.height), tex);
            //    }

            //    if (inner.xMax != outer.xMax)
            //    {
            //        float x = (inner.xMax - outer.xMin) / outer.width * outerRect.width - 1;
            //        DrawTiledTexture(new Rect(x, 0f, 1f, outerRect.height), tex);
            //    }

            //    if (inner.yMin != outer.yMin)
            //    {
            //        // GUI.DrawTexture is top-left based rather than bottom-left
            //        float y = (inner.yMin - outer.yMin) / outer.height * outerRect.height - 1;
            //        DrawTiledTexture(new Rect(0f, outerRect.height - y, outerRect.width, 1f), tex);
            //    }

            //    if (inner.yMax != outer.yMax)
            //    {
            //        float y = (inner.yMax - outer.yMin) / outer.height * outerRect.height - 1;
            //        DrawTiledTexture(new Rect(0f, outerRect.height - y, outerRect.width, 1f), tex);
            //    }
            //}

            //GUI.EndGroup();
        }
    }
 }
