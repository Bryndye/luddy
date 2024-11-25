using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FlexLayout.Edit
{

    public static class TextureUtil
    {
        public static Texture2D Rotate90(Texture2D texture, bool clockwise)
        {
            if (clockwise)
                return (ChangeAxis(texture, Vector2Int.down, Vector2Int.right));
            return (ChangeAxis(texture, Vector2Int.up, Vector2Int.left));
        }

        public static Texture2D Flip(Texture2D texture, bool horizontal)
        {
            if (horizontal)
                return (ChangeAxis(texture, Vector2Int.left, Vector2Int.up));
            return (ChangeAxis(texture, Vector2Int.right, Vector2Int.down));
        }

        private static Texture2D ChangeAxis(Texture2D texture, Vector2Int AxisX, Vector2Int AxisY)
        {
            Texture2D textureAltered;
            Color32[] original;
            Color32[] altered;
            Vector2Int index2D;
            Vector2Int indexStart;
            int wOriginal;
            int hOriginal;
            int iOriginal;
            int w;
            int h;
            int i;
            if (!texture.isReadable)
            {
                Debug.LogWarning(texture.name + ": Texture2D is not readable. Enable Read/Write in texture settings.");
                return (texture);
            }
            if (AxisX == Vector2Int.right && AxisY == Vector2Int.up)
                return (texture);
            if (Math.Abs(AxisX.x) + Math.Abs(AxisY.x) != 1)
                return (texture);
            if (Math.Abs(AxisX.y) + Math.Abs(AxisY.y) != 1)
                return (texture);
            original = texture.GetPixels32();
            wOriginal = texture.width;
            hOriginal = texture.height;
            w = (AxisX.x != 0 ? wOriginal : hOriginal);
            h = (AxisX.x != 0 ? hOriginal : wOriginal);
            altered = new Color32[w * h];

            indexStart = new Vector2Int();
            indexStart.x = (1 - (((AxisX + AxisY).x + 1) / 2)) * (w - 1);
            indexStart.y = (1 - (((AxisX + AxisY).y + 1) / 2)) * (h - 1);
            for (int y = 0; y < hOriginal; ++y)
            {
                for (int x = 0; x < wOriginal; ++x)
                {
                    iOriginal   = y * wOriginal + x;
                    index2D     = AxisX * x + AxisY * y + indexStart;
                    i           = index2D.y * w + index2D.x;
                    altered[i]  = original[iOriginal];
                }
            }
            textureAltered = new Texture2D(w, h);
            textureAltered.SetPixels32(altered);
            textureAltered.Apply();
            return (textureAltered);
        }
    }
}