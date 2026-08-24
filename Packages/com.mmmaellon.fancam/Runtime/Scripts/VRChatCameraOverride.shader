Shader "Custom/VRChatCameraOverride"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        // Use the [HDR] attribute to get an HDR color picker in the material inspector.
        [HDR] _BorderColor ("Border Color", Color) = (0,0,0,1)

        [MaterialToggle] _CameraOnly ( "Show in VRChat camera only", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Geometry-2000" "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend One Zero
            Cull Off
            ZWrite On
            ZTest Always

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            // Use half4 for HDR color.
            half4 _BorderColor; 
            float _VRChatCameraMode;
            float _CameraOnly;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }
            
            // The fragment shader must return half4 or float4 to support HDR.
            half4 frag (v2f i, out float depth : SV_Depth) : SV_Target
            {
                clip(_VRChatCameraMode - _CameraOnly);

                // --- Aspect Ratio Correction Logic (unchanged) ---
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float screenAspect = _ScreenParams.x / _ScreenParams.y;
                float textureAspect = _MainTex_TexelSize.z / _MainTex_TexelSize.w;

                float2 scale = float2(1.0, 1.0);
                float2 offset = float2(0.0, 0.0);

                if (screenAspect > textureAspect)
                {
                    scale.x = textureAspect / screenAspect;
                    offset.x = (1.0 - scale.x) / 2.0;
                }
                else
                {
                    scale.y = screenAspect / textureAspect;
                    offset.y = (1.0 - scale.y) / 2.0;
                }

                float2 finalUV = (screenUV - offset) / scale;
                float border = step(0, finalUV.x) * step(finalUV.x, 1) * step(0, finalUV.y) * step(finalUV.y, 1);
                clip(border - 0.5);

                // --- Color Sampling ---
                // Sample the texture into a half4 variable to preserve HDR data.
                half4 textureColor = tex2D(_MainTex, finalUV);

                depth = 1;
                return textureColor;
            }
            ENDCG
        }
    }
}

