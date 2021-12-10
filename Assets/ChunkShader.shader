Shader "Custom/ChunkShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Bumpmap", 2D) = "bump" {}
        _BumpScale("Bump Scale", float) = -1
    }
    SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
            LOD 200

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
            sampler2D _MainTex;
            sampler2D _BumpMap;
            float _BumpScale;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(1) // put shadows data into TEXCOORD1
                half3 normal : TEXCOORD2;
                half4 tangent : TEXCOORD3;
                float4 pos : SV_POSITION;
                float3 color : COLOR;
            };

            v2f vert(appdata_full v)
            {

                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.normal = v.normal;
                o.tangent = v.tangent;
                o.color = v.color;

                // compute shadows data
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 normal = UnpackNormal(tex2D(_BumpMap, i.uv));
                fixed4 col = tex2D(_MainTex, i.uv) * half4(i.color, 1.0);

                normal.xy *= _BumpScale;
                normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
                float3x3 tangent2Object =
                {
                    i.tangent.xyz * i.tangent.w,
                    cross(i.tangent * i.tangent.w, i.normal),
                    i.normal
                };
                tangent2Object = transpose(tangent2Object);
                normal = mul(tangent2Object, normal);
                normal = normalize(UnityObjectToWorldNormal(normal));

                half nl = max(0, dot(normal, _WorldSpaceLightPos0.xyz));
                float diff = nl * _LightColor0.rgb;
                float ambient = ShadeSH9(half4(normal, 1));

                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = diff * shadow + ambient;
                col.rgb *= lighting;
                return col;
            }
            ENDCG
        }

        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    FallBack "Diffuse"
}
