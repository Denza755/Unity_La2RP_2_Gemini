using System;
using UnityEngine;

public class Gear : MonoBehaviour
{
    [SerializeField] protected EntityReferenceHolder _referenceHolder;

    protected int _ownerId;

    [Header("Bones")]
    [SerializeField] protected Transform _rightHandBone;
    [SerializeField] protected Transform _leftHandBone;
    [SerializeField] protected Transform _shieldBone;

    [Header("Weapons")]
    [Header("Meta")]
    [System.NonSerialized] private Weapon _rightHandWeaponData;
    [System.NonSerialized] private Weapon _leftHandWeaponData;
    [SerializeField] protected float _weaponSizeRatio;
    [Header("Models")]
    [Header("Right hand")]
    [SerializeField] private WeaponType _rightHandType;
    [SerializeField] protected Transform _rightHandWeapon;
    [SerializeField] protected Transform _arrow;
    [Header("LeftHand")]
    [SerializeField] private WeaponType _leftHandType;
    [SerializeField] protected Transform _leftHandWeapon;

    protected NewBaseAnimationController AnimationController { get { return _referenceHolder.NewAnimationController; } }
    public WeaponType WeaponType
    {
        get
        {
            if (_rightHandType == WeaponType.bow || _leftHandType == WeaponType.bow)
            {
                return WeaponType.bow;
            }

            return (_leftHandType != WeaponType.none && _leftHandType != WeaponType.hand) ? _leftHandType : _rightHandType;
        }
    }
    public int OwnerId { get { return _ownerId; } set { _ownerId = value; } }

    public Transform RightHandBone { get { return GetRightHandBone(); } }
    public Transform LeftHandBone { get { return GetLeftHandBone(); } }
    public Transform ShieldBone { get { return GetShieldBone(); } }
    public Transform Arrow { get { return _arrow; } }

    [Header("Arrow Alignment")]
    [SerializeField] protected Vector3 _arrowForwardAxis = Vector3.left;
    [SerializeField] protected Vector3 _arrowNockOffset = Vector3.zero;

    public Vector3 ArrowForwardAxis => _arrowForwardAxis;
    public Vector3 ArrowNockOffset => _arrowNockOffset;

    public virtual void Initialize(int ownderId)
    {
        if (_referenceHolder == null)
        {
            TryGetComponent(out _referenceHolder);
            Debug.LogWarning($"[{transform.name}] EntityReferenceHolder was not assigned, please pre-assign it to avoid unecessary load.");
        }

        _ownerId = ownderId;

        _rightHandWeaponData = null;
        _leftHandWeaponData = null;

        GetLeftHandBone();
        GetRightHandBone();
    }

    // TODO: PRE-ASSIGN BONES IN PREFAB TO AVOID CPU LOAD

    public bool IsWeaponAlreadyEquipped(int itemId, bool leftSlot)
    {
        if (leftSlot)
        {
            if (_leftHandWeaponData == null)
            {
                Debug.Log("Left hand metadata is null, weapon not equiped.");
                return false;
            }

            bool idMatch = itemId == _leftHandWeaponData.Id;
            if (!idMatch)
            {
                Debug.Log("Left hand weapon id did not match, weapon not equiped.");
            }

            return idMatch;
        }
        else
        {
            if (_rightHandWeaponData == null)
            {
                Debug.Log("Right hand metadata is null, weapon not equiped.");
                return false;
            }

            bool idMatch = itemId == _rightHandWeaponData.Id;
            if (!idMatch)
            {
                Debug.Log("Right hand weapon id did not match, weapon not equiped.");
            }

            return idMatch;
        }
    }

