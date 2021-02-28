﻿Shader "Custom/Geometry/LineStream/Line"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Cull Off

            CGPROGRAM
            #pragma vertex vert
			#pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

			struct v2g
			{
				float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
			};

            struct g2f
            {
				float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2g vert (a2v v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

			//[maxvertexcount(6)]
			//void geom(triangle v2g input[3], inout LineStream<g2f> outStream)
			//{
			//	g2f o;
			//	for(int i = 0; i < 3; i++)
			//	{
			//		o.vertex = UnityObjectToClipPos(input[i].vertex);
			//		o.uv = input[i].uv;
			//		outStream.Append(o);

			//		int next = (i + 1) % 3;

			//		o.vertex = UnityObjectToClipPos(input[next].vertex);
			//		o.uv = input[next].uv;
			//		outStream.Append(o);
			//	}
			//}

            [maxvertexcount(1)]
			void geom(triangle v2g input[3], inout PointStream<g2f> outStream)
			{
				float4 vertex = float4(0, 0, 0, 0);
				float2 uv = float2(0, 0);

				for(int i = 0; i < 3; i++)
				{
					vertex += input[i].vertex;
					uv += input[i].uv;
				}

				vertex /= 3;
				uv /= 3;

				g2f o;
				o.vertex = UnityObjectToClipPos(vertex);
				o.uv = uv;

				outStream.Append(o);
			}

            fixed4 frag (g2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}