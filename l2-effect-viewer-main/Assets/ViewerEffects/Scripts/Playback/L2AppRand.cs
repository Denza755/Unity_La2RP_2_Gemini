using System;
using UnityEngine;

public static class L2AppRand
{
    public const uint Multiplier = 214013u;
    public const uint Increment = 2531011u;
    public const int MeshSpawnSlotToSlotDrawCount = 31;
    public const int MeshSpawnDrawsBeforeStartSpin = 22;
    public const int SpriteMotionSlotStride = 28;
    public const int SpriteSpinDraws = 22;

    public static uint Advance(uint state, int drawCount)
    {
        if (drawCount <= 0)
            return state;

        uint mul = Multiplier;
        uint add = Increment;
        uint a = 1u;
        uint c = 0u;
        int n = drawCount;
        while (n > 0)
        {
            if ((n & 1) != 0)
            {
                c = unchecked(mul * c + add);
                a = unchecked(mul * a);
            }

            add = unchecked(mul * add + add);
            mul = unchecked(mul * mul);
            n >>= 1;
        }

        return unchecked(a * state + c);
    }

    public static float BitsToFloat(uint state)
    {
        return BitConverter.Int32BitsToSingle(unchecked((int)state));
    }

    public static void ResolveGpuInstanceRandBits(
        bool hasMeshSpawn,
        bool hasStartSpin,
        uint meshEmitter3AppRandBaseState,
        uint spriteEmitterAppRandBaseState,
        int slotIndex,
        out float meshSpawnRandBits,
        out float startSpinRandBits,
        out float spriteMotionRandBits,
        out float spriteSpinRandBits)
    {
        meshSpawnRandBits = 0f;
        startSpinRandBits = 0f;
        uint spriteMotion = Advance(spriteEmitterAppRandBaseState, slotIndex * SpriteMotionSlotStride);
        spriteMotionRandBits = BitsToFloat(spriteMotion);
        spriteSpinRandBits = BitsToFloat(Advance(spriteMotion, SpriteSpinDraws));

        if (hasMeshSpawn)
        {
            uint velocityState = Advance(
                meshEmitter3AppRandBaseState,
                slotIndex * MeshSpawnSlotToSlotDrawCount);
            meshSpawnRandBits = BitsToFloat(velocityState);
            startSpinRandBits = BitsToFloat(
                Advance(velocityState, MeshSpawnDrawsBeforeStartSpin));
            return;
        }

        if (hasStartSpin)
        {
            startSpinRandBits = BitsToFloat(
                Advance(
                    meshEmitter3AppRandBaseState,
                    slotIndex * MeshSpawnSlotToSlotDrawCount));
        }
    }
}
