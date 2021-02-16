// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "BubbleSoccer/Water"
{Properties
	{
_HeightMin("Height Min", Float) = -1
_HeightMax("Height Max", Float) = 1
_ColorMin("Tint Color At Min", Color) = (0, 0, 0, 1)
_ColorMax("Tint Color At Max", Color) = (1, 1, 1, 1)
_MainTex("Albedo (RGB)", 2D) = "white" {}
_Glossiness("Smoothness", Range(0, 1)) = 0.5
_Metallic("Metallic", Range(0, 1)) = 0.0
_FoamColor("Foam Color", Color) = (1,1,1,1)
_FadeLength("Foam Length", float) = 1
_WaveA("Wave A (dir, steepness, wavelength)", Vector) = (1, 0, 0.5, 10)
_WaveB("Wave B", Vector) = (0, 1, 0.25, 20)
_WaveC("Wave C", Vector) = (1, 1, 0.15, 10)
}
SubShader{
	Tags { "RenderType" = "Opaque" }
	LOD 200

	CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
	#pragma target 3.0

	sampler2D _MainTex;

	struct Input {
		float2 uv_MainTex;
		float3 worldPos;
		float4 screenPos;
	};

	half _Glossiness;
	half _Metallic;
	float4 _WaveA, _WaveB, _WaveC;
	fixed4 _ColorMin;
	fixed4 _ColorMax;
	sampler2D _CameraDepthTexture;
	float _HeightMin;
	float _HeightMax;
	float4 _FoamColor;
	float _FadeLength;

	float3 GerstnerWave(
		float4 wave, float3 p, inout float3 tangent, inout float3 binormal
	) {
		float steepness = wave.z;
		float wavelength = wave.w;
		float k = 2 * UNITY_PI / wavelength;
		float c = sqrt(9.8 / k);
		float2 d = normalize(wave.xy);
		float f = k * (dot(d, p.xz) - c * _Time.y);
		float a = steepness / k;

		p.x += d.x * (a * cos(f));
		p.y = a * sin(f);
		p.z += d.y * (a * cos(f));

		tangent += float3(
			-d.x * d.x * (steepness * sin(f)),
			d.x * (steepness * cos(f)),
			-d.x * d.y * (steepness * sin(f))
			);
		binormal += float3(
			-d.x * d.y * (steepness * sin(f)),
			d.y * (steepness * cos(f)) ,
			-d.y * d.y * (steepness * sin(f))
			);
		return float3(
			d.x * (a * cos(f)),
			a * sin(f),
			d.y * (a * cos(f))
			);
	}

	void vert(inout appdata_full vertexData) {
		float3 gridPoint = vertexData.vertex.xyz + (1,1,3);
		float3 tangent = float3(1, 0, 0);
		float3 binormal = float3(0, 0, 1);
		float3 p = gridPoint;
		p += GerstnerWave(_WaveA, gridPoint, tangent, binormal);
		p += GerstnerWave(_WaveB, gridPoint, tangent, binormal);
		p += GerstnerWave(_WaveC, gridPoint, tangent, binormal);
		float3 normal = normalize(cross(binormal, tangent));
		vertexData.vertex.xyz = p;
		vertexData.normal = normal;


	}

	void surf(Input IN, inout SurfaceOutputStandard o) {
		float h = (_HeightMax - IN.worldPos.y) / (_HeightMax - _HeightMin);
	
		fixed4 tintColor = lerp(_ColorMax.rgba, _ColorMin.rgba, h);


		float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos)));
		float surfZ = -mul(UNITY_MATRIX_V, float4(IN.worldPos.xyz, 1)).z;
		float diff = sceneZ - surfZ;
		float intersect = 1 - saturate(diff / _FadeLength);
		
		fixed4 col = tex2D(_MainTex, IN.uv_MainTex) * tintColor;
		fixed4 finalCol = lerp(col, _FoamColor, pow(intersect,4));

		o.Albedo = finalCol.rgb;
		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
		o.Alpha = finalCol.a;
	}
	ENDCG
}
FallBack "Diffuse"
}