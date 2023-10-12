Shader "Custom/HoleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DotSize ("Dot Size", Range(0, 0.5)) = 0.02
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
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
            float _DotSize;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Calculate the distance from the pixel to the center
                float2 center = 0.5;
                float distanceToCenter = distance(i.uv, center);

                // If the pixel is within the specified dot size of the center, color it red
                if (distanceToCenter < _DotSize) {
                    return half4(1, 0, 0, 1); // Color the center pixel red
                }

                // Sample the texture as usual
                half4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}