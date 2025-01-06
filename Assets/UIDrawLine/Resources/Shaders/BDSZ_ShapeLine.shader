// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
Shader "UIShader/BDSZ_ShapeLine" 
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_Color("Text Color", Color) = (1,1,1,1)
		_Softness("Softness", vector) = (2.0,0.0,0.0,0.0)
		[HideInInspector] _SrcBlend("__src", Float) = 5.0 // SrcAlpha
		[HideInInspector] _DstBlend("__dst", Float) = 10.0 // OneMinusSrcAlpha
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
		}
		Lighting Off Cull Off ZTest Always ZWrite Off
		Blend[_SrcBlend][_DstBlend]
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		    #pragma multi_compile _ SHAPE_EDGE_GRADIENT_ON SHAPE_MIDDLE_GRADIENT_ON
            #pragma multi_compile _ SHAPE_BORDER_COLOR_ON
            #pragma multi_compile _ SHAPE_GLOW_ON
			 
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			#include "BDSZ_UIUtil.cginc"
			 
			sampler2D _MainTex;
			float4 frag(v2f i) : SV_Target
			{
     			
                //float4 _ArcData;//x,�����߶γ��ȣ�y,�߶ο�ȣ�
				//z�����Ϊ���ߣ���ôΪС��ʵ�Ĳ���+�հײ��ֳ��ȣ�
				//w, ���Ϊ���� ʵ�Ĳ��ֳ��ȡ�
				float4 _ArcData = i.texcoord2;
				float2 vLocalPos = float2(i.texcoord1.x,abs(i.texcoord1.y));
				float fPartX = fmod(vLocalPos.x, _ArcData.z);
			 	
				float fr = min(i.texcoord1.z,_ArcData.w*0.5f);
 				float fLeftLength = _ArcData.x - (vLocalPos.x - fPartX);
				float fCircleCenterX = min(_ArcData.w, fLeftLength) * 0.5f;
				float2 vDashedCircleCenter = float2(fCircleCenterX,_ArcData.y) - fr;
				////��Բ�Ĳ�ֵ��
				float2 vDistToDashedCircle = float2(abs(fPartX - fCircleCenterX),vLocalPos.y) - vDashedCircleCenter;
				////�㵽��߽�ľ��롣
				float fEdgeDistance = fr - (length(max(vDistToDashedCircle, 0.0)) + min(max(vDistToDashedCircle.x, vDistToDashedCircle.y), 0.0));
				
				if (i.texcoord1.w > 0.0f)
				{
					float fx = i.texcoord.x - vLocalPos.x;
					float fProjectX = clamp(fx,0.0f,i.texcoord1.w);
					float fDistance = _ArcData.y - length(float2(fx-fProjectX, vLocalPos.y));
					fEdgeDistance =  min(fDistance, fEdgeDistance);

				} 
			    
				float fGradient = 0.0f;
#if defined(SHAPE_MIDDLE_GRADIENT_ON) || defined(SHAPE_GLOW_ON)
				fGradient = saturate(fEdgeDistance / _ArcData.y);
#elif defined(SHAPE_EDGE_GRADIENT_ON)
				fGradient = saturate(fEdgeDistance / _ArcData.y);
#endif
 
				return CalcShapeColor(i, _MainTex, fEdgeDistance, fGradient);

				 
			}
			ENDCG
		}
	}
}