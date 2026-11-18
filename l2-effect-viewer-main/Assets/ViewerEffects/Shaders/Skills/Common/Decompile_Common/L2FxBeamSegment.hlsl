#ifndef L2_FX_BEAM_SEGMENT_INCLUDED
#define L2_FX_BEAM_SEGMENT_INCLUDED

// UBeamEmitter strip expansion for MVP (PTEP_Offset, LF/HF without noise).
//
// SOURCE: UBeamEmitter::SpawnParticle endpoint switch (this+1252) +
//         RenderParticles FBeamVertex sheets (camera-facing pair per HF point).
// CAPTURE: decompile/mesh_csv_beamEmitter.csv — UV.x along beam, UV.y edge 0/1.
//
// Unit mesh from ViewerBeamBuilder:
//   positionOS.z = t along beam in [0,1]
//   positionOS.x = across in [-0.5, 0.5]
//
// World position:
//   axisPoint = lerp(startWS, endWS, t)
//   right     = normalize(cross(beamDir, toCamera))   // camera-facing sheet
//   pos       = axisPoint + right * across * widthMeters
//
// PTEP_Offset (case 2): endUe = startUe + FRangeVector::GetRand(BeamEndPoints[i].Offset).

#include "L2FxAppRand.hlsl"

float3 L2Fx_Beam_PtepOffsetEndUe(float3 startUe, float2 offsetX, float2 offsetY, float2 offsetZ, inout uint state)
{
    float3 offsetUe = L2Fx_FRangeVector_GetRandYawPitchRoll(offsetX, offsetY, offsetZ, state);
    return startUe + offsetUe;
}

// Half-width in Unity meters from StartSizeRange.X (UU) and world calibration K.
float L2Fx_Beam_HalfWidthMeters(float sizeUU, float worldCalibK)
{
    float k = worldCalibK > 0.0 ? worldCalibK : 1.4;
    return 0.5 * sizeUU * (1.0 / 52.5) * k;
}

// Camera-facing beam sheet: across in [-0.5,0.5], t in [0,1].
float3 L2Fx_Beam_BillboardPointWS(
    float3 startWS,
    float3 endWS,
    float t,
    float across,
    float widthMeters,
    float3 cameraWS)
{
    float3 axis = endWS - startWS;
    float axisLenSq = dot(axis, axis);
    float3 beamDir = axisLenSq > 1e-12 ? axis * rsqrt(axisLenSq) : float3(0, 1, 0);
    float3 axisPoint = lerp(startWS, endWS, saturate(t));

    float3 toCam = cameraWS - axisPoint;
    float toCamLenSq = dot(toCam, toCam);
    float3 viewDir = toCamLenSq > 1e-12 ? toCam * rsqrt(toCamLenSq) : float3(0, 0, -1);

    float3 right = cross(beamDir, viewDir);
    float rightLenSq = dot(right, right);
    if (rightLenSq < 1e-12)
    {
        // Beam nearly toward camera — fall back to world up cross.
        right = cross(beamDir, float3(0, 1, 0));
        rightLenSq = dot(right, right);
        if (rightLenSq < 1e-12)
            right = float3(1, 0, 0);
        else
            right *= rsqrt(rightLenSq);
    }
    else
    {
        right *= rsqrt(rightLenSq);
    }

    return axisPoint + right * (across * widthMeters);
}

#endif
