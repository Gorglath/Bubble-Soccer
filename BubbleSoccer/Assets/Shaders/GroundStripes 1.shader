Shader "Enviornment/Ground Stripes"
{
    Properties
    {
        _DarkerColor ("DarkerColor", Color) = (0,0,0,0)
        _LighterColor("LighterColor", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        float4 _MainTex_ST;
       
        struct vertexInput {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 tangent : TANGENT;

        };

        struct Input
        {
            float3 worldPos;
            float4 vert;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _LighterColor;
        fixed4 _DarkerColor;

        void vert(inout vertexInput v, out Input o) {
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.vert = v.vertex;

        }
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.worldPos.xz + _MainTex_ST.zw;
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, uv * _MainTex_ST.xy);
            o.Albedo = lerp(_LighterColor,_DarkerColor,c.r);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
