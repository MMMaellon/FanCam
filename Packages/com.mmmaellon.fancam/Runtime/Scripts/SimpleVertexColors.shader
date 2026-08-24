Shader "Custom/SimpleVertexColors"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1,1,1,1)
        [HDR] _Color2 ("Color2", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert

        struct Input
        {
            float4 vertColor;
        };

        fixed4 _Color;
        fixed4 _Color2;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertColor = v.color;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = (_Color * IN.vertColor + _Color2).rgb;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
