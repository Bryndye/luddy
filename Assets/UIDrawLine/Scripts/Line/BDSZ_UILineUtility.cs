using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BDSZ_2020
{
    public class BDSZ_UILineUtility
    {


        /// <summary>
        /// 像素偏移
        /// </summary>
        static Vector2 s_PixelOffset = Vector2.zero;

        public static Vector2 pixelOffset {  set { s_PixelOffset = value; } }

        /// <summary>
        /// 中间数据
        /// </summary>
        static List<Vector2> s_v2dXAxis = new List<Vector2>();

 
        /// <summary>
        /// 获取材质
        /// </summary>
        /// <param name="texture">贴图</param>
        /// <param name="iSoftness">抗锯齿像素个数</param>
        /// <param name="iBorderThickness">边像素个数</param>
        /// <param name="borderColor">边缘颜色</param>
        /// <param name="bGradient">是否渐变？</param>
        /// <param name="gradientClr">渐变颜色</param>
        /// <param name="fGlowRange">发光范围</param>
        /// <param name="fGlowStrength">发光强度</param>
        /// <returns></returns>
        public static Material GetMaterial(Texture texture, int iSoftness, int iBorderThickness, Color32 borderColor, bool bGradient, Color32 gradientClr, float fGlowRange = 0.0f, float fGlowStrength = 0.0f)
        {

            Shader shader = Shader.Find("UIShader/BDSZ_ShapeLine");
            Material Material = new Material(shader);
            Material.SetTexture("_MainTex", texture);
            float fSoft = Mathf.Max(0.001f, iSoftness);

            fGlowStrength = Mathf.Clamp(fGlowStrength + 1.0f, 1.0f, 6.0f);
            Vector4 vSoftness = new Vector4(fSoft, iBorderThickness, Mathf.Clamp01(fGlowRange), fGlowStrength);
            //x,为平滑像素个数，y为最小边厚度。
            Material.SetVector("_Softness", vSoftness);
             if (iBorderThickness>0)
            {
                Material.EnableKeyword("SHAPE_BORDER_COLOR_ON");
                Material.SetColor("_BorderColor", borderColor);
            }
            else
            {
                Material.DisableKeyword("SHAPE_BORDER_COLOR_ON");
            }
            if (bGradient == false)
            {
                Material.DisableKeyword("SHAPE_EDGE_GRADIENT_ON");
                Material.DisableKeyword("SHAPE_MIDDLE_GRADIENT_ON");
            }
            else
            {
                Material.EnableKeyword("SHAPE_MIDDLE_GRADIENT_ON");
                Material.SetColor("_GradientColor", gradientClr);

            }
            if (fGlowRange > 0.0f)
            {
                Material.EnableKeyword("SHAPE_GLOW_ON");
                Material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                Material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            }
            else
            {
                Material.DisableKeyword("SHAPE_GLOW_ON");
                Material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                Material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            }
            return Material;
        }

        /// <summary>
        /// 绘制线段==========
        /// </summary>
        /// <param name="meshHelper">mehs helper</param>
        /// <param name="vBegin">线段起点</param>
        /// <param name="vEnd">线段终点</param>
        /// <param name="fWidth">线段宽度</param>
        /// <param name="clr">线段颜色</param>
        /// <param name="bRoundend">线段两端是否圆角？</param>
        /// <returns></returns>
        public static bool DrawLine(BDSZ_DrawMeshHelper meshHelper, Vector2 vBegin, Vector2 vEnd, float fWidth, Color32 clr, bool bRoundend, float fV = 1.0f)
        {
            return DrawLine(meshHelper, vBegin, vEnd, fWidth, clr, clr, bRoundend, fV);
        }
        /// <summary>
        /// 绘制线段==========
        /// </summary>
        /// <param name="meshHelper">mehs helper</param>
        /// <param name="vBegin">线段起点</param>
        /// <param name="vEnd">线段终点</param>
        /// <param name="fWidth">线段宽度</param>
        /// <param name="beginClr">线段起始颜色</param>
        /// <param name="endClr">线段结束颜色</param>
        /// <param name="bRoundend">线段两端是否圆角？</param>
        /// <param name="fV">纹理坐标系数</param>
        /// <param name="fSolidLength">纹理坐标系数</param>
        /// <param name="fSpaceLength">纹理坐标系数</param>
        /// <returns></returns>
        public static bool DrawLine(BDSZ_DrawMeshHelper meshHelper, Vector2 vBegin, Vector2 vEnd, float fWidth, Color32 beginClr, Color32 endClr, bool bRoundend, float fV = 1.0f, float fSolidLength = 0.0f, float fSpaceLength = 0.0f)
        {


            if (meshHelper==null || fWidth < 1.0f || meshHelper.VertexCount + 4 > BDSZ_DrawMeshHelper.c_iMaxVertexCount)
                return false;
            if (fWidth == 1.0f)
            {
                //1像素的线段，有可能浮点误差，导致绘制看不见，
                //特殊处理一下。
                if (vBegin.x == vEnd.x)
                {

                    vBegin.x = Mathf.Round(vBegin.x) + s_PixelOffset.x;
                    vEnd.x = vBegin.x;
                }
                else if (vBegin.y == vEnd.y)
                {
                    vBegin.y = Mathf.Round(vBegin.y) +  s_PixelOffset.y;
                    vEnd.y = vBegin.y;
                }
            }

            Vector2 vEndToBegin = vEnd - vBegin;
            float fLength = vEndToBegin.magnitude;
            if (fLength < 1.0f)
                return false;
            Vector2 vLineDir = vEndToBegin.normalized;
            Vector2 vLineYAxis = new Vector2(vLineDir.y, -vLineDir.x);

            float fAspectRatio = fLength / fWidth;
            float fRoundedRadius = 0.0f;

            if (bRoundend)
            {
                fRoundedRadius = Mathf.Min(fWidth * 0.5f, fLength * 0.5f);

            }
            Vector4 vData = new Vector4(fLength, fWidth * 0.5f, fLength, fLength);
            //是否为虚线？
            bool bDashed = (fSolidLength >= 1.0f && fSpaceLength >= 1.0f);
            if (bDashed && fSolidLength < fLength)
            {
                vData.z = fSolidLength + fSpaceLength;
                vData.w = fSolidLength;
            }

            float fU2 = fLength;

            meshHelper.m_uv1.Add(new Vector2(0.0f, 0.0f));
            meshHelper.m_uv2.Add(new Vector4(0.0f, -0.5f * fWidth, fRoundedRadius));

            meshHelper.m_uv1.Add(new Vector2(fAspectRatio * fV, 0.0f));
            meshHelper.m_uv2.Add(new Vector4(fU2, -0.5f * fWidth, fRoundedRadius));

            meshHelper.m_uv1.Add(new Vector2(fAspectRatio * fV, fV));
            meshHelper.m_uv2.Add(new Vector4(fU2, 0.5f * fWidth, fRoundedRadius));
            meshHelper.m_uv1.Add(new Vector2(0.0f, fV));
            meshHelper.m_uv2.Add(new Vector4(0.0f, 0.5f * fWidth, fRoundedRadius));


            meshHelper.m_Vertices.Add(vBegin - vLineYAxis * fWidth * 0.5f);
            meshHelper.m_Vertices.Add(vEnd - vLineYAxis * fWidth * 0.5f);
            meshHelper.m_Vertices.Add(vEnd + vLineYAxis * fWidth * 0.5f);
            meshHelper.m_Vertices.Add(vBegin + vLineYAxis * fWidth * 0.5f);


            meshHelper.m_uv3.Add(vData);

            meshHelper.m_uv3.Add(vData);
            meshHelper.m_uv3.Add(vData);

            meshHelper.m_uv3.Add(vData);

            meshHelper.m_Colors32.Add(beginClr);
            meshHelper.m_Colors32.Add(endClr);
            meshHelper.m_Colors32.Add(endClr);
            meshHelper.m_Colors32.Add(beginClr);

            return true;
        }


        /// <summary>
        /// 绘制连续线段，线段个数为顶点数/2
        /// </summary>
        /// <param name="meshHelper">mehs helper</param>
        /// <param name="lineSegments">线段顶点数据</param>
        /// <param name="verClrs">顶点颜色</param>
        /// <param name="fLineBold">线段宽度</param>
        /// <param name="bRoundend">线段两端是否圆角？</param>
        /// <param name="fSolidLength">实线长度</param>
        /// <param name="fSpaceLength">空白长度</param>
        /// <returns></returns>
        public static bool DrawLines(BDSZ_DrawMeshHelper meshHelper, List<Vector2> lineSegments, List<Color32> verClrs, float fLineBold, bool bRoundend = true, float fSolidLength = 0.0f, float fSpaceLength = 0.0f)
        {
            if (lineSegments.Count < 2)
                return false;
            int iWriteIndex = 0;
            float fTotalLength = 0.0f;

            for (int i = 0; i < lineSegments.Count; i += 2)
            {
                Vector2 vSub = lineSegments[i + 1] - lineSegments[i];

                if (Mathf.Abs(vSub.x) >= 1.0f || Mathf.Abs(vSub.y) >= 1.0f)
                {
                    fTotalLength += vSub.magnitude;
                    if (iWriteIndex != i)
                    {
                        lineSegments[iWriteIndex] = lineSegments[i];
                        lineSegments[iWriteIndex + 1] = lineSegments[i + 1];
                        verClrs[iWriteIndex] = verClrs[i];
                        verClrs[iWriteIndex + 1] = verClrs[i + 1];
                    }
                    iWriteIndex += 2;
                }
            }

            if (iWriteIndex < 2)
                return false;

            s_v2dXAxis.Clear();
            CalcLinesYAxis(lineSegments, s_v2dXAxis);


            fTotalLength = 0.0f;
            bool bDashed = (fSolidLength >= 1.0f && fSpaceLength >= 1.0f);

            int iBegin = meshHelper.VertexCount;
            if (bRoundend || bDashed)
            {

                if (meshHelper.VertexCount + iWriteIndex * 5 > BDSZ_DrawMeshHelper.c_iMaxVertexCount)
                    return false;
                float fRoundedRadius = bRoundend ? fLineBold * 0.5f : 0.0f;
                for (int i = 0; i < iWriteIndex; i += 2)
                {
                    fTotalLength = InnerDrawLinePiece(meshHelper, i, lineSegments, s_v2dXAxis, verClrs, fLineBold, fTotalLength, fRoundedRadius);
                }
            }
            else
            {
                if (meshHelper.VertexCount + iWriteIndex * 2 > BDSZ_DrawMeshHelper.c_iMaxVertexCount)
                    return false;

                for (int i = 0; i < iWriteIndex; i += 2)
                {
                    Vector2 vSegmentXAxis = lineSegments[i + 1] - lineSegments[i];
                    float fSegmentLength = vSegmentXAxis.magnitude;
                    vSegmentXAxis.Normalize();
                    float fNextLength = fTotalLength + fSegmentLength;

                    Vector2 vLeftTop = lineSegments[i + 0] - s_v2dXAxis[i + 0] * fLineBold * 0.5f;
                    Vector2 vRightTop = lineSegments[i + 1] - s_v2dXAxis[i + 1] * fLineBold * 0.5f;
                    Vector2 vRightBot = lineSegments[i + 1] + s_v2dXAxis[i + 1] * fLineBold * 0.5f;
                    Vector2 vLeftBot = lineSegments[i + 0] + s_v2dXAxis[i + 0] * fLineBold * 0.5f;

                    meshHelper.m_Vertices.Add(vLeftTop);
                    meshHelper.m_Vertices.Add(vRightTop);
                    meshHelper.m_Vertices.Add(vRightBot);
                    meshHelper.m_Vertices.Add(vLeftBot);

                    meshHelper.m_uv1.Add(new Vector2(fTotalLength, 0.5f));
                    meshHelper.m_uv1.Add(new Vector2(fNextLength, 0.5f));
                    meshHelper.m_uv1.Add(new Vector2(fNextLength, -0.5f));
                    meshHelper.m_uv1.Add(new Vector2(fTotalLength, -0.5f));


                    meshHelper.m_uv2.Add(new Vector4(fTotalLength, -fLineBold * 0.5f, 0.0f, 0.0f));

                    meshHelper.m_uv2.Add(new Vector4(fNextLength, -fLineBold * 0.5f, 0.0f, 0.0f));

                    meshHelper.m_uv2.Add(new Vector4(fNextLength, fLineBold * 0.5f, 0.0f, 0.0f));

                    meshHelper.m_uv2.Add(new Vector4(fTotalLength, fLineBold * 0.5f, 0.0f, 0.0f));


                    meshHelper.m_Colors32.Add(verClrs[i + 0]);
                    meshHelper.m_Colors32.Add(verClrs[i + 1]);
                    meshHelper.m_Colors32.Add(verClrs[i + 1]);
                    meshHelper.m_Colors32.Add(verClrs[i + 0]);


                    fTotalLength = fNextLength;
                }
            }
            Vector4 vData = new Vector4(fTotalLength, fLineBold * 0.5f, fTotalLength, fTotalLength);
            if (bDashed && fSolidLength + fSpaceLength < fTotalLength)
            {
                vData.z = fSolidLength + fSpaceLength;
                vData.w = fSolidLength;
            }
            for (int i = 0; i < meshHelper.VertexCount - iBegin; i++)
            {
                meshHelper.m_uv3.Add(vData);
            }

            return true;
        }

        /// <summary>
        /// 绘制连线中的一段
        /// </summary>
        /// <param name="meshHelper">mehs helper</param>
        /// <param name="iIndex"></param>
        /// <param name="lineSegments"></param>
        /// <param name="vLineYAxis"></param>
        /// <param name="verClrs"></param>
        /// <param name="fLineBold"></param>
        /// <param name="fLineLength"></param>
        /// <param name="fRoundedRadius"></param>

        static float InnerDrawLinePiece(BDSZ_DrawMeshHelper meshHelper, int iIndex, List<Vector2> lineSegments, List<Vector2> vLineYAxis, List<Color32> verClrs, float fLineBold, float fLineLength, float fRoundedRadius)
        {

            Vector2 vPieceXAxis = lineSegments[iIndex + 1] - lineSegments[iIndex];
            vPieceXAxis.Normalize();
            Vector2 vPieceYAxis = new Vector2(vPieceXAxis.y, -vPieceXAxis.x);

            float fHalfBold = fLineBold * 0.5f;

            Vector2 vLeftTop = lineSegments[iIndex + 0] - vLineYAxis[iIndex + 0] * fHalfBold;
            Vector2 vRightTop = lineSegments[iIndex + 1] - vLineYAxis[iIndex + 1] * fHalfBold;
            Vector2 vRightBot = lineSegments[iIndex + 1] + vLineYAxis[iIndex + 1] * fHalfBold;
            Vector2 vLeftBot = lineSegments[iIndex + 0] + vLineYAxis[iIndex + 0] * fHalfBold;

            float fPieceLength = Mathf.Min((vLeftTop - vRightTop).magnitude, (vLeftBot - vRightBot).magnitude);
            float fNextLength = fLineLength + fPieceLength;

            Vector2 vBotToTop = vLeftTop - vLeftBot;
            float fLeftEdgeLength = vBotToTop.magnitude;
            float fTriEdgeLength = 0.0f;
            meshHelper.BeginDraw(EMeshTopology.Triangles);
            if (fLeftEdgeLength >= fLineBold + 1.0f)
            {

                meshHelper.m_Vertices.Add(vLeftBot);
                meshHelper.m_Vertices.Add(vLeftTop);


                float fDot = Vector2.Dot(vBotToTop, vPieceXAxis);

                if (fDot < 0.0f)
                {
                    Vector2 vLeftTop1 = vLeftBot - vPieceYAxis * fLineBold;
                    if (fRoundedRadius > 0.0f)
                    {
                        fTriEdgeLength = (vLeftTop1 - vLeftTop).magnitude;
                    }
                    meshHelper.m_Vertices.Add(vLeftTop1);
                    meshHelper.m_uv1.Add(new Vector2(fLineLength, 1.0f));
                    meshHelper.m_uv1.Add(new Vector2(fLineLength + fTriEdgeLength, 0.0f));
                    meshHelper.m_uv1.Add(new Vector2(fLineLength, 0.0f));
                    meshHelper.m_uv2.Add(new Vector4(fLineLength, fHalfBold, fRoundedRadius, fTriEdgeLength * 0.5f));
                    meshHelper.m_uv2.Add(new Vector4(fLineLength, -fHalfBold, fRoundedRadius, fTriEdgeLength * 0.5f));
                    meshHelper.m_uv2.Add(new Vector4(fLineLength, -fHalfBold, fRoundedRadius, fTriEdgeLength * 0.5f));
                    vLeftTop = vLeftTop1;

                }
                else
                {
                    Vector2 vLeftBot1 = vLeftTop + vPieceYAxis * fLineBold;
                    if (fRoundedRadius > 0.0f)
                    {
                        fTriEdgeLength = (vLeftBot1 - vLeftBot).magnitude;
                    }
                    meshHelper.m_Vertices.Add(vLeftBot1);
                    meshHelper.m_uv1.Add(new Vector2(fLineLength + fTriEdgeLength, 1.0f));
                    meshHelper.m_uv1.Add(new Vector2(fLineLength, 0.0f));
                    meshHelper.m_uv1.Add(new Vector2(fLineLength, 1.0f));
                    meshHelper.m_uv2.Add(new Vector4(fLineLength, fHalfBold, fRoundedRadius, fTriEdgeLength * 0.5f));
                    meshHelper.m_uv2.Add(new Vector4(fLineLength, -fHalfBold, fRoundedRadius, fTriEdgeLength * 0.5f));
                    meshHelper.m_uv2.Add(new Vector4(fLineLength, fHalfBold, fRoundedRadius, fTriEdgeLength * 0.5f));
                    vLeftBot = vLeftBot1;
                }
                meshHelper.m_Colors32.Add(verClrs[iIndex + 0]);
                meshHelper.m_Colors32.Add(verClrs[iIndex + 0]);
                meshHelper.m_Colors32.Add(verClrs[iIndex + 0]);
            }

            vBotToTop = vRightTop - vRightBot;
            float fRightEdgeLength = vBotToTop.magnitude;


            if (fRightEdgeLength >= fLineBold + 1.0f)
            {
                meshHelper.m_Vertices.Add(vRightTop);
                meshHelper.m_Vertices.Add(vRightBot);

                float fDot = Vector2.Dot(vBotToTop, vPieceXAxis);
                if (fDot > 0.0f)
                {


                    Vector2 vRightTop1 = vRightBot - vPieceYAxis * fLineBold;
                    if (fRoundedRadius > 0.0f)
                    {
                        fTriEdgeLength = (vRightTop1 - vRightTop).magnitude;
                    }

                    meshHelper.m_Vertices.Add(vRightTop1);
                    meshHelper.m_uv1.Add(new Vector2(fNextLength + fTriEdgeLength, 0.0f));
                    meshHelper.m_uv1.Add(new Vector2(fNextLength, 1.0f));
                    meshHelper.m_uv1.Add(new Vector2(fNextLength, 0.0f));
                    meshHelper.m_uv2.Add(new Vector4(fNextLength, -fHalfBold, fRoundedRadius, fTriEdgeLength * 0.5f));
                    meshHelper.m_uv2.Add(new Vector4(fNextLength, fHalfBold, fRoundedRadius, fTriEdgeLength * 0.5f));
                    meshHelper.m_uv2.Add(new Vector4(fNextLength, -fHalfBold, fRoundedRadius, fTriEdgeLength * 0.5f));
                    vRightTop = vRightTop1;
                }
                else
                {
                    Vector2 vRightBot1 = vRightTop + vPieceYAxis * fLineBold;
                    if (fRoundedRadius > 0.0f)
                    {
                        fTriEdgeLength = (vRightBot1 - vRightBot).magnitude;
                    }
                    meshHelper.m_Vertices.Add(vRightBot1);
                    meshHelper.m_uv1.Add(new Vector2(fNextLength, 0.0f));
                    meshHelper.m_uv1.Add(new Vector2(fNextLength + fTriEdgeLength, 1.0f));
                    meshHelper.m_uv1.Add(new Vector2(fNextLength, 1.0f));
                    meshHelper.m_uv2.Add(new Vector4(fNextLength, -fHalfBold, fRoundedRadius, fTriEdgeLength * 0.5f));
                    meshHelper.m_uv2.Add(new Vector4(fNextLength, fHalfBold, fRoundedRadius, fTriEdgeLength * 0.5f));
                    meshHelper.m_uv2.Add(new Vector4(fNextLength, fHalfBold, fRoundedRadius, fTriEdgeLength * 0.5f));
                    vRightBot = vRightBot1;
                }
                meshHelper.m_Colors32.Add(verClrs[iIndex + 1]);
                meshHelper.m_Colors32.Add(verClrs[iIndex + 1]);
                meshHelper.m_Colors32.Add(verClrs[iIndex + 1]);
            }


            meshHelper.BeginDraw(EMeshTopology.Quadrangle);


            meshHelper.m_Vertices.Add(vLeftTop);
            meshHelper.m_Vertices.Add(vRightTop);
            meshHelper.m_Vertices.Add(vRightBot);
            meshHelper.m_Vertices.Add(vLeftBot);

            meshHelper.m_uv1.Add(new Vector2(fLineLength, 0.0f));
            meshHelper.m_uv1.Add(new Vector2(fNextLength, 0.0f));
            meshHelper.m_uv1.Add(new Vector2(fNextLength, 1.0f));
            meshHelper.m_uv1.Add(new Vector2(fLineLength, 1.0f));



            meshHelper.m_uv2.Add(new Vector4(fLineLength, -fHalfBold, fRoundedRadius, 0.0f));
            meshHelper.m_uv2.Add(new Vector4(fNextLength, -fHalfBold, fRoundedRadius, 0.0f));
            meshHelper.m_uv2.Add(new Vector4(fNextLength, fHalfBold, fRoundedRadius, 0.0f));
            meshHelper.m_uv2.Add(new Vector4(fLineLength, fHalfBold, fRoundedRadius, 0.0f));


            meshHelper.m_Colors32.Add(verClrs[iIndex + 0]);
            meshHelper.m_Colors32.Add(verClrs[iIndex + 1]);
            meshHelper.m_Colors32.Add(verClrs[iIndex + 1]);
            meshHelper.m_Colors32.Add(verClrs[iIndex + 0]);

            return fNextLength;
        }

        /// <summary>
        /// 绘制连续线段，线段个数为顶点数-1
        /// </summary>
        /// <param name="meshHelper">mehs helper</param>
        /// <param name="lineSegments">线段顶点数据</param>
        /// <param name="vLineYAxis">线段端点垂直轴方向</param>
        /// <param name="verClrs">顶点颜色</param>
        /// <param name="fLineBold">线段宽度</param>
        /// <param name="bRoundend">线段两端是否圆角？</param>
        /// <param name="fSolidLength">实线长度</param>
        /// <param name="fSpaceLength">空白长度</param>
        /// <returns></returns>
        public static bool DrawLineStrips(BDSZ_DrawMeshHelper meshHelper, List<Vector2> lineSegments, List<Vector2> vLineYAxis, List<Color32> verClrs, float fLineBold, bool bRoundend = true, float fSolidLength = 0.0f, float fSpaceLength = 0.0f)
        {
            if (lineSegments.Count < 2)
                return false;
            float fTotalLength = 0.0f;

            bool bDashed = (fSolidLength >= 1.0f && fSpaceLength >= 1.0f);
            if (bDashed || bRoundend)
            {
                if (meshHelper.VertexCount + (lineSegments.Count - 1) * 10 > BDSZ_DrawMeshHelper.c_iMaxVertexCount)
                    return false;
                float fRoundedRadius = bRoundend ? fLineBold * 0.5f : 0.0f;
                int iBegin = meshHelper.VertexCount;

                for (int i = 0; i < lineSegments.Count - 1; i++)
                {

                    fTotalLength = InnerDrawLinePiece(meshHelper, i, lineSegments, vLineYAxis, verClrs, fLineBold, fTotalLength, fRoundedRadius);
                }
                Vector4 vData = new Vector4(fTotalLength, fLineBold * 0.5f, fTotalLength, fTotalLength);
                if (bDashed)
                {
                    vData.z = fSolidLength + fSpaceLength;
                    vData.w = fSolidLength;
                }
                for (int i = 0; i < meshHelper.VertexCount - iBegin; i++)
                {
                    meshHelper.m_uv3.Add(vData);
                }

            }
            else
            {
                if (meshHelper.VertexCount + lineSegments.Count * 2 > BDSZ_DrawMeshHelper.c_iMaxVertexCount)
                    return false;

                meshHelper.BeginDraw(EMeshTopology.Ribbon);
                for (int i = 0; i < lineSegments.Count; i++)
                {

                    if (i > 0)
                    {
                        fTotalLength += (lineSegments[i] - lineSegments[i - 1]).magnitude;
                    }
                    Vector3 vTopVec = lineSegments[i] + vLineYAxis[i] * fLineBold * 0.5f;
                    Vector3 vBotVec = lineSegments[i] - vLineYAxis[i] * fLineBold * 0.5f;

                    meshHelper.m_Vertices.Add(vTopVec);
                    meshHelper.m_Vertices.Add(vBotVec);

                    meshHelper.m_uv1.Add(new Vector2(fTotalLength, 0.5f));
                    meshHelper.m_uv1.Add(new Vector2(fTotalLength, -0.5f));
                    meshHelper.m_uv2.Add(new Vector3(fTotalLength, fLineBold * 0.5f, 0.0f));
                    meshHelper.m_uv2.Add(new Vector3(fTotalLength, -fLineBold * 0.5f, 0.0f));
                    meshHelper.m_Colors32.Add(verClrs[i]);
                    meshHelper.m_Colors32.Add(verClrs[i]);
                }
                Vector4 vData = new Vector4(fTotalLength, fLineBold * 0.5f, fTotalLength, fTotalLength);
                for (int i = 0; i < lineSegments.Count; i++)
                {

                    meshHelper.m_uv3.Add(vData);
                    meshHelper.m_uv3.Add(vData);
                }
            }
            meshHelper.BeginDraw(EMeshTopology.Quadrangle);

            return true;

        }

        /// <summary>
        /// 绘制曲线
        /// </summary>
        /// <param name="meshHelper">mehs helper</param>
        /// <param name="vKeys"></param>
        /// <param name="fThickness">线厚度</param>
        /// <param name="clr">颜色</param>
        /// <param name="type">插值类型</param>
        /// <param name="fSamplingDistance">采样距离</param>
        /// <param name="bRoundend">线段两端是否圆角？</param>
        /// <param name="fSolidLength"></param>
        /// <param name="fSpaceLength"></param>
        /// <returns></returns>
        public static bool DrawCurveLine(BDSZ_DrawMeshHelper meshHelper, List<Vector2> vKeys, float fThickness, Color32 clr, EUILineSplineType type = EUILineSplineType.Catmullrom, float fSamplingDistance = 8.0f, bool bRoundend = true, float fSolidLength = 0.0f, float fSpaceLength = 0.0f)
        {


            List<Vector2> vLineVecs = new List<Vector2>();
            List<Vector2> vLineYAxis = new List<Vector2>();
            List<Color32> vLineClrs = new List<Color32>();
            if (type == EUILineSplineType.Line)
            {
                Vector2 vLastLineDir = (vKeys[1] - vKeys[0]).normalized;
                vLineVecs.Add(vKeys[0]);
                vLineYAxis.Add(new Vector2(vLastLineDir.y, -vLastLineDir.x));
                for (int i = 1; i < vKeys.Count; i++)
                {
                    Vector2 vCurDir = Vector2.zero;
                    if (i + 1 < vKeys.Count)
                    {
                        vCurDir = (vKeys[i + 1] - vKeys[i]).normalized;
                    }
                    Vector2 vAvarageDir = (vLastLineDir + vCurDir) * 0.5f;
                    vAvarageDir.Normalize();
                    float fDot = Vector3.Dot(vAvarageDir, vLastLineDir);
                    Vector2 vTemp = new Vector2(vAvarageDir.y, -vAvarageDir.x);
                    vTemp *= (1.0f / fDot);
                    vLineYAxis.Add(vTemp);
                    vLastLineDir = vCurDir;
                    vLineVecs.Add(vKeys[i]);

                }
            }
            else
            {
                BDSZ_UISplineCurve splineCurve = new BDSZ_UISplineCurve(vKeys, false, type);
                splineCurve.Sampling(vLineVecs, vLineYAxis, fSamplingDistance);
            }
            for (int i = 0; i < vLineVecs.Count; i++)
            {
                vLineClrs.Add(clr);
            }
              
            DrawLineStrips(meshHelper, vLineVecs, vLineYAxis, vLineClrs, fThickness, bRoundend, fSolidLength, fSpaceLength);
            return true;

        }
        /// <summary>
        /// 计算线段带子的纵轴，相邻2个点组成一个线段，例如0-1，1-2，线段个数为顶点数-1
        /// </summary>
        /// <param name="lineSegments">线段顶点数据</param>
        /// <param name="vYAxis">线段纵轴</param>
        /// <returns></returns>
        public static bool CalcLineStripsYAxis(List<Vector2> lineSegments, List<Vector2> vYAxis)
        {

            if (lineSegments.Count < 2)
                return false;
            Vector2 vLastDir = (lineSegments[1] - lineSegments[0]).normalized;

            vYAxis.Clear();
            vYAxis.Add(new Vector2(vLastDir.y, -vLastDir.x));
            Vector2 vTemp;
            int iLast = lineSegments.Count - 1;
            bool bIsClosed = lineSegments[0] == lineSegments[iLast];
            int iEnd = lineSegments.Count - 1;
            if (bIsClosed)
                iEnd++;
            for (int i = 1; i < iEnd; i++)
            {
                Vector2 vCurrentDir = lineSegments[(i + 1) % lineSegments.Count] - lineSegments[i];
                vCurrentDir.Normalize();
                Vector2 vAvarageDir = (vLastDir + vCurrentDir) * 0.5f;
                vAvarageDir.Normalize();
                float fDot = Vector3.Dot(vAvarageDir, vLastDir);
                vTemp = new Vector2(vAvarageDir.y, -vAvarageDir.x);
                vTemp *= (1.0f / fDot);
                vYAxis.Add(vTemp);
                vLastDir = vCurrentDir;
            }
            if (bIsClosed == false)
            {
                vYAxis.Add(new Vector2(vLastDir.y, -vLastDir.x));
            }
            else
            {
                vYAxis[0] = vYAxis[iLast];
            }
            return true;

        }

        /// <summary>
        /// 计算线段的纵轴,每2个点组成一段线段，如0-1，2-3， 线段个数为顶点数/2
        /// </summary>
        /// <param name="lineSegments">线段顶点数据</param>
        /// <param name="vYAxis">线段纵轴</param>
        /// <returns></returns>
        public static bool CalcLinesYAxis(List<Vector2> lineSegments, List<Vector2> vYAxis)
        {

            if (lineSegments.Count < 2)
                return false;
            Vector2 vLastDir = (lineSegments[1] - lineSegments[0]).normalized;
            vYAxis.Clear();
            Vector2 vTemp = new Vector2(vLastDir.y, -vLastDir.x);
            int iLast = lineSegments.Count - 1;
            bool bIsClosed = lineSegments[0] == lineSegments[iLast];
            int iEnd = lineSegments.Count;
            iEnd += 2;
            for (int i = 2; i < iEnd; i += 2)
            {
                vYAxis.Add(vTemp);
                Vector2 vCurrentDir = lineSegments[(i + 1) % lineSegments.Count] - lineSegments[i % lineSegments.Count];
                vCurrentDir.Normalize();
                Vector2 vAvarageDir = (vLastDir + vCurrentDir) * 0.5f;
                vAvarageDir.Normalize();
                float fDot = Vector3.Dot(vAvarageDir, vLastDir);
                vTemp = new Vector2(vAvarageDir.y, -vAvarageDir.x);
                vTemp *= (1.0f / fDot);
                vYAxis.Add(vTemp);
                vLastDir = vCurrentDir;
            }
            if (bIsClosed == false)
            {
                vLastDir = (lineSegments[iLast] - lineSegments[iLast - 1]).normalized;
                vYAxis[iLast] = new Vector2(vLastDir.y, -vLastDir.x);

            }
            else
            {
                vYAxis[0] = vYAxis[iLast];
            }
            return true;

        }

    }
}