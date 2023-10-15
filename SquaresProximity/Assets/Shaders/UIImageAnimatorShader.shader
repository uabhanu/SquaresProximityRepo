Shader "Custom/UIImageAnimatorShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScaleSpeed ("Scale Speed", Range(0.1, 2.0)) = 0.5
        _MaxBackwardDistance ("Max Backward Distance", Range(0.0, 1.0)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
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

            float _ScaleSpeed;
            float _MaxBackwardDistance;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                // Apply animation here
                float scale = 1.0 + sin(_Time.y * _ScaleSpeed) * _MaxBackwardDistance;
                scale = max(scale, 1.0);  // Ensure the scale doesn't go below 1.0
                o.vertex.xy *= scale;
                return o;
            }

            sampler2D _MainTex;
            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}