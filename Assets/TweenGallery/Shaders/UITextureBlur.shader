// Blurs this Image's texture only, not the screen or its child UI elements.
// Use a standalone, Clamp-wrapped texture. Sampling a packed sprite can read
// neighbouring atlas sprites when the blur radius exceeds its padding.
Shader "TweenGallery/UITextureBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Blur ("Blur Radius (texture pixels)", Range(0,12)) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "False"
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
        // Samples are accumulated in premultiplied-alpha space to avoid dark
        // fringes from the RGB values in fully transparent texture texels.
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "TextureBlur"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                half4 mask : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            float _Blur;

            v2f vert(appdata_t input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
                output.color = input.color * _Color;

                // Match UGUI's RectMask2D soft clipping in canvas-local units.
                float2 pixelSize = output.vertex.w;
                pixelSize /= max(abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy)), float2(0.0001, 0.0001));
                float4 rect = clamp(_ClipRect, -2e10, 2e10);
                output.mask = half4(input.vertex.xy * 2 - rect.xy - rect.zw,
                    0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize)));
                return output;
            }

            half4 ReadPremultiplied(float2 uv)
            {
                half4 sampleColor = tex2D(_MainTex, uv) + _TextureSampleAdd;
                sampleColor.rgb *= sampleColor.a;
                return sampleColor;
            }

            half4 frag(v2f input) : SV_Target
            {
                // A compact nine-tap 2D Gaussian kernel. Radius is specified in
                // source texture pixels, independent of Canvas scale.
                float2 offset = _MainTex_TexelSize.xy * clamp(_Blur, 0.0, 12.0);
                half4 color = ReadPremultiplied(input.texcoord) * 0.25;
                color += ReadPremultiplied(input.texcoord + float2( offset.x, 0)) * 0.125;
                color += ReadPremultiplied(input.texcoord + float2(-offset.x, 0)) * 0.125;
                color += ReadPremultiplied(input.texcoord + float2(0,  offset.y)) * 0.125;
                color += ReadPremultiplied(input.texcoord + float2(0, -offset.y)) * 0.125;
                color += ReadPremultiplied(input.texcoord + float2( offset.x,  offset.y)) * 0.0625;
                color += ReadPremultiplied(input.texcoord + float2( offset.x, -offset.y)) * 0.0625;
                color += ReadPremultiplied(input.texcoord + float2(-offset.x,  offset.y)) * 0.0625;
                color += ReadPremultiplied(input.texcoord + float2(-offset.x, -offset.y)) * 0.0625;

                color.rgb *= input.color.rgb * input.color.a;
                color.a *= input.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                half2 clipMask = saturate((_ClipRect.zw - _ClipRect.xy - abs(input.mask.xy)) * input.mask.zw);
                color *= clipMask.x * clipMask.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
    FallBack Off
}
