Shader "L2/Viewer/BeamEmitter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _L2FxWorldCalibration ("World Calibration K", Float) = 1.4
        _SizeRange ("Start Size UU Min Max", Vector) = (6, 10, 0, 0)
        _StartLocationOffsetUc ("StartLocationOffset UE", Vector) = (0, 0, 0, 0)
        _StartLocationRangeXUc ("StartLocation X Min Max", Vector) = (0, 0, 0, 0)
        _StartLocationRangeYUc ("StartLocation Y Min Max", Vector) = (0, 0, 0, 0)
        _StartLocationRangeZUc ("StartLocation Z Min Max", Vector) = (0, 0, 0, 0)
        [Toggle] _UsePolar ("PTLS_Polar", Float) = 0
        _PolarThetaRangeUc ("Polar Theta Min Max", Vector) = (0, 360, 0, 0)
        _PolarPhiRangeUc ("Polar Phi Min Max", Vector) = (0, 0, 0, 0)
        _PolarRadiusRangeUc ("Polar Radius Min Max", Vector) = (0, 0, 0, 0)
        _BeamEndOffsetXUc ("BeamEnd Offset X", Vector) = (0, 0, 0, 0)
        _BeamEndOffsetYUc ("BeamEnd Offset Y", Vector) = (0, 0, 0, 0)
        _BeamEndOffsetZUc ("BeamEnd Offset Z", Vector) = (-190, -190, 0, 0)
        _BeamEndpointMode ("0 Offset 1 Distance 2 Absolute", Float) = 0
        _BeamDistanceRangeUc ("BeamDistance Min Max", Vector) = (0, 0, 0, 0)
        _SpriteMotionRandStateBits ("appRand State Before Spawn", Float) = 0
        _StartTime ("Start Time", Float) = 0
        _LifetimeRange ("Lifetime Min Max", Vector) = (2, 3, 0, 0)
        _InitialDelayRange ("Initial Delay Min Max", Vector) = (0, 0, 0, 0)
        [Toggle] _FadeIn ("Fade In", Float) = 0
        _FadeInEndTime ("Fade In End", Float) = 0
        [Toggle] _Fadeout ("Fade Out", Float) = 0
        _FadeoutStartTime ("Fade Out Start", Float) = 0
        _ColorScaleCount ("Color Scale Keys", Float) = 2
        _ColorScaleParam ("Color Scale Repeats", Float) = 0
        _ColorKey0 ("Color Scale 0", Color) = (1, 1, 1, 1)
        _ColorKey1Time ("Color Scale 1 Time", Float) = 1
        _ColorKey1 ("Color Scale 1", Color) = (1, 1, 1, 1)
        _ColorKey2Time ("Color Scale 2 Time", Float) = 1
        _ColorKey2 ("Color Scale 2", Color) = (1, 1, 1, 1)
        _ColorMulMin ("ColorMultiplier Min RGB", Vector) = (1, 1, 1, 0)
        _ColorMulMax ("ColorMultiplier Max RGB", Vector) = (1, 1, 1, 0)
        _Opacity ("Opacity", Range(0, 2)) = 1
        _RgbBoost ("RGB Boost (debug, not UC)", Range(0, 16)) = 1
        [Toggle] _L2SpriteColorGammaToLinear ("L2 Color Gamma→Linear", Float) = 1
        _ColorFadeAlphaBlend ("ColorFade AlphaBlend (PTDS 1/2/4)", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "ViewerBeam"
            Tags { "LightMode" = "UniversalForward" }
            Blend [_SrcBlend] [_DstBlend]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../Common/L2FxEmitterSpawn.hlsl"
            #include "../Common/L2FxCoreGeometryTest.hlsl"
            #include "../Common/Decompile_Common/L2FxAppRand.hlsl"
            #include "../Common/Decompile_Common/L2FxSpritePolar.hlsl"
            #include "../Common/Decompile_Common/L2FxStartLocationRange.hlsl"
            #include "../Common/Decompile_Common/L2FxBeamSegment.hlsl"
            #include "../Common/Decompile_Common/L2FxSpriteColorFade.hlsl"
            #include "../Common/Decompile_Common/L2FxSpriteColorGammaLinear.hlsl"
            #include "../Common/Decompile_Common/L2FxPTDS_DrawStyle.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _L2FxWorldCalibration;
                float4 _SizeRange;
                float4 _StartLocationOffsetUc;
                float4 _StartLocationRangeXUc;
                float4 _StartLocationRangeYUc;
                float4 _StartLocationRangeZUc;
                float _UsePolar;
                float4 _PolarThetaRangeUc;
                float4 _PolarPhiRangeUc;
                float4 _PolarRadiusRangeUc;
                float4 _BeamEndOffsetXUc;
                float4 _BeamEndOffsetYUc;
                float4 _BeamEndOffsetZUc;
                float _BeamEndpointMode;
                float4 _BeamDistanceRangeUc;
                float _SpriteMotionRandStateBits;
                float _StartTime;
                float4 _LifetimeRange;
                float4 _InitialDelayRange;
                float _FadeIn;
                float _FadeInEndTime;
                float _Fadeout;
                float _FadeoutStartTime;
                float _ColorScaleCount;
                float _ColorScaleParam;
                float4 _ColorKey0;
                float _ColorKey1Time;
                float4 _ColorKey1;
                float _ColorKey2Time;
                float4 _ColorKey2;
                float4 _ColorMulMin;
                float4 _ColorMulMax;
                float _Opacity;
                float _RgbBoost;
                float _L2SpriteColorGammaToLinear;
                float _ColorFadeAlphaBlend;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                nointerpolation float ageSeconds : TEXCOORD1;
                nointerpolation float lifetimeSeconds : TEXCOORD2;
                nointerpolation float seed : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                uint spawnState = asuint(_SpriteMotionRandStateBits);

                float3 spawnLocalUe = 0;
                if (_UsePolar > 0.5)
                {
                    spawnLocalUe = L2Fx_SpritePolar_GetRandUe(
                        _PolarThetaRangeUc.xy, _PolarPhiRangeUc.xy, _PolarRadiusRangeUc.xy, spawnState);
                }
                else
                {
                    spawnLocalUe = L2Fx_StartLocationRange_GetRandUe(
                        _StartLocationRangeXUc.xy, _StartLocationRangeYUc.xy, _StartLocationRangeZUc.xy, spawnState);
                }
                float3 startUe = spawnLocalUe + _StartLocationOffsetUc.xyz;

                float3 endUe = startUe;
                if (_BeamEndpointMode < 0.5)
                {
                    // PTEP_Offset
                    endUe = L2Fx_Beam_PtepOffsetEndUe(
                        startUe,
                        _BeamEndOffsetXUc.xy, _BeamEndOffsetYUc.xy, _BeamEndOffsetZUc.xy,
                        spawnState);
                }
                else if (_BeamEndpointMode < 1.5)
                {
                    // PTEP_Distance — along +Z local if no velocity; heal uses Offset.
                    float dist = L2Fx_FRange_GetRand(_BeamDistanceRangeUc.xy, spawnState);
                    endUe = startUe + float3(0, 0, -dist);
                }
                else
                {
                    // PTEP_OffsetAsAbsolute — offset is world/local absolute end.
                    float3 absEnd = L2Fx_FRangeVector_GetRandYawPitchRoll(
                        _BeamEndOffsetXUc.xy, _BeamEndOffsetYUc.xy, _BeamEndOffsetZUc.xy, spawnState);
                    endUe = absEnd;
                }

                float lifetimeSeconds = L2Fx_FRange_GetRand(_LifetimeRange.xy, spawnState);
                float sizeUU = L2Fx_FRange_GetRand(_SizeRange.xy, spawnState);
                float delay = L2Fx_RandomInitialDelay(_InitialDelayRange.xy, _SpriteMotionRandStateBits, _StartTime, 3.0);
                float ageSeconds = max(0.0, _Time.y - _StartTime - delay);

                float3 startOS = L2Fx_UcPositionToUnityMeters(startUe, _L2FxWorldCalibration);
                float3 endOS = L2Fx_UcPositionToUnityMeters(endUe, _L2FxWorldCalibration);
                float3 startWS = TransformObjectToWorld(startOS);
                float3 endWS = TransformObjectToWorld(endOS);
                float widthM = L2Fx_Beam_HalfWidthMeters(sizeUU, _L2FxWorldCalibration) * 2.0;

                float t = IN.positionOS.z;
                float across = IN.positionOS.x;
                float3 positionWS = L2Fx_Beam_BillboardPointWS(
                    startWS, endWS, t, across, widthM, _WorldSpaceCameraPos.xyz);

                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.uv = IN.uv;
                OUT.ageSeconds = ageSeconds;
                OUT.lifetimeSeconds = lifetimeSeconds;
                OUT.seed = _SpriteMotionRandStateBits;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float4 colorFade = L2Fx_SpriteColorFade_FullKeys(
                    (uint)_ColorScaleCount, _ColorScaleParam, _ColorFadeAlphaBlend,
                    _ColorKey0, _ColorKey1Time, _ColorKey1,
                    _ColorKey2Time, _ColorKey2, 1.0, _ColorKey2,
                    _ColorMulMin.xyz, _ColorMulMax.xyz,
                    IN.ageSeconds, IN.lifetimeSeconds, 1.0,
                    _FadeIn, _FadeInEndTime, _Fadeout, _FadeoutStartTime,
                    _Opacity, 1.0, IN.seed, _StartTime);
                colorFade = L2Fx_SpriteColor_ApplyGammaToLinearIfEnabled(colorFade, _L2SpriteColorGammaToLinear);
                half4 lit = tex * (half4)colorFade;
                lit.rgb *= (half)_RgbBoost;
                return lit;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
