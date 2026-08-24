Shader "Unlit/IconShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Scale ("Screen Size Scale", Range(0.01, 2.0)) = 0.1
        _Color ("Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 100
        Cull Off
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Scale;
            fixed4 _Color;
            float _Cutoff;
            v2f vert (appdata v)
            {
                v2f o;
                float3 centerViewPos = mul(UNITY_MATRIX_MV, float4(0,0,0,1)).xyz;
                float3 finalViewPos = centerViewPos + v.vertex.xyz * length(centerViewPos) * _Scale;
                o.vertex = mul(UNITY_MATRIX_P, float4(finalViewPos, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                clip(col.a - _Cutoff);
                return col * _Color;
            }
            ENDCG
        }
    }
}