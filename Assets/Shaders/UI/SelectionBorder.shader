Shader "UI/SelectionBorder"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Border Color", Color) = (1,1,1,1)
        _BorderThickness ("Border Thickness (UV)", Vector) = (0.02, 0.02, 0, 0)
        _GlowSize ("Glow Size (UV)", Vector) = (0.05, 0.05, 0, 0)
        _GlowColor ("Glow Color", Color) = (1,1,1,0.6)
        _GlowIntensity ("Glow Intensity", Range(0,3)) = 1
        _PulseSpeed ("Pulse Speed", Float) = 3
        _PulseAmount ("Pulse Amount", Range(0,1)) = 0.3

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
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 mask          : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float4 _BorderThickness;
            float4 _GlowSize;
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _PulseSpeed;
            float _PulseAmount;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 dist = min(IN.texcoord, 1 - IN.texcoord);
                float2 borderEdge = _BorderThickness.xy;
                float2 glowEdge = borderEdge + _GlowSize.xy;

                float borderMask = max(
                    step(dist.x, borderEdge.x),
                    step(dist.y, borderEdge.y));

                float glowMask = max(
                    1 - smoothstep(borderEdge.x, glowEdge.x, dist.x),
                    1 - smoothstep(borderEdge.y, glowEdge.y, dist.y));
                glowMask *= (1 - borderMask);

                if (borderMask + glowMask <= 0)
                    return fixed4(0, 0, 0, 0);

                float pulse = 1 - _PulseAmount + _PulseAmount * (0.5 + 0.5 * sin(_Time.y * _PulseSpeed));

                fixed4 color = IN.color * borderMask + _GlowColor * _GlowIntensity * glowMask;
                color.a = (IN.color.a * borderMask + _GlowColor.a * _GlowIntensity * glowMask) * pulse;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
