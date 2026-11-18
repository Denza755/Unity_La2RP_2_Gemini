Shader "L2/Viewer/SpriteEmitter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _L2FxWorldCalibration ("World Calibration K", Float) = 1.1
        _SizeRange ("Start Size UU Min Max", Vector) = (4, 4, 0, 0)
        _StartLocationRangeUU ("StartLocation half-extents", Vector) = (0, 0, 0, 0)
        _StartLocationRangeXUc ("StartLocation X Min Max", Vector) = (0, 0, 0, 0)
        _StartLocationRangeYUc ("StartLocation Y Min Max", Vector) = (0, 0, 0, 0)
        _StartLocationRangeZUc ("StartLocation Z Min Max", Vector) = (0, 0, 0, 0)
        [Toggle] _UsePolar ("PTLS_Polar", Float) = 0
        _PolarThetaRangeUc ("Polar Theta Min Max", Vector) = (0, 360, 0, 0)
        _PolarPhiRangeUc ("Polar Phi Min Max", Vector) = (0, 0, 0, 0)
        _PolarRadiusRangeUc ("Polar Radius Min Max", Vector) = (0, 0, 0, 0)
        _StartVelocityRangeXUc ("StartVelocity X Min Max", Vector) = (0, 0, 0, 0)
        _StartVelocityRangeYUc ("StartVelocity Y Min Max", Vector) = (0, 0, 0, 0)
        _StartVelocityRangeZUc ("StartVelocity Z Min Max", Vector) = (0, 0, 0, 0)
        _AccelerationUc ("Acceleration UE XYZ", Vector) = (0, 0, 0, 0)
        _SpriteMotionRandStateBits ("appRand State Before Spawn", Float) = 0
        _SpawnDeltaTime ("Spawn Delta Time", Float) = 0.012
        _PtvdMode ("PTVD 0 none 1 StartPosAndOwner 2 OwnerAndStartPos", Float) = 0
        _PtduDirectionMode ("PTDU 0 FaceCamera 1 Normal", Float) = 0
        _StartTime ("Start Time", Float) = 0
        _LifetimeRange ("Lifetime Min Max", Vector) = (1, 1, 0, 0)
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
        _SpriteSpinStartRangeUc ("Start Spin Min Max", Vector) = (0, 0, 0, 0)
        _SpriteSpinSpsRangeUc ("Spin Per Sec Min Max", Vector) = (0, 0, 0, 0)
        _SpriteSpinCcwOrCw ("Spin CCW/CW", Vector) = (1, 1, 1, 0)
        _SpriteSpinRandStateBits ("appRand State Before Spin", Float) = 0
        [Toggle] _UseSizeScale ("Use SizeScale", Float) = 0
        _SizeScaleRepeats ("SizeScale Repeats", Float) = 0
        _SizeKey0 ("Size Key 0 Time Size", Vector) = (0, 1, 0, 0)
        _SizeKey1 ("Size Key 1 Time Size", Vector) = (1, 1, 0, 0)
        _SizeKey2 ("Size Key 2 Time Size", Vector) = (1, 1, 0, 0)
        _TextureUSubdivisions ("Atlas U Cells", Float) = 1
        _TextureVSubdivisions ("Atlas V Cells", Float) = 1
        _SubdivisionStart ("Subdivision Start", Float) = 0
        _SubdivisionEnd ("Subdivision End", Float) = 0
        [Toggle] _UseRandomSubdivision ("UseRandomSubdivision", Float) = 0
        [Toggle] _BlendBetweenSubdivisions ("BlendBetweenSubdivisions", Float) = 0
        _RgbBoost ("RGB Boost (debug, not UC)", Range(0, 16)) = 1
        [Toggle] _L2SpriteColorGammaToLinear ("L2 Color Gamma→Linear", Float) = 1
        _ColorFadeAlphaBlend ("ColorFade AlphaBlend (PTDS 1/2/4)", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 6
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" "L2FxGpuInstancing" = "On" }
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "ViewerSprite"
            Tags { "LightMode" = "UniversalForward" }
            // PTDS_DrawStyle_Reference: Blend line lives in the Pass.
            Blend [_SrcBlend] [_DstBlend]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // EmitterSpawn is pulled only because Decompile_Common ColorFade
            // calls L2Fx_SampleColorScale there (same as verified kirakira).
            #include "../Common/L2FxEmitterSpawn.hlsl"
            #include "../Common/L2FxCoreGeometryTest.hlsl"
            #include "../Common/Decompile_Common/L2FxAppRand.hlsl"
            #include "../Common/Decompile_Common/L2FxSpritePolar.hlsl"
            #include "../Common/Decompile_Common/L2FxStartLocationRange.hlsl"
            #include "../Common/Decompile_Common/L2FxPTVD_OwnerAndStartPosition.hlsl"
            #include "../Common/Decompile_Common/L2FxPTVD_StartPositionAndOwner.hlsl"
            #include "../Common/Decompile_Common/L2FxSpriteMotion.hlsl"
            #include "../Common/Decompile_Common/L2FxSpriteSpin.hlsl"
            #include "../Common/Decompile_Common/L2FxSpriteSizeScale.hlsl"
            #include "../Common/Decompile_Common/L2FxSpriteColorFade.hlsl"
            #include "../Common/Decompile_Common/L2FxSpriteColorGammaLinear.hlsl"
            #include "../Common/Decompile_Common/L2FxPTDS_DrawStyle.hlsl"
            #include "../Common/L2FxFlipbook.hlsl"
            #include "../Common/L2FxSpriteEmitterVertex.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _L2FxWorldCalibration;
                float4 _SizeRange;
                float4 _StartLocationRangeUU;
                float4 _StartLocationRangeXUc;
                float4 _StartLocationRangeYUc;
                float4 _StartLocationRangeZUc;
                float _UsePolar;
                float4 _PolarThetaRangeUc;
                float4 _PolarPhiRangeUc;
                float4 _PolarRadiusRangeUc;
                float4 _StartVelocityRangeXUc;
                float4 _StartVelocityRangeYUc;
                float4 _StartVelocityRangeZUc;
                float4 _AccelerationUc;
                float _SpriteMotionRandStateBits;
                float _SpawnDeltaTime;
                float _PtvdMode;
                float _PtduDirectionMode;
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
                float4 _SpriteSpinStartRangeUc;
                float4 _SpriteSpinSpsRangeUc;
                float4 _SpriteSpinCcwOrCw;
                float _SpriteSpinRandStateBits;
                float _UseSizeScale;
                float _SizeScaleRepeats;
                float4 _SizeKey0;
                float4 _SizeKey1;
                float4 _SizeKey2;
                float _TextureUSubdivisions;
                float _TextureVSubdivisions;
                float _SubdivisionStart;
                float _SubdivisionEnd;
                float _UseRandomSubdivision;
                float _BlendBetweenSubdivisions;
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
                float2 uvAtlas : TEXCOORD0;
                float2 uvBlend : TEXCOORD1;
                nointerpolation float blend : TEXCOORD2;
                nointerpolation float ageSeconds : TEXCOORD3;
                nointerpolation float lifetimeSeconds : TEXCOORD4;
                nointerpolation float seed : TEXCOORD5;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                uint spawnState = asuint(_SpriteMotionRandStateBits);
                float3 rawVelocityUe = L2Fx_FRangeVector_GetRandYawPitchRoll(
                    _StartVelocityRangeXUc.xy, _StartVelocityRangeYUc.xy, _StartVelocityRangeZUc.xy, spawnState);
                float3 spawnPositionUe = 0;
                if (_UsePolar > 0.5)
                {
                    spawnPositionUe = L2Fx_SpritePolar_GetRandUe(
                        _PolarThetaRangeUc.xy, _PolarPhiRangeUc.xy, _PolarRadiusRangeUc.xy, spawnState);
                }
                else
                {
                    spawnPositionUe = L2Fx_StartLocationRange_GetRandUe(
                        _StartLocationRangeXUc.xy, _StartLocationRangeYUc.xy, _StartLocationRangeZUc.xy, spawnState);
                }

                float lifetimeSeconds = L2Fx_FRange_GetRand(_LifetimeRange.xy, spawnState);
                float spawnSizeUU = L2Fx_FRange_GetRand(_SizeRange.xy, spawnState);
                float3 velocityBeforePtvdUe = rawVelocityUe + _AccelerationUc.xyz * _SpawnDeltaTime;
                float3 velocityUe = velocityBeforePtvdUe;
                if (_PtvdMode > 1.5)
                    velocityUe = L2FxPTVD_OwnerAndStartPosition(velocityBeforePtvdUe, spawnPositionUe, float3(0, 0, 0));
                else if (_PtvdMode > 0.5)
                    velocityUe = L2FxPTVD_StartPositionAndOwner(velocityBeforePtvdUe, spawnPositionUe, float3(0, 0, 0));

                float delay = L2Fx_RandomInitialDelay(_InitialDelayRange.xy, _SpriteMotionRandStateBits, _StartTime, 3.0);
                float ageSeconds = max(0.0, _Time.y - _StartTime - delay);
                float ageNorm = saturate(ageSeconds / max(lifetimeSeconds, 1e-4));
                bool implicitZero = _SizeKey0.x <= 0.0001;
                float sizeMul = L2Fx_SpriteSizeScale_ScalarFromUniforms(
                    ageNorm, _UseSizeScale, _SizeScaleRepeats, 3u, implicitZero, false,
                    _SizeKey0.x, _SizeKey0.y, _SizeKey1.x, _SizeKey1.y, _SizeKey2.x, _SizeKey2.y, 1.0, 1.0, 1.0, 1.0);
                float sizeM = L2Fx_GetFinalVertexSizeMeters(spawnSizeUU * sizeMul, _L2FxWorldCalibration);

                float3 spawnOffsetOS = L2Fx_UcPositionToUnityMeters(spawnPositionUe, _L2FxWorldCalibration);
                float3 motionOffsetOS = L2Fx_SpriteMotion_DisplacementUnityCalibrated(
                    velocityUe, _AccelerationUc.xyz, ageSeconds, 1.0, _L2FxWorldCalibration);

                float startSpin;
                float spinsPerSecond;
                L2Fx_SpriteSpin_SpawnSlotFloatsFromAppRandState(
                    _SpriteSpinStartRangeUc.xy, _SpriteSpinSpsRangeUc.xy, _SpriteSpinCcwOrCw.xyz,
                    asuint(_SpriteSpinRandStateBits), startSpin, spinsPerSecond);
                float2 rotatedQuad = L2Fx_SpriteSpin_RotateBillboardOffset(
                    IN.positionOS.xy * sizeM,
                    L2Fx_SpriteSpin_EvaluateRadians(startSpin, spinsPerSecond, ageSeconds));
                float3 centerWS = TransformObjectToWorld(spawnOffsetOS + motionOffsetOS);
                float3 positionWS;
                if (_PtduDirectionMode > 0.5)
                {
                    // PTDU_Normal: quad lies on the particle normal plane.
                    // Default Normal = UE (0,0,1) → Unity +Y → ground flash (wh_shield_ta).
                    positionWS = L2Fx_PtduNormalPositionWS(
                        centerWS,
                        float3(rotatedQuad.x, rotatedQuad.y, 0.0),
                        1.0,
                        float3(0.0, 0.0, 0.0));
                }
                else
                {
                    float3 toCamera = _WorldSpaceCameraPos.xyz - centerWS;
                    toCamera = dot(toCamera, toCamera) > 1e-8 ? normalize(toCamera) : float3(0, 1, 0);
                    float3 rightWS = normalize(cross(float3(0, 1, 0), toCamera));
                    float3 upWS = normalize(cross(toCamera, rightWS));
                    positionWS = centerWS + rightWS * rotatedQuad.x + upWS * rotatedQuad.y;
                }
                OUT.positionHCS = TransformWorldToHClip(positionWS);

                int uSub = max(1, (int)_TextureUSubdivisions);
                int vSub = max(1, (int)_TextureVSubdivisions);
                int lo = min((int)_SubdivisionStart, (int)_SubdivisionEnd);
                int hi = max((int)_SubdivisionStart, (int)_SubdivisionEnd);
                float2 uv = IN.uv;
                float blend = 0.0;
                float2 uvB = uv;
                if (uSub * vSub > 1)
                {
                    if (_UseRandomSubdivision > 0.5)
                    {
                        int frame = L2Fx_FlipbookSubDivisionRandomFrame(_SpriteMotionRandStateBits, _StartTime, lo, hi, 19.0);
                        uv = L2Fx_FlipbookAtlasUV(IN.uv, frame, uSub, vSub);
                    }
                    else if (_BlendBetweenSubdivisions > 0.5)
                    {
                        int fa, fb;
                        L2Fx_FlipbookBlendFrames(ageNorm, lo, hi, fa, fb, blend);
                        uv = L2Fx_FlipbookAtlasUV(IN.uv, fa, uSub, vSub);
                        uvB = L2Fx_FlipbookAtlasUV(IN.uv, fb, uSub, vSub);
                    }
                    else
                    {
                        int frame = L2Fx_FlipbookFrameIndex(ageNorm, lo, hi);
                        uv = L2Fx_FlipbookAtlasUV(IN.uv, frame, uSub, vSub);
                    }
                }
                OUT.uvAtlas = uv;
                OUT.uvBlend = uvB;
                OUT.blend = blend;
                OUT.ageSeconds = ageSeconds;
                OUT.lifetimeSeconds = lifetimeSeconds;
                OUT.seed = _SpriteMotionRandStateBits;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uvAtlas);
                if (IN.blend > 0.0)
                {
                    half4 texB = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uvBlend);
                    tex = lerp(tex, texB, (half)IN.blend);
                }
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
