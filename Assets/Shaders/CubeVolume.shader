// 2022-05-24 BeXide
// original https://docs.unity3d.com/ja/2021.3/Manual/class-Texture3D.html

Shader "Unlit/VolumeShader"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
        _MainColor ("Color", Color) = (1,1,1,1)
        _Alpha ("Alpha", float) = 0.02
        _StepSize ("Step Size", float) = 0.01
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend One OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            //レイマーチングサンプルの最大数 
            #define MAX_STEP_COUNT 128

            // 可能な浮動小数点の不正確性 
            #define EPSILON 0.001f

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 objectVertex : TEXCOORD0;
                float3 vectorToSurface : TEXCOORD1;
            };

            sampler3D _MainTex;
            float4 _MainTex_ST;
            float _Alpha;
            float _StepSize;
            float4 _MainColor;

            v2f vert (appdata v)
            {
                v2f o;

                // オブジェクト空間の頂点。これがレイマーチングの開始点になります
                o.objectVertex = v.vertex;

                // ワールド空間でカメラから頂点までのベクトルを計算します
                float3 worldVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vectorToSurface = worldVertex - _WorldSpaceCameraPos;

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 BlendUnder(float4 color, float4 newColor)
            {
                color.rgb += (1.0 - color.a) * newColor.a * newColor.rgb;
                color.a += (1.0 - color.a) * newColor.a;
                return color;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // オブジェクトの前面でレイマーチングを開始します
                float3 rayOrigin = i.objectVertex;

                // カメラからオブジェクトサーフェスへのベクトルを使用してレイの方向を取得
                float3 rayDirection = mul(unity_WorldToObject, float4(normalize(i.vectorToSurface), 1));

                float4 color = 0;
                float3 samplePosition = rayOrigin;

                // オブジェクト全体にレイマーチ
                for (int i = 0; i < MAX_STEP_COUNT; i++)
                {
                    // 単位立方体の範囲内でのみ色を累積します
                    if(max(abs(samplePosition.x), max(abs(samplePosition.y), abs(samplePosition.z))) < 0.5f + EPSILON)
                    {
                        const float3 uvw = samplePosition + 0.5f;
                        float4 sampledColor = tex3D(_MainTex, uvw);
                        
                        //const float sampleValue = sampledColor.r * _Alpha;
                        //color += (1.0 - color) * sampleValue;

                        sampledColor.a *= _Alpha;
                        color = BlendUnder(color, sampledColor);
                        
                        samplePosition += rayDirection * _StepSize;
                    }
                }

                //float4 color = _MainColor * value;
                color = float4(_MainColor.rgb * color.r, color.a);
                return color;
            }
            ENDCG
        }
    }
}