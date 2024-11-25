using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FlexLayout
{
    public static class UIGizmos
    {
        private static readonly Color orange = new Color32(255, 165, 0, 255);
        private static readonly Color yellow = new Color32(255, 255, 0, 255);

        public static void SetColor(Color color)
        {
            Gizmos.color = color;
        }

        public static void SetColorPadding()
        {
            SetColor(orange);
        }

        public static void SetColorMargin()
        {
            SetColor(yellow);
        }

        public static void SetColorLine()
        {
            SetColor(Color.blue);
        }

        public static void DrawRect(Rect rect, RectTransform transform)
        {
            if (rect.size.x == 0f || rect.size.y == 0f)
                return;
            rect = CanvasToWorld(rect, transform);
            Gizmos.DrawWireCube(rect.center, rect.size);
        }

        private static Rect CanvasToWorld(Rect rect, RectTransform transform)
        {
            Canvas canvas;
            Vector2 changeSize;
            Vector2 changePos;

            canvas = transform.GetComponentInParent<Canvas>();

            changeSize = rect.size * (canvas.scaleFactor - 1f);
            changePos = (rect.position - (Vector2)transform.position) * (canvas.scaleFactor - 1f);
            rect.size += changeSize;
            rect.position += changePos;
            return (rect);
        }
    }
}