using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SkinnedMeshSync : MonoBehaviour
{
    [SerializeField] private Transform _bodyPartsContainer;
    [SerializeField] private SkinnedMeshRenderer _rootSkinnedRenderer;
    [SerializeField] private SkinnedMeshRenderer[] _destSkinnedRenderer;
    [SerializeField] private Transform _rootBone;

    public Transform RootBone { get { return _rootBone; } }

    private static readonly string[] OrcHead1Bone = new string[] {
        "Bip01_head"
    };

    private static readonly string[] OrcHead7Bones = new string[] {
        "Bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head"
    };

    private static readonly string[] OrcHead9Bones = new string[] {
        "Bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_L_Clavicle", "Bip01_R_Clavicle"
    };

    private static readonly Dictionary<string, string[]> KnownPartBones = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        // === MOrc Body ===
        { "morc_m000_u", new string[] { "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "bip01_spine", "Bip01_head", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm" } },
        { "morc_m000_l", new string[] { "Bip01_Pelvis", "bip01_spine", "Bip01_L_Thigh", "Bip01_L_Calf", "Bip01_R_Thigh", "Bip01_R_Calf" } },
        { "morc_m000_g", new string[] { "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger0", "Bip01_R_Finger01" } },
        { "morc_m000_b", new string[] { "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0" } },
        { "morc_m001_u", new string[] { "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "bip01_spine", "Bip01_head", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm" } },
        { "morc_m001_l", new string[] { "Bip01_Pelvis", "bip01_spine", "Bone08", "Bip01_L_Thigh", "Bone16", "Bone04", "Bone14", "Bone15", "Bone21", "Bone22", "Bip01_L_Calf", "Bone12", "Bone13", "Bone23", "Bone24", "Bone17", "Bone18", "Bip01_R_Thigh", "Bip01_R_Calf" } },
        { "morc_m001_g", new string[] { "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger0", "Bip01_R_Finger01" } },
        { "morc_m001_b", new string[] { "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0" } },

        // === MOrc Face & Hair ===
        { "morc_m000_f", OrcHead1Bone },
        { "morc_m000_m00_bh", OrcHead1Bone },
        { "morc_m001_m00_bh", OrcHead1Bone },
        { "morc_m002_m00_bh", OrcHead1Bone },
        { "morc_m003_m00_bh", OrcHead1Bone },
        { "morc_m004_m00_bh", OrcHead1Bone },
        { "morc_m008_m00_bh", OrcHead1Bone },
        { "morc_m009_m00_bh", OrcHead1Bone },
        { "morc_m000_m00_ah", OrcHead1Bone },

        // === FOrc Body ===
        { "forc_m000_u", new string[] { "Bip01_Neck", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "bip01_spine", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_head", "Breast_L_Bone", "Breast_R_Bone", "Bip01_Spine1", "bip01_spine2" } },
        { "forc_m000_l", new string[] { "Bip01_Pelvis", "bip01_spine", "Bip01_R_Thigh", "Bip01_R_Calf", "Bip01_L_Thigh", "Bip01_L_Calf" } },
        { "forc_m000_g", new string[] { "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger0", "Bip01_L_Finger01" } },
        { "forc_m000_b", new string[] { "Bip01_R_Calf", "Bip01_L_Calf", "bip01_l_foot", "bip01_r_foot", "Bip01_L_Toe0", "Bip01_R_Toe0" } },
        { "forc_m001_u", new string[] { "Bip01_Neck", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "bip01_spine", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_head", "Breast_L_Bone", "Breast_R_Bone", "Bip01_Spine1", "bip01_spine2" } },
        { "forc_m001_l", new string[] { "Bip01_Pelvis", "bip01_spine", "Bip01_R_Thigh", "Bip01_R_Calf", "Bip01_L_Thigh", "Bip01_L_Calf", "Bone21", "Bone43", "Bone44", "Bone01", "Bone02", "Bone50", "Bone51", "Bone41", "Bone05", "Bone45", "Bone46", "Bone47", "Bone48" } },
        { "forc_m001_g", new string[] { "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger0", "Bip01_L_Finger01" } },
        { "forc_m001_b", new string[] { "Bip01_R_Thigh", "Bip01_R_Calf", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "bip01_r_foot", "Bip01_L_Toe0", "Bip01_R_Toe0" } },

        // === FOrc Face & Hair ===
        { "forc_m000_f", OrcHead1Bone },
        { "forc_m000_m00_bh", OrcHead1Bone },
        { "forc_m001_m00_bh", OrcHead1Bone },
        { "forc_m002_m00_bh", OrcHead1Bone },
        { "forc_m003_m00_bh", OrcHead1Bone },
        { "forc_m004_m00_bh", OrcHead1Bone },
        { "forc_m005_m00_bh", OrcHead1Bone },
        { "forc_m006_m00_bh", OrcHead1Bone },
        { "forc_m008_m00_bh", OrcHead1Bone },
        { "forc_m009_m00_bh", OrcHead1Bone },
        { "forc_m000_m00_ah", OrcHead1Bone },

        // === MShaman Body ===
        { "mshaman_m000_u", new string[] { "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm" } },
        { "mshaman_m000_l", new string[] { "Bip01_Pelvis", "bip01_spine", "Bip01_R_Thigh", "Bip01_R_Calf", "Bip01_L_Thigh", "Bip01_L_Calf" } },
        { "mshaman_m000_g", new string[] { "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger1", "Bip01_R_Finger11" } },
        { "mshaman_m000_b", new string[] { "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0" } },
        { "mshaman_m001_u", new string[] { "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm" } },
        { "mshaman_m001_l", new string[] { "Bip01_Pelvis", "Bone02_01", "Bone02_02", "Bone02_03", "Bone02_04", "Bone03_01", "Bone03_02", "Bone03_03", "Bone03_04", "bip01_spine", "Bone05_01", "Bone05_02", "Bone05_03", "Bone05_04", "Bone001", "Bone04_01", "Bone04_02", "Bone04_03", "Bone04_04", "Bip01_R_Thigh", "Bip01_R_Calf", "Bip01_L_Thigh", "Bip01_L_Calf" } },
        { "mshaman_m001_g", new string[] { "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger1", "Bip01_R_Finger11" } },
        { "mshaman_m001_b", new string[] { "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0" } },

        // === MShaman Face & Hair ===
        { "mshaman_m000_f", OrcHead1Bone },
        { "mshaman_m000_m00_bh", OrcHead1Bone },
        { "mshaman_m001_m00_bh", OrcHead1Bone },
        { "mshaman_m002_m00_bh", OrcHead1Bone },
        { "mshaman_m003_m00_bh", OrcHead1Bone },
        { "mshaman_m004_m00_bh", OrcHead1Bone },
        { "mshaman_m008_m00_bh", OrcHead1Bone },
        { "mshaman_m009_m00_bh", OrcHead1Bone },
        { "mshaman_m000_m00_ah", OrcHead1Bone },

        // === FShaman Body ===
        { "fshaman_m000_u", new string[] { "Bip01_Pelvis", "Breast_L_Bone", "Breast_R_Bone", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm" } },
        { "fshaman_m000_l", new string[] { "Bip01_Pelvis", "bip01_spine", "Bip01_R_Thigh", "Bip01_R_Calf", "Bip01_L_Thigh", "Bip01_L_Calf" } },
        { "fshaman_m000_g", new string[] { "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger1", "Bip01_L_Finger11" } },
        { "fshaman_m000_b", new string[] { "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0" } },
        { "fshaman_m001_u", new string[] { "Breast_L_Bone", "Breast_R_Bone", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm" } },
        { "fshaman_m001_l", new string[] { "Bip01_Pelvis", "bip01_spine", "Bone09_01", "Bone001", "Bone09_02", "Bone09_03", "Bone09_04", "Bone08_01", "Bone08_02", "Bone08_03", "Bone08_04", "Bone07_01", "Bone07_02", "Bone07_03", "Bone07_04", "Bip01_R_Thigh", "Bip01_R_Calf", "Bip01_L_Thigh", "Bip01_L_Calf" } },
        { "fshaman_m001_g", new string[] { "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger1", "Bip01_L_Finger11" } },
        { "fshaman_m001_b", new string[] { "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0" } },

        // === FShaman Face & Hair ===
        { "fshaman_m000_f", OrcHead1Bone },
        { "fshaman_m000_m00_bh", OrcHead1Bone },
        { "fshaman_m001_m00_bh", OrcHead1Bone },
        { "fshaman_m002_m00_bh", OrcHead1Bone },
        { "fshaman_m003_m00_bh", OrcHead1Bone },
        { "fshaman_m004_m00_bh", OrcHead1Bone },
        { "fshaman_m005_m00_bh", OrcHead1Bone },
        { "fshaman_m006_m00_bh", OrcHead1Bone },
        { "fshaman_m008_m00_bh", OrcHead1Bone },
        { "fshaman_m009_m00_bh", OrcHead1Bone },
        { "fshaman_m000_m00_ah", OrcHead1Bone },

        // === mfighter Gloves (70 bones) ===
        { "mfighter_m000_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0Nub", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1Nub", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone08", "Bone09", "Bone02", "Bone04", "Dummy01", "Bone17", "Bone18", "Dummy11", "Bone06", "Bone07", "Dummy12", "Bone23", "Bone24", "Dummy16", "Bone21", "Bone22", "Dummy15", "Bone19", "Bone20", "Dummy14" } },
        { "mfighter_m001_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0Nub", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1Nub", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone08", "Bone09", "Bone02", "Bone04", "Dummy01", "Bone17", "Bone18", "Dummy11", "Bone06", "Bone07", "Dummy12", "Bone23", "Bone24", "Dummy16", "Bone21", "Bone22", "Dummy15", "Bone19", "Bone20", "Dummy14" } },

        // === ffighter Gloves (76 bones) ===
        { "ffighter_m000_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0Nub", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1Nub", "Shield_R_Bone", "Weapon_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Breast_L_Bone", "Breast_L_Bone_Dummy", "Breast_R_Bone", "Breast_R_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone21", "Bone07", "Bone40", "Bone41", "Dummy01", "Bone43", "Bone44", "Dummy02", "Bone01", "Bone02", "Dummy03", "Bone47", "Bone48", "Dummy05", "Bone49", "Bone50", "Dummy06", "Bone45", "Bone46", "Dummy04", "Bone03", "Dummy07" } },
        { "ffighter_m001_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0Nub", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1Nub", "Shield_R_Bone", "Weapon_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Breast_L_Bone", "Breast_L_Bone_Dummy", "Breast_R_Bone", "Breast_R_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone21", "Bone07", "Bone40", "Bone41", "Dummy01", "Bone43", "Bone44", "Dummy02", "Bone01", "Bone02", "Dummy03", "Bone47", "Bone48", "Dummy05", "Bone49", "Bone50", "Dummy06", "Bone45", "Bone46", "Dummy04", "Bone03", "Dummy07" } },

        // === melf Gloves (66 bones) ===
        { "melf_m000_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0R", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1R", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone08", "Bone06", "Dummy02", "Bone05", "Dummy03", "Bone18", "Bone02", "Dummy10", "Bone19", "Dummy11", "Bone20", "Dummy12", "Bone11", "Bone01", "Dummy01", "Bone09" } },
        { "melf_m001_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0R", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1R", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone08", "Bone06", "Dummy02", "Bone05", "Dummy03", "Bone18", "Bone02", "Dummy10", "Bone19", "Dummy11", "Bone20", "Dummy12", "Bone11", "Bone01", "Dummy01", "Bone09" } },

        // === felf Gloves (70 bones) ===
        { "felf_m000_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0R", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1R", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone01", "Bone02", "Bone03", "Bone04", "Skirt_RS_Bone_Dummy02", "Bone08", "Bone09", "Skirt_RS_Bone_Dummy03", "Skirt L Bone", "Bone05", "Dummy02", "Skirt R Bone", "Bone10", "Dummy16", "Bone06", "Bone07", "Skirt_B_Bone_Dummy", "Bone11", "Bone12", "Skirt_B_Bone_Dummy01" } },
        { "felf_m001_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0R", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1R", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone01", "Bone02", "Bone03", "Bone04", "Skirt_RS_Bone_Dummy02", "Bone08", "Bone09", "Skirt_RS_Bone_Dummy03", "Skirt L Bone", "Bone05", "Dummy02", "Skirt R Bone", "Bone10", "Dummy16", "Bone06", "Bone07", "Skirt_B_Bone_Dummy", "Bone11", "Bone12", "Skirt_B_Bone_Dummy01" } },

        // === mdarkelf Gloves (82 bones) ===
        { "mdarkelf_m000_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0R", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1R", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Shield_L_Bone", "Weapon_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Collar_L_Bone", "Collar_L_Bone_Dummy", "Collar_R_Bone", "Collar_R_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone01", "Bone03", "Bone07", "Bone08", "Bone09", "Bone10", "Dummy02", "Bone36", "Bone37", "Bone38", "Bone39", "Dummy15", "Bone40", "Bone41", "Bone42", "Dummy16", "Bone43", "Bone44", "Bone45", "Dummy17", "Bone33", "Bone34", "Bone35", "Dummy14", "Bone19", "Bone23", "Bone29", "Dummy12" } },
        { "mdarkelf_m001_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0R", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1R", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Shield_L_Bone", "Weapon_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Collar_L_Bone", "Collar_L_Bone_Dummy", "Collar_R_Bone", "Collar_R_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone01", "Bone03", "Bone07", "Bone08", "Bone09", "Bone10", "Dummy02", "Bone36", "Bone37", "Bone38", "Bone39", "Dummy15", "Bone40", "Bone41", "Bone42", "Dummy16", "Bone43", "Bone44", "Bone45", "Dummy17", "Bone33", "Bone34", "Bone35", "Dummy14", "Bone19", "Bone23", "Bone29", "Dummy12" } },

        // === fdarkelf Gloves (84 bones) ===
        { "fdarkelf_m000_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0R", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1R", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Collar_L_Bone", "Collar_L_Bone_Dummy", "Collar_R_Bone", "Collar_R_Bone_Dummy", "Breast_R_Bone", "Breast_R_Bone_Dummy", "Breast_L_Bone", "Breast_L_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "skirt_Bone01", "skirt_Bone02", "LF_skirt_bone01", "LF_skirt_bone02", "LF_skirt_bone03", "LF_skirt_bone04", "LF_skirt_bone05", "RF_skirt_bone01", "RF_skirt_bone02", "RF_skirt_bone03", "RF_skirt_bone04", "RF_skirt_bone05", "RB_skirt_bone01", "RB_skirt_bone02", "RB_skirt_bone03", "RB_skirt_bone04", "LB_skirt_bone01", "LB_skirt_bone02", "LB_skirt_bone03", "LB_skirt_bone04", "RS_skirt_bone01", "RS_skirt_bone02", "RS_skirt_bone03", "LS_skirt_bone01", "LS_skirt_bone02", "LS_skirt_bone03" } },
        { "fdarkelf_m001_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0R", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1R", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Collar_L_Bone", "Collar_L_Bone_Dummy", "Collar_R_Bone", "Collar_R_Bone_Dummy", "Breast_R_Bone", "Breast_R_Bone_Dummy", "Breast_L_Bone", "Breast_L_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "skirt_Bone01", "skirt_Bone02", "LF_skirt_bone01", "LF_skirt_bone02", "LF_skirt_bone03", "LF_skirt_bone04", "LF_skirt_bone05", "RF_skirt_bone01", "RF_skirt_bone02", "RF_skirt_bone03", "RF_skirt_bone04", "RF_skirt_bone05", "RB_skirt_bone01", "RB_skirt_bone02", "RB_skirt_bone03", "RB_skirt_bone04", "LB_skirt_bone01", "LB_skirt_bone02", "LB_skirt_bone03", "LB_skirt_bone04", "RS_skirt_bone01", "RS_skirt_bone02", "RS_skirt_bone03", "LS_skirt_bone01", "LS_skirt_bone02", "LS_skirt_bone03" } },

        // === mdwarf Gloves (76 bones) ===
        { "mdwarf_m000_g", new string[] { "bip01", "Bip01 Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Shield_L_Bone", "Weapon_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0R", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1Nub", "Shield_R_Bone", "Weapon_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone08", "Bone09", "Bone03", "Bone06", "Dummy13", "Bone15", "Bone16", "Dummy16", "Bone01", "Bone02", "Dummy10", "Bone17", "Bone18", "Dummy17", "Bone07", "Bone14", "Dummy15", "Bone12", "Bone13", "Dummy14", "Bone10", "Bone11", "Dummy11", "Bone04", "Bone05", "Dummy12" } },
        { "mdwarf_m001_g", new string[] { "bip01", "Bip01 Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Shield_L_Bone", "Weapon_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0R", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1Nub", "Shield_R_Bone", "Weapon_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone08", "Bone09", "Bone03", "Bone06", "Dummy13", "Bone15", "Bone16", "Dummy16", "Bone01", "Bone02", "Dummy10", "Bone17", "Bone18", "Dummy17", "Bone07", "Bone14", "Dummy15", "Bone12", "Bone13", "Dummy14", "Bone10", "Bone11", "Dummy11", "Bone04", "Bone05", "Dummy12" } },

        // === fdwarf Gloves (86 bones) ===
        { "fdwarf_m000_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Shield_L_Bone", "Weapon_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0R", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1R", "Shield_R_Bone", "Weapon_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone01", "Bone02", "Bone03", "Bone04", "Bone09", "Dummy05", "Bone05", "Bone08", "Bone15", "Dummy06", "Bone65", "Bone66", "Bone67", "Dummy04", "Bone62", "Bone63", "Bone64", "Dummy08", "Bone59", "Bone60", "Bone61", "Dummy07", "Bone06", "Bone07", "Bone10", "Dummy03", "Bone68", "Bone69", "Bone70", "Bone71", "Dummy10", "Bone11", "Bone12", "Bone13", "Bone14", "Dummy09" } },
        { "fdwarf_m001_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Shield_L_Bone", "Weapon_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0R", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1R", "Shield_R_Bone", "Weapon_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone01", "Bone02", "Bone03", "Bone04", "Bone09", "Dummy05", "Bone05", "Bone08", "Bone15", "Dummy06", "Bone65", "Bone66", "Bone67", "Dummy04", "Bone62", "Bone63", "Bone64", "Dummy08", "Bone59", "Bone60", "Bone61", "Dummy07", "Bone06", "Bone07", "Bone10", "Dummy03", "Bone68", "Bone69", "Bone70", "Bone71", "Dummy10", "Bone11", "Bone12", "Bone13", "Bone14", "Dummy09" } },

        // === mmagic Gloves (86 bones) ===
        { "mmagic_m000_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0Nub", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1Nub", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone01", "Bone01-01", "Bone01-02", "Bone01-03", "Bone01-04", "Bone01-05", "Dummy01", "Bone02-01", "Bone02-02", "Bone02-03", "Bone02-04", "Dummy02", "Bone03-01", "Bone03-02", "Bone03-03", "Bone03-04", "Bone03-05", "Dummy03", "Bone04-01", "Bone04-02", "Bone04-03", "Bone04-04", "Bone04-05", "Dummy04", "Bone05-01", "Bone05-02", "Bone05-03", "Bone05-04", "Dummy05", "Bone06-01", "Bone06-02", "Bone06-03", "Bone06-04", "Bone06-05", "Dummy06", "Bone02" } },
        { "mmagic_m001_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0Nub", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1Nub", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone01", "Bone01-01", "Bone01-02", "Bone01-03", "Bone01-04", "Bone01-05", "Dummy01", "Bone02-01", "Bone02-02", "Bone02-03", "Bone02-04", "Dummy02", "Bone03-01", "Bone03-02", "Bone03-03", "Bone03-04", "Bone03-05", "Dummy03", "Bone04-01", "Bone04-02", "Bone04-03", "Bone04-04", "Bone04-05", "Dummy04", "Bone05-01", "Bone05-02", "Bone05-03", "Bone05-04", "Dummy05", "Bone06-01", "Bone06-02", "Bone06-03", "Bone06-04", "Bone06-05", "Dummy06", "Bone02" } },
        { "mmagic_m005_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0Nub", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1Nub", "Weapon_R_Bone", "Shield_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone01", "Bone01-01", "Bone01-02", "Bone01-03", "Bone01-04", "Bone01-05", "Dummy01", "Bone02-01", "Bone02-02", "Bone02-03", "Bone02-04", "Dummy02", "Bone03-01", "Bone03-02", "Bone03-03", "Bone03-04", "Bone03-05", "Dummy03", "Bone04-01", "Bone04-02", "Bone04-03", "Bone04-04", "Bone04-05", "Dummy04", "Bone05-01", "Bone05-02", "Bone05-03", "Bone05-04", "Dummy05", "Bone06-01", "Bone06-02", "Bone06-03", "Bone06-04", "Bone06-05", "Dummy06", "Bone02" } },

        // === fmagic Gloves (90 bones) ===
        { "fmagic_m000_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0Nub", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1Nub", "Shield_R_Bone", "Weapon_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone03", "Bone03-01", "Bone03-02", "Bone03-03", "Bone03-04", "Bone03-05", "Dummy03", "Bone02-01", "Bone02-02", "Bone02-03", "Bone02-04", "Bone02-05", "Dummy02", "Bone04-01", "Bone04-02", "Bone04-03", "Bone04-04", "Bone04-05", "Dummy04", "Bone05-01", "Bone05-02", "Bone05-03", "Bone05-04", "Bone05-05", "Dummy05", "Bone06-01", "Bone06-02", "Bone06-03", "Bone06-04", "Bone06-05", "Bone06-06", "Dummy06", "Bone72", "Bone01-01", "Bone01-02", "Bone01-03", "Bone01-04", "Bone01-05", "Bone01-06", "Dummy01" } },
        { "fmagic_m001_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0Nub", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1Nub", "Shield_R_Bone", "Weapon_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone03", "Bone03-01", "Bone03-02", "Bone03-03", "Bone03-04", "Bone03-05", "Dummy03", "Bone02-01", "Bone02-02", "Bone02-03", "Bone02-04", "Bone02-05", "Dummy02", "Bone04-01", "Bone04-02", "Bone04-03", "Bone04-04", "Bone04-05", "Dummy04", "Bone05-01", "Bone05-02", "Bone05-03", "Bone05-04", "Bone05-05", "Dummy05", "Bone06-01", "Bone06-02", "Bone06-03", "Bone06-04", "Bone06-05", "Bone06-06", "Dummy06", "Bone72", "Bone01-01", "Bone01-02", "Bone01-03", "Bone01-04", "Bone01-05", "Bone01-06", "Dummy01" } },
        { "fmagic_m003_g", new string[] { "bip01", "Bip01_Pelvis", "bip01_spine", "Bip01_L_Thigh", "Bip01_L_Calf", "bip01_l_foot", "Bip01_L_Toe0", "Bip01_L_Toe0Nub", "Bip01_R_Thigh", "Bip01_R_Calf", "bip01_r_foot", "Bip01_R_Toe0", "Bip01_R_Toe0Nub", "Bip01_Spine1", "bip01_spine2", "Bip01_Neck", "Bip01_head", "Bip01_HeadNub", "Bip01_L_Clavicle", "Bip01_L_UpperArm", "Bip01_L_Forearm", "Bip01_L_Hand", "Bip01_L_Finger0", "Bip01_L_Finger01", "Bip01_L_Finger0Nub", "Bip01_L_Finger1", "Bip01_L_Finger11", "Bip01_L_Finger1Nub", "Weapon_L_Bone", "Shield_L_Bone", "Shoulder_L_Bone", "Shoulder_L_Bone_Dummy", "Bip01_R_Clavicle", "Bip01_R_UpperArm", "Bip01_R_Forearm", "Bip01_R_Hand", "Bip01_R_Finger0", "Bip01_R_Finger01", "Bip01_R_Finger0Nub", "Bip01_R_Finger1", "Bip01_R_Finger11", "Bip01_R_Finger1Nub", "Shield_R_Bone", "Weapon_R_Bone", "Shoulder_R_Bone", "Shoulder_R_Bone_Dummy", "Cape_Bone", "Cape_Bone_Dummy", "Sheath_L_Bone", "Sheath_L_Bone_Dummy", "Bone03", "Bone03-01", "Bone03-02", "Bone03-03", "Bone03-04", "Bone03-05", "Dummy03", "Bone02-01", "Bone02-02", "Bone02-03", "Bone02-04", "Bone02-05", "Dummy02", "Bone04-01", "Bone04-02", "Bone04-03", "Bone04-04", "Bone04-05", "Dummy04", "Bone05-01", "Bone05-02", "Bone05-03", "Bone05-04", "Bone05-05", "Dummy05", "Bone06-01", "Bone06-02", "Bone06-03", "Bone06-04", "Bone06-05", "Bone06-06", "Dummy06", "Bone72", "Bone01-01", "Bone01-02", "Bone01-03", "Bone01-04", "Bone01-05", "Bone01-06", "Dummy01" } },
    };

    void Start()
    {
        SyncMesh();
    }

    void Update()
    {
#if (UNITY_EDITOR)
        if (!EditorApplication.isPlaying)
        {
            SyncMesh();
        }
#endif
    }

    public void SyncMesh()
    {
        if (this.gameObject.activeSelf && this.gameObject.activeInHierarchy)
        {
            StartCoroutine(SyncTask());
        }
    }

    private IEnumerator SyncTask()
    {
        if (_bodyPartsContainer == null && transform.parent != null && transform.parent.childCount > 1)
        {
            _bodyPartsContainer = transform.parent.GetChild(1);
        }
        if (_rootSkinnedRenderer == null && transform.childCount > 1)
        {
            _rootSkinnedRenderer = transform.GetChild(1).GetComponentInChildren<SkinnedMeshRenderer>();
        }

        yield return new WaitForEndOfFrame();

        DoSync();
    }

    private void DoSync()
    {
        if (_bodyPartsContainer == null)
        {
            if (transform.parent != null && transform.parent.childCount > 1)
            {
                _bodyPartsContainer = transform.parent.GetChild(1);
            }
            if (_bodyPartsContainer == null)
            {
                _bodyPartsContainer = transform.FindRecursive("Bodyparts");
            }
        }
        if (_bodyPartsContainer == null) return;
        _bodyPartsContainer.gameObject.SetActive(true);

        if (_rootSkinnedRenderer == null)
        {
            if (transform.childCount > 1)
            {
                _rootSkinnedRenderer = transform.GetChild(1).GetComponentInChildren<SkinnedMeshRenderer>(true);
            }
            if (_rootSkinnedRenderer == null)
            {
                _rootSkinnedRenderer = GetComponentInChildren<SkinnedMeshRenderer>(true);
            }
            if (_rootSkinnedRenderer == null && transform.parent != null)
            {
                _rootSkinnedRenderer = transform.parent.GetComponentInChildren<SkinnedMeshRenderer>(true);
            }
        }

        // Retrieve ALL SkinnedMeshRenderers (including children)
        _destSkinnedRenderer = _bodyPartsContainer.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        // Build bone map strictly from this character's animated skeleton
        Animator anim = GetComponentInChildren<Animator>(true);
        if (anim == null && transform.parent != null)
        {
            anim = transform.parent.GetComponentInChildren<Animator>(true);
        }
        Transform searchRoot = anim != null ? anim.transform : transform;
        Transform[] allTransforms = searchRoot.GetComponentsInChildren<Transform>(true);
        var boneMap = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in allTransforms)
        {
            if (_bodyPartsContainer != null && t.IsChildOf(_bodyPartsContainer)) continue;
            if (!boneMap.ContainsKey(t.name))
            {
                boneMap[t.name] = t;
            }
        }

        // Also search in parent/root (excluding body parts)
        Transform topRoot = transform.parent != null ? transform.parent : transform;
        foreach (var t in topRoot.GetComponentsInChildren<Transform>(true))
        {
            if (_bodyPartsContainer != null && t.IsChildOf(_bodyPartsContainer)) continue;
            if (!boneMap.ContainsKey(t.name))
            {
                boneMap[t.name] = t;
            }
        }

        Transform charRootBone = null;
        if (boneMap.TryGetValue("Bip01", out Transform bip))
        {
            charRootBone = bip;
        }
        else if (boneMap.TryGetValue("Bip01 Pelvis", out Transform pelvis))
        {
            charRootBone = pelvis;
        }
        else if (boneMap.TryGetValue("bip01_spine", out Transform spine))
        {
            charRootBone = spine;
        }
        else if (_rootSkinnedRenderer != null && _rootSkinnedRenderer.rootBone != null)
        {
            if (boneMap.TryGetValue(_rootSkinnedRenderer.rootBone.name, out Transform liveRoot))
            {
                charRootBone = liveRoot;
            }
            else
            {
                charRootBone = _rootSkinnedRenderer.rootBone;
            }
        }
        else if (_rootBone != null)
        {
            charRootBone = _rootBone;
        }
        _rootBone = charRootBone;

        Transform headBone = null;
        if (!boneMap.TryGetValue("Bip01_head", out headBone))
        {
            if (!boneMap.TryGetValue("Bip01 Head", out headBone))
            {
                if (!boneMap.TryGetValue("bip01_head", out headBone))
                {
                    foreach (var kvp in boneMap)
                    {
                        if (kvp.Key.IndexOf("head", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            headBone = kvp.Value;
                            break;
                        }
                    }
                }
            }
        }

        Bounds bounds = new Bounds(Vector3.zero, new Vector3(5f, 5f, 5f));

        foreach (var renderer in _destSkinnedRenderer)
        {
            if (renderer == null || renderer.sharedMesh == null) continue;

            renderer.enabled = true;
            renderer.transform.localScale = Vector3.one;
            renderer.localBounds = bounds;
            renderer.updateWhenOffscreen = true;

            int bindposeCount = renderer.sharedMesh.bindposes != null ? renderer.sharedMesh.bindposes.Length : 0;
            if (bindposeCount == 0) continue;

            string partKey = renderer.gameObject.name.ToLowerInvariant().Replace("(clone)", "").Trim();
            string meshKey = renderer.sharedMesh.name.ToLowerInvariant().Trim();
            bool isHeadOrHair = partKey.Contains("_f") || partKey.Contains("_h") || partKey.Contains("face") || partKey.Contains("hair") ||
                               meshKey.Contains("_f") || meshKey.Contains("_h") || meshKey.Contains("face") || meshKey.Contains("hair");
            bool isOrcOrShaman = partKey.StartsWith("morc") || partKey.StartsWith("forc") || partKey.StartsWith("mshaman") || partKey.StartsWith("fshaman") ||
                                 meshKey.StartsWith("morc") || meshKey.StartsWith("forc") || meshKey.StartsWith("mshaman") || meshKey.StartsWith("fshaman");
            Transform targetHeadOrRoot = (isHeadOrHair && headBone != null) ? headBone : charRootBone;

            // Strategy 1: Look up bone names by part/mesh name from KnownPartBones
            string[] boneNames = null;
            if (!KnownPartBones.TryGetValue(partKey, out boneNames))
            {
                KnownPartBones.TryGetValue(meshKey, out boneNames);
            }

            // Strategy 1b: Fallback for other armor tiers of known parts (e.g. morc_m002_u -> morc_m000_u)
            if (boneNames == null && partKey.Contains("_m00") && partKey.Length >= 10)
            {
                int lastUnder = partKey.LastIndexOf('_');
                if (lastUnder > 0)
                {
                    string slot = partKey.Substring(lastUnder);
                    string racePrefix = partKey.Split('_')[0];
                    if (KnownPartBones.TryGetValue($"{racePrefix}_m000{slot}", out string[] fb) && fb.Length == bindposeCount)
                    {
                        boneNames = fb;
                    }
                    else if (KnownPartBones.TryGetValue($"{racePrefix}_m001{slot}", out fb) && fb.Length == bindposeCount)
                    {
                        boneNames = fb;
                    }
                    else if (KnownPartBones.TryGetValue($"{racePrefix}_m003{slot}", out fb) && fb.Length == bindposeCount)
                    {
                        boneNames = fb;
                    }
                    else if (KnownPartBones.TryGetValue($"{racePrefix}_m005{slot}", out fb) && fb.Length == bindposeCount)
                    {
                        boneNames = fb;
                    }
                }
            }

            // Strategy 1c: Generic Orc / Head / Hair dynamic detection
            if (boneNames == null || boneNames.Length != bindposeCount)
            {
                if (bindposeCount == 1 && (isHeadOrHair || isOrcOrShaman))
                {
                    boneNames = OrcHead1Bone;
                }
                else if (bindposeCount == 7 && (isHeadOrHair || isOrcOrShaman))
                {
                    boneNames = OrcHead7Bones;
                }
                else if (bindposeCount == 9 && (isHeadOrHair || isOrcOrShaman))
                {
                    boneNames = OrcHead9Bones;
                }
            }

            if (boneNames != null && boneNames.Length == bindposeCount)
            {
                Transform[] newBones = new Transform[boneNames.Length];
                for (int i = 0; i < boneNames.Length; i++)
                {
                    string bName = boneNames[i];
                    if (boneMap.TryGetValue(bName, out Transform matched) ||
                        boneMap.TryGetValue(bName.Replace(" ", "_"), out matched) ||
                        boneMap.TryGetValue(bName.Replace("_", " "), out matched))
                    {
                        newBones[i] = matched;
                    }
                    else
                    {
                        newBones[i] = targetHeadOrRoot;
                    }
                }
                renderer.bones = newBones;
                if (targetHeadOrRoot != null) renderer.rootBone = targetHeadOrRoot;
                continue;
            }

            // Strategy 2: Standard Lineage 2 race skeleton (Human, Elf, Dark Elf, Dwarf)
            // Where _rootSkinnedRenderer.bones contains the full matching skeleton
            if (_rootSkinnedRenderer != null && _rootSkinnedRenderer.bones != null &&
                _rootSkinnedRenderer.bones.Length == bindposeCount &&
                _rootSkinnedRenderer.bones.Length > 0 &&
                _rootSkinnedRenderer.bones[0] != null)
            {
                Transform[] matchedBones = new Transform[_rootSkinnedRenderer.bones.Length];
                bool allMatched = true;
                for (int i = 0; i < _rootSkinnedRenderer.bones.Length; i++)
                {
                    Transform b = _rootSkinnedRenderer.bones[i];
                    if (b != null && (boneMap.TryGetValue(b.name, out Transform liveB) ||
                                      boneMap.TryGetValue(b.name.Replace(" ", "_"), out liveB) ||
                                      boneMap.TryGetValue(b.name.Replace("_", " "), out liveB)))
                    {
                        matchedBones[i] = liveB;
                    }
                    else if (b != null)
                    {
                        matchedBones[i] = b;
                    }
                    else
                    {
                        allMatched = false;
                        break;
                    }
                }

                renderer.bones = allMatched ? matchedBones : _rootSkinnedRenderer.bones;

                if (_rootSkinnedRenderer.rootBone != null &&
                    (boneMap.TryGetValue(_rootSkinnedRenderer.rootBone.name, out Transform liveRoot) ||
                     boneMap.TryGetValue(_rootSkinnedRenderer.rootBone.name.Replace(" ", "_"), out liveRoot) ||
                     boneMap.TryGetValue(_rootSkinnedRenderer.rootBone.name.Replace("_", " "), out liveRoot)))
                {
                    renderer.rootBone = liveRoot;
                }
                else if (_rootSkinnedRenderer.rootBone != null)
                {
                    renderer.rootBone = _rootSkinnedRenderer.rootBone;
                }
                else if (charRootBone != null)
                {
                    renderer.rootBone = charRootBone;
                }

                continue;
            }

            // Strategy 3: Renderer already has live non-null bone array of matching length with named bones
            if (renderer.bones != null && renderer.bones.Length == bindposeCount && renderer.bones.Length > 0)
            {
                bool anyNonNull = false;
                for (int i = 0; i < renderer.bones.Length; i++)
                {
                    if (renderer.bones[i] != null)
                    {
                        anyNonNull = true;
                        break;
                    }
                }

                if (anyNonNull)
                {
                    Transform[] newBones = new Transform[renderer.bones.Length];
                    for (int i = 0; i < renderer.bones.Length; i++)
                    {
                        Transform b = renderer.bones[i];
                        if (b != null && (boneMap.TryGetValue(b.name, out Transform matched) ||
                                          boneMap.TryGetValue(b.name.Replace(" ", "_"), out matched) ||
                                          boneMap.TryGetValue(b.name.Replace("_", " "), out matched)))
                        {
                            newBones[i] = matched;
                        }
                        else if (b != null)
                        {
                            newBones[i] = b;
                        }
                        else
                        {
                            newBones[i] = targetHeadOrRoot;
                        }
                    }
                    renderer.bones = newBones;
                    if (targetHeadOrRoot != null) renderer.rootBone = targetHeadOrRoot;
                    continue;
                }
            }

            // Strategy 4: Fallback - ensure renderer.bones is non-null, correct length, and has no null entries
            if (renderer.bones == null || renderer.bones.Length == 0 || renderer.bones.Length != bindposeCount)
            {
                Transform[] fallbackBones = new Transform[bindposeCount];
                for (int i = 0; i < bindposeCount; i++)
                {
                    fallbackBones[i] = targetHeadOrRoot;
                }
                renderer.bones = fallbackBones;
            }
            else
            {
                Transform[] curBones = renderer.bones;
                bool hasNull = false;
                for (int i = 0; i < curBones.Length; i++)
                {
                    if (curBones[i] == null)
                    {
                        curBones[i] = targetHeadOrRoot;
                        hasNull = true;
                    }
                }
                if (hasNull)
                {
                    renderer.bones = curBones;
                }
            }
            if (targetHeadOrRoot != null)
            {
                renderer.rootBone = targetHeadOrRoot;
            }
        }
    }
}
