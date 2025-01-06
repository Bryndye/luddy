#ifndef UI_SHADER_INCLUDED
#define UI_SHADER_INCLUDED


struct appdata_t 
{
	float4 vertex            : POSITION;
    float4 color : COLOR;
	float2 texcoord          : TEXCOORD0;
	float4 texcoord1         : TEXCOORD1;
	float4 texcoord2         : TEXCOORD2;
	 
};

struct v2f
{
	float4 vertex : SV_POSITION;
    float4 color : COLOR;
	float2 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float4 texcoord2 : TEXCOORD2;
	float4 WorldPosition : TEXCOORD3;
	 
};
 
inline float QuickGet2DClipping (in float2 position, in float4 clipRect)
{
 	float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
 	return inside.x * inside.y;
}
 
uniform float4 _MainTex_ST;
uniform float4 _Color;
uniform float4 _ClipRect;
v2f vert(appdata_t v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.color = v.color * _Color;
	o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
	o.texcoord1 = v.texcoord1;
	o.texcoord2 = v.texcoord2;
	o.WorldPosition = v.vertex;
	return o;
}

uniform float4 _CircleClip;
float CircleClip(v2f i)
{
	half2 center = half2(_CircleClip.xy + _CircleClip.zw) * 0.5;
	half2 distToCenter = i.WorldPosition.xy - center;
	half2 distNoramal = distToCenter / (center - half2(_CircleClip.x, _CircleClip.y));
	float x = length(distNoramal);
	half inner = step(0.95f, x);
	half outer = step(1.0f, x);
	half smooth = lerp(outer, inner, (x - 0.95f) * 20.0f);
	half alpha = (1 - smooth);
	return alpha;
}

float2 DistanceToEdge(float2 pos,float2 v0,float2 v1,float2 lastResult)
{
	float2 e = v1 - v0;
	float2 w = pos - v0;
	float2 b = w - e * clamp(dot(w, e) / dot(e, e), 0.0, 1.0);
	lastResult.x = min(lastResult.x, dot(b, b));


	bool3 cond = bool3(pos.y >= v0.y,
		pos.y <v1.y,
		e.x* w.y>e.y * w.x);
	if (all(cond) || all(!(cond))) lastResult.y = -lastResult.y;
	return lastResult;
}

//点到线段的距离=====================================================
float VecToLineSegmentDistance(float2 p, float2 vLineBegin, float2 vLineEnd)
{
	float2 vLineDir = vLineEnd - vLineBegin;
	float d =max(0.0001f, dot(vLineDir,vLineDir));
 	float2 AB = p - vLineBegin;
	float fDistanceToLineSegment0 = dot(vLineDir, AB) / d;
	float fClamp = clamp(fDistanceToLineSegment0,0.0f,1.0f);
	float2 vNearestPoint = vLineBegin + fClamp * vLineDir;
	float fdir = sign(dot(AB, float2(vLineDir.y, -vLineDir.x)));
	return  length(p-vNearestPoint)*fdir;
}

//抗锯齿多边形 像素 shader========================================
 

//x，为平滑像素个数，y为边最小厚度。
float4 _Softness;
float4 _BorderColor;
float4 _GradientColor;
float4 CalcShapeColor(v2f i, sampler2D _MainTex, float fEdgeDistance,float fGradient)
{
	 
	float4 resultClr = i.color; 
#if defined(SHAPE_EDGE_GRADIENT_ON) || defined(SHAPE_MIDDLE_GRADIENT_ON)
	resultClr = lerp(_GradientColor, resultClr, fGradient);
#endif
	 resultClr =  tex2D(_MainTex, i.texcoord)* resultClr;
#ifdef SHAPE_BORDER_COLOR_ON
	float fBorderDistance = max(fEdgeDistance - _Softness.y, 0);
	float fBlend = saturate(fBorderDistance / _Softness.x);
	resultClr = lerp(_BorderColor, resultClr, fBlend);
#endif
	float fSoft = saturate(fEdgeDistance / _Softness.x)* QuickGet2DClipping(i.WorldPosition.xy, _ClipRect);

#ifdef SHAPE_GLOW_ON
	 
	float fGlowRange =_Softness.z;
	float fBackground = smoothstep(0.0f, 1.0f, fGradient); 
	float fGlow = smoothstep(1.0f - fGlowRange, 1.0f, fGradient);
	resultClr.rgb *= (fGlow + fBackground) * _Softness.w * fSoft* resultClr.a;
#endif

	resultClr.a = resultClr.a * fSoft ;
	return resultClr;
}
//抗锯齿多边形 像素 shader========================================
#endif

