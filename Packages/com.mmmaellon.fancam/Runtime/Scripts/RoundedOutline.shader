Shader "Custom/RoundedOutline-UI"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width (local units)", Float) = 5
        _CornerRadius ("Corner Radius (local units)", Float) = 5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _CornerRadius;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 localPos : TEXCOORD1;
                // float2 size : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float2 dir = sign(v.uv - 0.5);
                // o.size = abs(v.vertex.xy);
                o.localPos = v.vertex.xy + dir * _OutlineWidth;
                o.uv = (o.localPos + o.size) / (o.size * 2);
                o.pos = UnityObjectToClipPos(float4(o.localPos, v.vertex.z, v.vertex.w));
                return o;
            }

            // signed distance to a rounded box centered at origin
            // float roundedBoxSDF(float2 p, float2 size, float radius)
            // {
            //     float2 q = abs(p) - size + radius;
            //     return length(max(q, 0)) + min(max(q.x, q.y), 0) - radius;
            // }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(1,0,0,1);
                // float dist = roundedBoxSDF(i.localPos, i.size, _CornerRadius);
                // float aa = fwidth(dist);
                //
                // // fully outside outline band: discard early
                // if (dist > _OutlineWidth + aa) clip(-1);
                //
                // fixed4 texCol = tex2D(_MainTex, i.uv);
                //
                // // 1 = fully image, 0 = fully outline, smoothed across image edge
                // float insideMask = 1 - smoothstep(-aa, aa, dist);
                //
                // // 1 = fully outline, 0 = fully transparent, smoothed across outer edge
                // float outlineMask = 1 - smoothstep(_OutlineWidth - aa, _OutlineWidth + aa, dist);
                //
                // fixed4 col = lerp(_OutlineColor * outlineMask, texCol, insideMask);
                // col.a = lerp(_OutlineColor.a * outlineMask, texCol.a, insideMask);
                //
                // return col;
            }
            ENDCG
        }
    }
}

