Shader "Hidden/HistogramSimple"
{
    Properties
    {

    }
    SubShader
    {
        //Blend SrcAlpha OneMinusSrcAlpha

        Tags {

            "Queue" = "Overlay"

            //"RenderType"="Transparent"
        }

        Pass
        {

            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest Always
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct vertexIn
            {
                float3 pos : POSITION;
            };
            struct vertexOut
            {
                float4 position : SV_POSITION;
            };
            float4x4 _MVP;
            float4 _Color;
            float _Transparancy;
            vertexOut vert (vertexIn v)
            {
                vertexOut vo;
                vo.position = mul(_MVP, float4(v.pos, 1.0));
                return vo;
            }

            float4 frag (vertexOut f) : COLOR0
            {
                return float4(_Color.rgb, _Transparancy);
            }
            ENDCG
        }
    }
}