    public virtual void EquipAllWeapons(Appearance appearance)
    {
        int rHandId = appearance.RHand;
        int lHandId = appearance.LHand;

        Weapon rWeapon = rHandId != 0 ? ItemTable.Instance.GetWeapon(rHandId) : null;
        WeaponType rType = (rWeapon != null && rWeapon.Weapongrp != null) ? rWeapon.Weapongrp.WeaponType : WeaponType.none;

        // Check if left hand contains arrows (EtcItem)
        EtcItem lEtcItem = lHandId != 0 ? ItemTable.Instance.GetEtcItem(lHandId) : null;
        bool lIsArrow = lEtcItem != null && lEtcItem.EtcItemgrp != null && lEtcItem.EtcItemgrp.EtcItemType == "arrow";

        Weapon lWeapon = (!lIsArrow && lHandId != 0) ? ItemTable.Instance.GetWeapon(lHandId) : null;

        if (rHandId != 0 && rWeapon != null)
        {
            if (rType == WeaponType.bow)
            {
                UnequipWeapon(true);
                UnequipWeapon(false);
                EquipWeapon(rHandId, rWeapon, false);
                int arrowId = lIsArrow ? lHandId : 17;
                EquipArrow(arrowId);
            }
            else if (rType == WeaponType.dual || rType == WeaponType.fist)
            {
                UnequipWeapon(true);
                EquipWeapon(rHandId, rWeapon, false);
            }
            else
            {
                if (rType == WeaponType.bigword || rType == WeaponType.pole || rType == WeaponType.bigblunt)
                {
                    UnequipWeapon(true);
                }
                EquipWeapon(rHandId, rWeapon, false);
            }
        }
        else
        {
            UnequipWeapon(false);
        }

        if (lIsArrow)
        {
            // Arrows equipped in LHand slot: configure arrow in right hand
            EquipArrow(lHandId);
        }
        else if (lWeapon != null)
        {
            // Shield or secondary weapon
            EquipWeapon(lHandId, lWeapon, true);
        }
        else
        {
            bool isTwoHanded = rWeapon != null && rWeapon.Weapongrp != null &&
                (rWeapon.Weapongrp.BodyPart == ItemSlot.SLOT_LR_HAND || rType == WeaponType.bow || rType == WeaponType.dual);

            if (!isTwoHanded)
            {
                UnequipWeapon(true);
            }
        }
    }

    public virtual void EquipArrow(int arrowId = 17)
    {
        if (arrowId <= 0) arrowId = 17;

        if (_arrow != null)
        {
            UnEquipArrow();
        }

        Debug.Log($"[{transform.name}] Equip arrow {arrowId}");
        GameObject arrowPrefab = ModelTable.Instance.GetItemModelById(arrowId);
        if (arrowPrefab == null && arrowId != 17)
        {
            arrowPrefab = ModelTable.Instance.GetItemModelById(17);
        }

        if (arrowPrefab == null)
        {
            Debug.LogWarning($"Could not load arrow prefab {arrowId} in DB for entity {_ownerId}.");
            return;
        }

        GameObject go = GameObject.Instantiate(arrowPrefab);
        go.SetActive(false);
        go.transform.name = "arrow";

        Transform rightBone = GetRightHandBone();
        AttachWeaponInstanceToBone(go, arrowPrefab, rightBone);
        CalculateArrowAlignmentData(go, out _arrowForwardAxis, out _arrowNockOffset);
        _arrow = go.transform;
    }

    private const float ReferenceWeaponBoneScale = 100f;
    // assets/models/weapons (UnitScaleFactor=1, globalScale=1) is ~133.684x larger than LineageWeapons (globalScale=0.019, UnitScaleFactor=100).
    private const float FallbackToLegacyWeaponScale = 0.019f / 2.54f;
    private const float MaxWorldWeaponExtent = 3.5f;

    /// <summary>Parents a weapon/shield/arrow instance onto targetBone and normalizes scale across 100x/1x rigs and fallback assets.</summary>
    private void AttachWeaponInstanceToBone(GameObject instance, GameObject prefab, Transform targetBone)
    {
        Transform parent = targetBone != null ? targetBone : transform;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        Vector3 parentLossy = parent.lossyScale;
        float boneScale = Mathf.Max(0.0001f, (Mathf.Abs(parentLossy.x) + Mathf.Abs(parentLossy.y) + Mathf.Abs(parentLossy.z)) / 3f);

        // Legacy Data/Animations skeletons use 100x armature scale; Orc/Shaman skeletons use ~0.76..1.0x.
        float boneCompensation = boneScale < 10f ? (ReferenceWeaponBoneScale / boneScale) : 1f;

        float maxLocalExtent = GetMaxLocalMeshExtent(instance);
        bool isFallbackAsset = (ModelTable.Instance != null && ModelTable.Instance.IsFallbackWeaponPrefab(prefab))
            || (maxLocalExtent > 0.08f && instance.transform.Find("handle") != null);

        float assetCompensation = isFallbackAsset ? FallbackToLegacyWeaponScale : 1f;
        float finalScale = GetWeaponSizeRatio() * boneCompensation * assetCompensation;

        float worldExtent = maxLocalExtent * boneScale * finalScale;
        if (worldExtent > MaxWorldWeaponExtent && worldExtent > 0.0001f)
        {
            finalScale *= MaxWorldWeaponExtent / worldExtent;
        }

        instance.transform.localScale = Vector3.one * finalScale;
    }

