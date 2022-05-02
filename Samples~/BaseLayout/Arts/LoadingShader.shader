// Loading shader source. Copyright (c) 2022 Mael Lacour. MPL 2.0 license (see https://www.mozilla.org/en-US/MPL/2.0/)

Shader "Eyap/LoadingShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0.0, 0.0, 0.0, 1.0)
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0


        _CircleRadius ("Circle Radius", float) = 4
        _DotCount ("Dot count", int) = 4
        _DotColor ("Dot color", Color) = (1.0, 1.0, 1.0, 1.0)
        _DotSpeed ("Dot Speed", float) = 2
        _DotRadius ("Dot Radius", float) = 4
        _Slow ("Slow", float) = 0.7
        _DotShift ("Dot Shift", float) = 0.01
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                half4   color       : COLOR;
                float2  uv          : TEXCOORD0;
                float3  positionWS  : TEXCOORD1;
                float4  mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            half4 _MainTex_ST;
            float4 _Color;

            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            CBUFFER_START(UnityPerMaterial)
                half _CircleRadius;
                uint _DotCount;
                half4 _DotColor;
                half _DotSpeed;
                half _DotRadius;
                half _Slow;
                half _DotShift;
            CBUFFER_END

            half2 circle(float t, float radius) {
                float x = -radius * cos(t);
                float y = radius * sin(t);
                return float2(x, y);
            }

            bool isInsideCircle(float2 pos, float2 center, float radius)
            {
                float2 v = (pos - center);
                return dot(v, v) < radius * radius;
            }

            Varyings UnlitVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = positionInputs.positionCS;
                o.positionWS = positionInputs.positionWS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float2 pixelSize = v.positionOS.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                o.mask = float4(v.positionOS.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));
                o.color = v.color * _Color;
                return o;
            }

            half4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 color = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                const float2 centerOffset = float2(-0.5, -0.5);
                bool inCircle;
                for (uint dot = 0; dot < _DotCount; dot++)
                {
                    half timeScaled = (_Time.x - dot * _DotShift) * _DotSpeed;
                    half slowEffect = cos(timeScaled) * _Slow;

                    half2 dotPosition = circle(timeScaled - slowEffect, _CircleRadius);
                    inCircle = inCircle || isInsideCircle(i.uv + centerOffset, dotPosition, _DotRadius);
                }

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(i.mask.xy)) * i.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color = lerp(color, _DotColor, inCircle);
                return color;
            }

            ENDHLSL
        }
    }
    Fallback "UI/Default"
}
