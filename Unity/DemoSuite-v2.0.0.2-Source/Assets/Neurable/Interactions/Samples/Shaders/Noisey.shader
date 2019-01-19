// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Neurable/Noisey"
{
	Properties
	{
		[Toggle]_AddNoise("Add Noise", Float) = 0
		_MaxNoise("MaxNoise", Range( 0 , 0.1)) = 0.05
		_Albedo("Albedo", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_Emission("Emission", 2D) = "black" {}
		_Occlusion("Occlusion", 2D) = "white" {}
		_Color("Color", Color) = (0,0,0,0)
		_Smoothness("Smoothness", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 4.6
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Color;
		uniform sampler2D _Albedo;
		uniform sampler2D _Emission;
		uniform float _Smoothness;
		uniform sampler2D _Occlusion;
		uniform float _AddNoise;
		uniform float _MaxNoise;


		float3 mod289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float simplePerlin3D22 = snoise( ( float4( ase_vertexNormal , 0.0 ) + _SinTime ).xyz );
			float VertexOffset10 = lerp(0.0,(0 + (simplePerlin3D22 - 0) * (_MaxNoise - 0) / (1 - 0)),_AddNoise);
			float3 temp_cast_2 = (VertexOffset10).xxx;
			v.vertex.xyz += temp_cast_2;
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_TexCoord18 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			o.Normal = tex2D( _Normal, uv_TexCoord18 ).rgb;
			o.Albedo = ( _Color * tex2D( _Albedo, uv_TexCoord18 ) ).rgb;
			o.Emission = tex2D( _Emission, uv_TexCoord18 ).rgb;
			o.Smoothness = _Smoothness;
			o.Occlusion = tex2D( _Occlusion, uv_TexCoord18 ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=13801
-1434;-18;1426;1364;1190.658;510.0728;1.068064;True;False
Node;AmplifyShaderEditor.CommentaryNode;9;-2370.623,440.7866;Float;False;1385.437;694.0002;Vertex Noise;8;3;1;10;19;22;4;8;20;;1,1,1,1;0;0
Node;AmplifyShaderEditor.NormalVertexDataNode;8;-2336.135,616.0713;Float;False;0;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SinTimeNode;20;-2342.446,829.5611;Float;False;0;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-2117.034,659.6531;Float;False;2;2;0;FLOAT3;0,0,0,0;False;1;FLOAT4;0,0,0;False;1;FLOAT4
Node;AmplifyShaderEditor.RangedFloatNode;4;-2093.563,959.5771;Float;False;Property;_MaxNoise;MaxNoise;1;0;0.05;0;0.1;0;1;FLOAT
Node;AmplifyShaderEditor.NoiseGeneratorNode;22;-1978.681,592.795;Float;False;Simplex3D;1;0;FLOAT3;0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.TFHCRemapNode;3;-1780.687,699.8181;Float;False;5;0;FLOAT;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-918.6567,-130.0714;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;23;-454.751,-645.0098;Float;False;Property;_Color;Color;6;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;14;-464.6567,-396.0714;Float;True;Property;_Albedo;Albedo;2;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ToggleSwitchNode;1;-1505.795,685.7158;Float;False;Property;_AddNoise;Add Noise;0;0;0;2;0;FLOAT;0.0;False;1;FLOAT;0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;10;-1195.008,756.712;Float;False;VertexOffset;-1;True;1;0;FLOAT;0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;17;-533.6567,273.9286;Float;True;Property;_Occlusion;Occlusion;5;0;Assets/Textures and Sprites/Lit UI/White.psd;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;25;-498.5529,208.7345;Float;False;Property;_Smoothness;Smoothness;7;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;11;-171.9534,454.6307;Float;True;10;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;15;-457.6567,-187.0714;Float;True;Property;_Normal;Normal;3;0;None;True;0;False;bump;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-48.75098,-514.0098;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SamplerNode;16;-537.6567,23.92859;Float;True;Property;_Emission;Emission;4;0;Assets/AmplifyShaderEditor/Plugins/EditorResources/Textures/black.png;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;50,-74;Float;False;True;6;Float;ASEMaterialInspector;0;0;StandardSpecular;Neurable/Noisey;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;15;10;25;False;0.5;True;0;One;One;0;One;One;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;-1;-1;-1;0;1;;0;0;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;19;0;8;0
WireConnection;19;1;20;0
WireConnection;22;0;19;0
WireConnection;3;0;22;0
WireConnection;3;4;4;0
WireConnection;14;1;18;0
WireConnection;1;1;3;0
WireConnection;10;0;1;0
WireConnection;17;1;18;0
WireConnection;15;1;18;0
WireConnection;24;0;23;0
WireConnection;24;1;14;0
WireConnection;16;1;18;0
WireConnection;0;0;24;0
WireConnection;0;1;15;0
WireConnection;0;2;16;0
WireConnection;0;4;25;0
WireConnection;0;5;17;0
WireConnection;0;11;11;0
ASEEND*/
//CHKSM=6DC30FBD2A2D4161248489CD245F37DC4B23DE68