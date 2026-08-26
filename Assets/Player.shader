Shader "Custom/XRaySilhouette"
{
    Properties
    {
        _Color ("Boja", Color) = (0.2, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Transparent" }

        Pass
        {
            ZTest Greater
            ZWrite Off
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}