    private static float GetMaxLocalMeshExtent(GameObject instance)
    {
        float maxExtent = 0f;
        foreach (MeshFilter mf in instance.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null) continue;
            maxExtent = Mathf.Max(maxExtent, GetMeshExtentInRootSpace(mf.sharedMesh.bounds.size, mf.transform, instance.transform));
        }

        foreach (SkinnedMeshRenderer smr in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr.sharedMesh == null) continue;
            maxExtent = Mathf.Max(maxExtent, GetMeshExtentInRootSpace(smr.sharedMesh.bounds.size, smr.transform, instance.transform));
        }

        return maxExtent;
    }

    private static float GetMeshExtentInRootSpace(Vector3 boundsSize, Transform child, Transform root)
    {
        Vector3 relScale = Vector3.one;
        Transform current = child;
        while (current != null && current != root)
        {
            relScale = Vector3.Scale(relScale, current.localScale);
            current = current.parent;
        }

        return Mathf.Max(
            Mathf.Abs(boundsSize.x * relScale.x),
            Mathf.Abs(boundsSize.y * relScale.y),
            Mathf.Abs(boundsSize.z * relScale.z));
    }

    private float GetWeaponSizeRatio()
    {
        if (_weaponSizeRatio <= 0.001f)
        {
            if (_referenceHolder == null || _referenceHolder.Entity == null || _referenceHolder.Entity.Appearance == null)
            {
                return 1.0f;
            }
            float collisionHeight = _referenceHolder.Entity.Appearance.CollisionHeight;
            float ratio = collisionHeight > 0.001f ? (collisionHeight / 0.44f) : 1.0f;
            _weaponSizeRatio = Mathf.Clamp(ratio, 0.5f, 2.0f);
        }

        return _weaponSizeRatio;
    }

    public virtual void UnEquipArrow()
    {
        if (_arrow == null)
        {
            return;
        }

        Debug.Log($"[{transform.name}] Unequip arrow");
        Destroy(_arrow.gameObject);
        _arrow = null;
        _arrowForwardAxis = Vector3.left;
        _arrowNockOffset = Vector3.zero;
    }

    public virtual void ShowArrow()
    {
        if (_arrow == null)
        {
            EquipArrow();
        }

        // Debug.Log($"[{transform.name}] Show arrow");
        if (_arrow != null)
        {
            if (_arrowForwardAxis == Vector3.zero)
            {
                CalculateArrowAlignmentData(_arrow.gameObject, out _arrowForwardAxis, out _arrowNockOffset);
            }
            _arrow.gameObject.SetActive(true);
            UpdateArrowOrientation();
        }
    }

    public virtual void HideArrow()
    {
        // Debug.Log($"[{transform.name}] Hide arrow");
        if (_arrow != null)
        {
            _arrow.gameObject.SetActive(false);
        }
    }

    public virtual void EquipAllArmors(Appearance appearance) { }

    public virtual void EquipWeapon(int weaponId, Weapon weapon, bool leftSlot)
    {
        if (weaponId == 0 || weapon == null || weapon.Weapongrp == null)
        {
            return;
        }

        WeaponType weaponType = weapon.Weapongrp.WeaponType;
        if (IsWeaponAlreadyEquipped(weaponId, leftSlot))
        {
            Debug.Log($"Weapon {weaponId} of type {weaponType} is already equipped in {(leftSlot ? "left" : "right")} slot.");
            return;
        }
        else
        {
            Debug.Log($"Weapon {weaponId} of type {weaponType} was not equipped in {(leftSlot ? "left" : "right")} slot.");
        }

        UnequipWeapon(leftSlot);

        GameObject[] weaponPrefabs = ModelTable.Instance.GetWeaponsById(weaponId); //TODO: For duals and fists
        if (weaponPrefabs == null)
        {
            Debug.LogWarning($"Could not load weapon prefab array for weaponId: {weaponId} (entity: {_ownerId}).");
            return;
        }

        for (int i = 0; i < weaponPrefabs.Length; i++)
        {
            if (weaponPrefabs[i] == null)
            {
                Debug.LogWarning($"Missing prefab at index {i} for weaponId: {weaponId} (entity: {_ownerId}).");
                return;
            }
        }

        // Updating weapon type
        if (weaponType == WeaponType.bow)
        {
            _rightHandWeaponData = weapon;
            _rightHandType = WeaponType.bow;
            _leftHandWeaponData = weapon;
            _leftHandType = WeaponType.bow;
        }
        else if (leftSlot)
        {
            _leftHandWeaponData = weapon;
            _leftHandType = weapon.Weapongrp.WeaponType;
        }
        else
        {
            _rightHandWeaponData = weapon;
            _rightHandType = weapon.Weapongrp.WeaponType;
        }

        if (weapon.Weapongrp.WeaponType != WeaponType.none)
        { // Do not update for shields
            UpdateWeaponType(weapon.Weapongrp.WeaponType);
        }

        // Instantiating weapon
        for (int i = 0; i < weaponPrefabs.Length; i++)
        {
            GameObject prefab = weaponPrefabs[i];
            GameObject go = GameObject.Instantiate(prefab);
            go.SetActive(false);
            go.transform.name = "weapon";

            Transform targetBone;
            if (weapon.Weapongrp.WeaponType == WeaponType.none)
            {
                _leftHandWeapon = go.transform;
                targetBone = GetShieldBone();
            }
            else if (weapon.Weapongrp.WeaponType == WeaponType.bow || leftSlot)
            {
                _leftHandWeapon = go.transform;
                targetBone = GetLeftHandBone();
            }
            else if ((weapon.Weapongrp.WeaponType == WeaponType.dual || weapon.Weapongrp.WeaponType == WeaponType.fist) && i > 0)
            {
                _leftHandWeapon = go.transform;
                targetBone = GetLeftHandBone();
            }
            else
            {
                _rightHandWeapon = go.transform;
                targetBone = GetRightHandBone();
            }

            AttachWeaponInstanceToBone(go, prefab, targetBone);
            go.SetActive(true);

            if (weaponType == WeaponType.bow)
            {
                EquipArrow();
            }
        }
    }

    protected virtual void UpdateWeaponType(WeaponType weaponType) { }

    public virtual void UpdateWeaponAnim(WeaponAnimType value) { }

    protected virtual Transform GetLeftHandBone()
    {
        if (_leftHandBone == null)
        {
            _leftHandBone = transform.FindRecursive("Weapon_L_Bone")
                ?? transform.FindRecursive("weapon_l_bone")
                ?? transform.FindRecursive("Bow Bone")
                ?? transform.FindRecursive("bow_bone")
                ?? transform.FindRecursive("Sword Bone01")
                ?? transform.FindRecursive("Bip01_L_Hand")
                ?? transform.FindRecursive("Bip01 L Hand");

            if (_leftHandBone == null)
            {
                Debug.LogWarning($"[{transform.name}] Left hand bone was not assigned, please pre-assign bones to avoid unecessary load.");
            }
        }
        return _leftHandBone;
    }

    protected virtual Transform GetRightHandBone()
    {
        if (_rightHandBone == null)
        {
            _rightHandBone = transform.FindRecursive("Weapon_R_Bone")
                ?? transform.FindRecursive("weapon_r_bone")
                ?? transform.FindRecursive("Sword Bone")
                ?? transform.FindRecursive("sword_bone")
                ?? transform.FindRecursive("Bip01_R_Hand")
                ?? transform.FindRecursive("Bip01 R Hand");

            if (_rightHandBone == null)
            {
                Debug.LogWarning($"[{transform.name}] Right hand bone was not assigned, please pre-assign bones to avoid unecessary load.");
            }
        }
        return _rightHandBone;
    }

    protected virtual Transform GetShieldBone()
    {
        if (_shieldBone == null)
        {
            _shieldBone = transform.FindRecursive("Shield_L_Bone")
                ?? transform.FindRecursive("Shield Bone")
                ?? transform.FindRecursive("shield_bone")
                ?? transform.FindRecursive("Weapon_L_Bone")
                ?? transform.FindRecursive("Bip01_L_Hand")
                ?? transform.FindRecursive("Bip01 L Hand");

            if (_shieldBone == null)
            {
                Debug.LogWarning($"[{transform.name}] Shield bone was not assigned, please pre-assign bones to avoid unecessary load.");
            }
        }
        return _shieldBone;
    }

    public virtual void UnequipWeapon(bool leftSlot)
    {
        Transform weaponBone = leftSlot ? GetLeftHandBone() : GetRightHandBone();
        if (weaponBone != null)
        {
            for (int i = weaponBone.childCount - 1; i >= 0; i--)
            {
                Transform child = weaponBone.GetChild(i);
                // Do NOT destroy weapon_trail (ParticleSystem attached to weapon bone)
                if (child.name == "weapon" || child.name == "shield")
                {
                    Destroy(child.gameObject);
                }
            }
        }

        if (leftSlot)
        {
            Transform shieldBone = GetShieldBone();
            if (shieldBone != null)
            {
                for (int i = shieldBone.childCount - 1; i >= 0; i--)
                {
                    Transform child = shieldBone.GetChild(i);
                    if (child.name == "weapon" || child.name == "shield")
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        if (_rightHandType == WeaponType.bow || _leftHandType == WeaponType.bow)
        {
            UnEquipArrow();
        }

        if (leftSlot)
        {
            _leftHandWeapon = null;
            _leftHandWeaponData = null;
            _leftHandType = WeaponType.hand;
            UpdateWeaponAnim(WeaponAnimParser.GetWeaponAnim(_rightHandType == WeaponType.none ? WeaponType.hand : _rightHandType));
        }
        else
        {
            _rightHandWeapon = null;
            _rightHandWeaponData = null;
            _rightHandType = WeaponType.hand;
            UpdateWeaponAnim(WeaponAnimParser.GetWeaponAnim(_leftHandType == WeaponType.none ? WeaponType.hand : _leftHandType));
        }
    }

    public virtual void StartTrail()
    {
    }

    public virtual void StopTrail()
    {
    }

    public virtual void UpdateAppearance(Appearance oldAppearance, Appearance newAppearance)
    {
        if (oldAppearance.ShouldUpdateWeapons(newAppearance))
        {
            if (oldAppearance.ShouldUpdateColSize(newAppearance))
            {
                oldAppearance.CollisionHeight = newAppearance.CollisionHeight;
                oldAppearance.CollisionRadius = newAppearance.CollisionRadius;
            }

            EquipAllWeapons(newAppearance);
        }
    }

    protected virtual void LateUpdate()
    {
        UpdateArrowOrientation();
    }

    protected void UpdateArrowOrientation()
    {
        if (_arrow != null && _arrow.gameObject.activeInHierarchy)
        {
            Transform rightBone = GetRightHandBone();
            Transform leftBone = GetLeftHandBone();

            Vector3 pRight = rightBone != null ? rightBone.position : transform.position;

            Vector3 pLeft;
            if (_leftHandWeapon != null)
            {
                Transform handle = _leftHandWeapon.Find("handle");
                pLeft = handle != null ? handle.position : _leftHandWeapon.position;
            }
            else if (leftBone != null)
            {
                pLeft = leftBone.position;
            }
            else
            {
                pLeft = pRight + transform.forward * 0.7f;
            }

            Vector3 aimDir = pLeft - pRight;
            Vector3 forwardDir;
            if (aimDir.sqrMagnitude > 0.0001f)
            {
                forwardDir = aimDir.normalized;
            }
            else
            {
                forwardDir = transform.forward;
            }

            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(forwardDir, up)) > 0.99f)
            {
                up = Vector3.forward;
            }

            Vector3 forwardAxis = _arrowForwardAxis != Vector3.zero ? _arrowForwardAxis : Vector3.left;
            Quaternion targetWorldRot = Quaternion.LookRotation(forwardDir, up) * Quaternion.FromToRotation(forwardAxis, Vector3.forward);
            _arrow.rotation = targetWorldRot;

            Vector3 nockWorldOffset = _arrow.TransformVector(_arrowNockOffset);
            _arrow.position = pRight - nockWorldOffset;
        }
    }

    public static Vector3 GetArrowForwardAxis(GameObject arrowObj)
    {
        if (arrowObj == null) return Vector3.left;
        CalculateArrowAlignmentData(arrowObj, out Vector3 forwardAxis, out _);
        return forwardAxis;
    }

    public static void CalculateArrowAlignmentData(GameObject arrowObj, out Vector3 forwardAxis, out Vector3 nockOffset)
    {
        forwardAxis = Vector3.left;
        nockOffset = Vector3.zero;

        if (arrowObj == null) return;

        Transform handle = arrowObj.transform.Find("handle");
        if (handle == null)
        {
            handle = arrowObj.transform.FindRecursive("handle");
        }

        Bounds localBounds = CalculateLocalMeshBounds(arrowObj);
        Vector3 size = localBounds.size;

        if (handle != null)
        {
            nockOffset = arrowObj.transform.InverseTransformPoint(handle.position);
            Vector3 toCenter = localBounds.center - nockOffset;

            // Project toCenter onto primary axis
            if (Mathf.Abs(toCenter.x) >= Mathf.Abs(toCenter.y) && Mathf.Abs(toCenter.x) >= Mathf.Abs(toCenter.z))
            {
                forwardAxis = toCenter.x >= 0 ? Vector3.right : Vector3.left;
            }
            else if (Mathf.Abs(toCenter.z) >= Mathf.Abs(toCenter.x) && Mathf.Abs(toCenter.z) >= Mathf.Abs(toCenter.y))
            {
                forwardAxis = toCenter.z >= 0 ? Vector3.forward : Vector3.back;
            }
            else
            {
                forwardAxis = toCenter.y >= 0 ? Vector3.up : Vector3.down;
            }
        }
        else
        {
            // For native Lineage arrows (like wooden_arrow_m00):
            // Tip is at -X (localBounds.min.x), so forward direction is -X (Vector3.left).
            // Nock (feathers) is at +X (localBounds.max.x).
            if (size.x >= size.y && size.x >= size.z)
            {
                forwardAxis = Vector3.left;
                nockOffset = new Vector3(localBounds.max.x, localBounds.center.y, localBounds.center.z);
            }
            else if (size.z >= size.x && size.z >= size.y)
            {
                forwardAxis = Vector3.back;
                nockOffset = new Vector3(localBounds.center.x, localBounds.center.y, localBounds.max.z);
            }
            else
            {
                forwardAxis = Vector3.down;
                nockOffset = new Vector3(localBounds.center.x, localBounds.max.y, localBounds.center.z);
            }
        }
    }

    public static Bounds CalculateLocalMeshBounds(GameObject root)
    {
        Bounds totalBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;
        Transform rootTransform = root.transform;

        foreach (MeshFilter mf in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null) continue;
            Bounds b = mf.sharedMesh.bounds;
            AddTransformedBounds(rootTransform, mf.transform, b, ref totalBounds, ref hasBounds);
        }

        foreach (SkinnedMeshRenderer smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (smr.sharedMesh == null) continue;
            Bounds b = smr.sharedMesh.bounds;
            AddTransformedBounds(rootTransform, smr.transform, b, ref totalBounds, ref hasBounds);
        }

        if (!hasBounds)
        {
            totalBounds = new Bounds(Vector3.zero, new Vector3(0.5f, 0.05f, 0.05f));
        }

        return totalBounds;
    }

    private static void AddTransformedBounds(Transform root, Transform child, Bounds localBounds, ref Bounds totalBounds, ref bool hasBounds)
    {
        Matrix4x4 localToRoot = root.worldToLocalMatrix * child.localToWorldMatrix;
        Vector3 center = localBounds.center;
        Vector3 extents = localBounds.extents;

        Vector3[] corners = new Vector3[8]
        {
            center + new Vector3(-extents.x, -extents.y, -extents.z),
            center + new Vector3( extents.x, -extents.y, -extents.z),
            center + new Vector3(-extents.x,  extents.y, -extents.z),
            center + new Vector3( extents.x,  extents.y, -extents.z),
            center + new Vector3(-extents.x, -extents.y,  extents.z),
            center + new Vector3( extents.x, -extents.y,  extents.z),
            center + new Vector3(-extents.x,  extents.y,  extents.z),
            center + new Vector3( extents.x,  extents.y,  extents.z),
        };

        for (int i = 0; i < 8; i++)
        {
            Vector3 p = localToRoot.MultiplyPoint3x4(corners[i]);
            if (!hasBounds)
            {
                totalBounds = new Bounds(p, Vector3.zero);
                hasBounds = true;
            }
            else
            {
                totalBounds.Encapsulate(p);
            }
        }
    }
}
