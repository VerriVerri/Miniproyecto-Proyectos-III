Shader "Custom/WorldSpaceTextureScale"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" { }
        _TextureScale ("Texture Scale in Units", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Declare properties
            sampler2D _MainTex;
            float _TextureScale;

            // Vertex shader input and output structs
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldScale : TEXCOORD2;
            };

            // Vertex shader
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // Standard clip space position
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // World position for object
                o.worldScale = abs(mul((float3x3)unity_ObjectToWorld, v.normal)); // Object scale in world space
                o.uv = v.uv; // Pass through the original UVs
                return o;
            }

            // Fragment shader
            half4 frag(v2f i) : SV_Target
            {
                // Calculate the object scale in world space
                float maxScale = max(i.worldScale.x, max(i.worldScale.y, i.worldScale.z));
                
                // Adjust the UVs based on the world scale and the desired texture scale
                float2 scaledUV = i.uv * _TextureScale * maxScale;

                // Sample the texture with the scaled UVs
                return tex2D(_MainTex, scaledUV);
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}






