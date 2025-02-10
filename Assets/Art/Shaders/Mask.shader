Shader "Custom/MaskShader" {
    Properties {
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        // It is important that the mask writes to the depth buffer so that it occludes objects behind it.
        Pass {
            // Set the stencil so that pixels drawn by this mask get a reference value (1)
            Stencil {
                Ref 1
                Comp Always
                Pass Replace
            }
            // Use a flat color
            ZWrite On
            Cull Off
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };

            fixed4 _Color;

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                return _Color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}