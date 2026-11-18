Shader "L2/Viewer/MeshEmitter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StartTime ("Start Time", Float) = 0
        _Seed ("Seed", Float) = 0
        _InitialDelayRange ("Initial Delay Min Max", Vector) = (0, 0, 0, 0)
        _LifetimeRange ("Lifetime Min Max", Vector) = (1, 1, 0, 0)
        [Toggle] _UseManualAge ("Use Manual Age", Float) = 0
        _ManualAge ("Manual Age sec", Range(0, 2)) = 0
        _L2FxWorldCalibration ("World Calibration K", Float) = 1.8
        _StartSizeXY ("StartSize X/Y", Float) = 0.132
        _StartSize ("StartSize uniform", Float) = 0.132
        _StartSizeZRange ("StartSize Z Min Max", Vector) = (0, 0, 0, 0)
        [Toggle] _UseSizeScale ("Use SizeScale", Float) = 1
        _SizeKey0 ("SizeScale 0 Time Size", Vector) = (0, 1, 0, 0)
        _SizeKey1 ("SizeScale 1 Time Size", Vector) = (1, 1, 0, 0)
        _SizeKey2 ("SizeScale 2 Time Size", Vector) = (1, 1, 0, 0)
        _SizeKey3 ("SizeScale 3 Time Size", Vector) = (1, 1, 0, 0)
        _SizeKey4 ("SizeScale 4 Time Size", Vector) = (1, 1, 0, 0)
        _StartLocationZRangeUU ("StartLocation Z Min Max UU", Vector) = (0, 0, 0, 0)
        _StartVelocityZRangeUU ("StartVelocity Z Min Max UU", Vector) = (0, 0, 0, 0)
        _MeshSpawnRandStateBits ("appRand TLS before StartVelocity", Float) = 0
        [Toggle] _UsePtrsActor ("UseRotationFrom PTRS_Actor", Float) = 0
        [Toggle] _SpinParticles ("SpinParticles", Float) = 0
        _StartSpinYawRangeUc ("StartSpin X / Yaw", Vector) = (0, 0, 0, 0)
        _StartSpinPitchRangeUc ("StartSpin Y / Pitch", Vector) = (0, 0, 0, 0)
        _StartSpinRollRangeUc ("StartSpin Z / Roll", Vector) = (0, 0, 0, 0)
        _SpsYawPitchRollUc ("SpinsPerSecond c0 c1 c2", Vector) = (0, 0, 0, 0)
        _SpinCCWorCW ("SpinCCWorCW X Y Z", Vector) = (0, 0, 0, 0)
        _StartSpinRandStateBits ("appRand TLS before StartSpin", Float) = 0
        _ColorMultiplier ("ColorMultiplier RGB", Vector) = (1, 1, 1, 0)
        _ColorKey0 ("ColorScale 0", Color) = (1, 1, 1, 1)
        _ColorKey1 ("ColorScale 1", Color) = (1, 1, 1, 1)
        [Toggle] _FadeIn ("FadeIn", Float) = 0
        _FadeInEndTime ("FadeIn End sec", Float) = 0
        [Toggle] _FadeOut ("FadeOut", Float) = 0
        _FadeOutStartTime ("FadeOut Start sec", Float) = 0
        _Opacity ("Opacity", Range(0, 2)) = 1
        _RgbBoost ("RGB Boost (debug, not UC)", Range(0, 16)) = 1
        [Toggle] _L2SpriteColorGammaToLinear ("L2 Color Gamma→Linear", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 6
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" "L2FxGpuInstancing" = "On" }
        Cull Off
        ZWrite Off

        Pass
        {
            Name "ViewerMesh"
            Tags { "LightMode" = "UniversalForward" }
            // PTDS_DrawStyle_Reference: written in the Pass. Potion = Brighten.
            // 0 One/Zero  1 SrcAlpha/OneMinusSrcAlpha  2 DstColor/SrcColor
            // 3 One/One   4 One/OneMinusSrcAlpha       5 Zero/OneMinusSrcColor
            // 6 One/OneMinusSrcColor
            Blend [_SrcBlend] [_DstBlend]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../Common/L2FxCoreGeometryTest.hlsl"
            #include "../Common/Decompile_Common/L2FxMeshSizeScale.hlsl"
            #include "../Common/Decompile_Common/L2FxMeshColorFade.hlsl"
            #include "../Common/Decompile_Common/L2FxSpriteColorGammaLinear.hlsl"
            #include "../Common/Decompile_Common/L2FxMeshSpin.hlsl"
            #include "../Common/Decompile_Common/L2FxPTRS_Actor.hlsl"
            #include "../Common/Decompile_Common/L2FxMeshMotion.hlsl"
            #include "../Common/Decompile_Common/L2FxMeshSpawnParticle.hlsl"
            #include "../Common/Decompile_Common/L2FxPTDS_DrawStyle.hlsl"
            #include "../Common/L2FxMeshParticleMotion.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _StartTime;
                float _Seed;
                float4 _InitialDelayRange;
                float4 _LifetimeRange;
                float _UseManualAge;
                float _ManualAge;
                float _L2FxWorldCalibration;
                float _StartSizeXY;
                float _StartSize;
                float4 _StartSizeZRange;
                float _UseSizeScale;
                float4 _SizeKey0;
                float4 _SizeKey1;
                float4 _SizeKey2;
                float4 _SizeKey3;
                float4 _SizeKey4;
                float4 _StartLocationZRangeUU;
                float4 _StartVelocityZRangeUU;
                float _MeshSpawnRandStateBits;
                float _UsePtrsActor;
                float _SpinParticles;
                float4 _StartSpinYawRangeUc;
                float4 _StartSpinPitchRangeUc;
                float4 _StartSpinRollRangeUc;
                float4 _SpsYawPitchRollUc;
                float4 _SpinCCWorCW;
                float _StartSpinRandStateBits;
                float4 _ColorMultiplier;
                float4 _ColorKey0;
                float4 _ColorKey1;
                float _FadeIn;
                float _FadeInEndTime;
                float _FadeOut;
                float _FadeOutStartTime;
                float _Opacity;
                float _RgbBoost;
                float _L2SpriteColorGammaToLinear;
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
                half4 color : COLOR0;
            };

            float ResolveAgeSeconds(out float lifetime)
            {
                lifetime = max(L2Fx_RandomLifetime(_LifetimeRange.xy, _Seed, _StartTime, 7.0), 1e-4);
                if (_UseManualAge > 0.5)
                    return _ManualAge;
                float delay = L2Fx_RandomInitialDelay(_InitialDelayRange.xy, _Seed, _StartTime, 3.0);
                return _StartTime > 0.0 ? L2Fx_AgeSeconds(_Time.y, _StartTime, delay) : max(0.0, _Time.y - delay);
            }

            float ResolveSizeScale(float ageNorm)
            {
                return L2Fx_MeshSizeScale_ScalarFromKeys5(
                    ageNorm, _UseSizeScale, 0.0, 1.0, 0.0, 5,
                    _SizeKey0.x, _SizeKey0.y,
                    _SizeKey1.x, _SizeKey1.y,
                    _SizeKey2.x, _SizeKey2.y,
                    _SizeKey3.x, _SizeKey3.y,
                    _SizeKey4.x, _SizeKey4.y);
            }

            float4 ResolveColor(float ageSeconds, float lifetime)
            {
                float4 color = L2Fx_MeshColorFade_FullKeys6(
                    ageSeconds, lifetime, 0.0, _ColorMultiplier.xyz,
                    _FadeIn, _FadeInEndTime, _FadeOut, _FadeOutStartTime, _Opacity,
                    _ColorKey0, 1.0, _ColorKey1, 1.0, _ColorKey1, 1.0, _ColorKey1, 1.0, _ColorKey1, 1.0, _ColorKey1);
                return L2Fx_SpriteColor_ApplyGammaToLinearIfEnabled(color, _L2SpriteColorGammaToLinear);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float lifetime;
                float ageSeconds = ResolveAgeSeconds(lifetime);
                float ageNorm = saturate(ageSeconds / lifetime);
                float sizeScale = ResolveSizeScale(ageNorm);

                float startSizeZ = _StartSize;
                float startLocZ = 0.0;
                float startVelZ = 0.0;
                uint meshSpawnState = asuint(_MeshSpawnRandStateBits);
                bool ptrs = _UsePtrsActor > 0.5;
                if (!ptrs && meshSpawnState != 0u)
                {
                    L2Fx_MeshSpawnParticle_SampleLocVelSizeZ(
                        _StartLocationZRangeUU.xy, _StartVelocityZRangeUU.xy, _StartSizeZRange.xy,
                        float2(_StartSizeXY, _StartSizeXY),
                        float2(_ColorMultiplier.x, _ColorMultiplier.x),
                        float2(_ColorMultiplier.y, _ColorMultiplier.y),
                        float2(_ColorMultiplier.z, _ColorMultiplier.z),
                        _LifetimeRange.xy, _InitialDelayRange.xy, float2(1.0, 1.0),
                        meshSpawnState, startLocZ, startVelZ, startSizeZ);
                }
                else if (!ptrs)
                {
                    startSizeZ = L2Fx_RandomRange(_StartSizeZRange.xy, _Seed, _StartTime, 11.0);
                    startLocZ = L2Fx_RandomRange(_StartLocationZRangeUU.xy, _Seed, _StartTime, 13.0);
                    startVelZ = L2Fx_RandomRange(_StartVelocityZRangeUU.xy, _Seed, _StartTime, 17.0);
                }

                float scaleXY = L2Fx_GetFinalMeshScale(ptrs ? _StartSize : _StartSizeXY, sizeScale, _L2FxWorldCalibration);
                float scaleZ = L2Fx_GetFinalMeshScale(ptrs ? _StartSize : startSizeZ, sizeScale, _L2FxWorldCalibration);
                float3 localMeshOS = IN.positionOS.xyz;

                uint appRandState = asuint(_StartSpinRandStateBits);
                float3 startSpinC012 = appRandState != 0u
                    ? L2Fx_MeshSpin_StartYawPitchRollUruFromAppRandState(
                        _StartSpinYawRangeUc.xy, _StartSpinPitchRangeUc.xy, _StartSpinRollRangeUc.xy, appRandState)
                    : L2Fx_MeshSpin_StartYawPitchRollUruFromMappedRanges(
                        _StartSpinYawRangeUc.xy, _StartSpinPitchRangeUc.xy, _StartSpinRollRangeUc.xy, _Seed, _StartTime);
                float3 directionSign = float3(
                    _SpinCCWorCW.x == 0.0 ? -1.0 : 1.0,
                    _SpinCCWorCW.y == 0.0 ? -1.0 : 1.0,
                    _SpinCCWorCW.z == 0.0 ? -1.0 : 1.0);
                if (ptrs)
                    directionSign = float3(1.0, 1.0, 1.0);
                float3 spinRateC012 = _SpinParticles > 0.5
                    ? L2Fx_MeshSpin_VelocityYawPitchRollUruPerSecond(_SpsYawPitchRollUc.xyz, directionSign)
                    : float3(0, 0, 0);

                if (ptrs)
                {
                    // PTRS_Actor_Reference: GameObject already has owner TRS →
                    // L2FxPTRSActor_TransformLocalMeshUnity = S(FinalSize)*R(Spin), UE (X,Y,Z).
                    float3 finalSizeUe = float3(scaleXY, scaleXY, scaleXY);
                    localMeshOS = L2FxPTRSActor_TransformLocalMeshUnity(
                        IN.positionOS.xyz, finalSizeUe, spinRateC012, startSpinC012, ageSeconds);
                }
                else
                {
                    // Unity mesh is UE(X,Z,Y): ground rings lie on XZ (thin Y).
                    localMeshOS = IN.positionOS.xyz * float3(scaleXY, scaleZ, scaleXY);
                    if (_SpinParticles > 0.5)
                    {
                        float3 yawPitchRollUru = L2Fx_MeshSpin_EvaluateYawPitchRollUru(
                            startSpinC012, spinRateC012, ageSeconds);

                        // UC X-only spin (magiccircle / Ring): same as ShieldTaMesh —
                        // rotate around Unity +Y so the ground plane stays horizontal.
                        // Full FRotator is kept when Pitch/Roll ranges or SPS are used.
                        bool yawOnly =
                            abs(_StartSpinPitchRangeUc.x) + abs(_StartSpinPitchRangeUc.y) +
                            abs(_StartSpinRollRangeUc.x) + abs(_StartSpinRollRangeUc.y) +
                            abs(_SpsYawPitchRollUc.y) + abs(_SpsYawPitchRollUc.z) < 1e-5;
                        if (yawOnly)
                        {
                            float yawRadians =
                                yawPitchRollUru.x * (6.28318530718 / 65536.0);
                            float3 nrmOS = float3(0, 1, 0);
                            L2Fx_ApplyMeshSpinAroundY(localMeshOS, nrmOS, true, yawRadians);
                        }
                        else
                        {
                            float3 pitchYawRollRadians = L2Fx_MeshSpin_YawPitchRollToPitchYawRoll(
                                L2Fx_MeshSpin_YawPitchRollUruToRadians(yawPitchRollUru));
                            localMeshOS = L2Fx_MeshSpin_RotateUnityLocalPositionPitchYawRoll(
                                localMeshOS, pitchYawRollRadians);
                        }
                    }
                }

                float3 motionOS = 0;
                if (!ptrs)
                {
                    float3 locUe = L2Fx_MeshMotion_EvaluatePositionUe(
                        float3(0.0, 0.0, startLocZ), float3(0.0, 0.0, startVelZ), float3(0.0, 0.0, 0.0), ageSeconds);
                    motionOS = L2Fx_UcPositionToUnityMeters(locUe, _L2FxWorldCalibration);
                }

                float3 positionWS = TransformObjectToWorld(motionOS + localMeshOS);
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.uv = IN.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                float visible = (_UseManualAge > 0.5 || (ageSeconds >= 0.0 && ageSeconds < lifetime)) ? 1.0 : 0.0;
                OUT.color = (half4)(ResolveColor(ageSeconds, lifetime) * visible);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // PTDS_AlphaBlend README + live FF: out = tex * color. No luma, no forced A=1.
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                return tex * IN.color * half4((half3)_RgbBoost, 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
