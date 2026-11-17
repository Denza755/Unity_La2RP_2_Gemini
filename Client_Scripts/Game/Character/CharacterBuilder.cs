using UnityEngine;

public class CharacterBuilder : MonoBehaviour
{
    private static CharacterBuilder _instance;
    public static CharacterBuilder Instance { get { return _instance; } }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
    }

    // Load player animations, face and hair
    public GameObject BuildCharacterBase(CharacterModelType raceId, PlayerAppearance appearance, EntityType entityType)
    {

        //Debug.Log($"Building character: Race:{raceId} Sex:{appearance.Sex} Face:{appearance.Face} Hair:{appearance.HairStyle} HairC:{appearance.HairColor}");

        Entity entity;
        GameObject containerPrefab = ModelTable.Instance.GetContainer(raceId, entityType);
        if (containerPrefab == null)
        {
            Debug.LogError($"Character base container {raceId} for {entityType} is null in ModelTable.");
            return null;
        }

        GameObject go = Instantiate(containerPrefab);
        bool isOrc = raceId == CharacterModelType.MOrc || raceId == CharacterModelType.FOrc ||
                     raceId == CharacterModelType.MShaman || raceId == CharacterModelType.FShaman;

        go.TryGetComponent<Entity>(out entity);
        if (entity == null)
        {
            Debug.LogError($"Character base {raceId} missing entity class.");
        }
        else
        {
            entity.RaceId = raceId;
            float defColHeight = CharacterModelHelper.GetColHeight(raceId);
            float defColRadius = CharacterModelHelper.GetColRadius(raceId);
            entity.Appearance.CollisionHeight = appearance.CollisionHeight > 0 ? appearance.CollisionHeight : defColHeight;
            entity.Appearance.CollisionRadius = appearance.CollisionRadius > 0 ? appearance.CollisionRadius : defColRadius;
        }

        if (isOrc)
        {
            Transform model = go.transform.Find("model") ?? (go.transform.childCount > 0 ? go.transform.GetChild(0) : null);
            float scaleFactor = CharacterModelHelper.GetOrcScaleFactor(raceId);

            CharacterController cc = go.GetComponent<CharacterController>();
            if (cc != null)
            {
                float targetHeight = CharacterModelHelper.GetControllerHeight(raceId);
                float targetRadius = CharacterModelHelper.GetControllerRadius(raceId);

                cc.skinWidth = 0.01f;
                cc.height = targetHeight;
                cc.radius = targetRadius;
                cc.center = new Vector3(0, targetHeight * 0.5f, 0);
                cc.stepOffset = Mathf.Min(0.3f, targetHeight * 0.35f);
            }

            if (model != null)
            {
                model.localScale = Vector3.one * scaleFactor;
                // If CharacterController is present, its bottom contact surface is at center.y - height * 0.5f - skinWidth = -0.01f.
                // Resting on the ground places transform.position.y at +0.01f (skinWidth).
                // Offsetting model by -skinWidth ensures the bottom of boots (_b) is exactly at ground level y = 0 with no levitation.
                float groundCompensation = (cc != null) ? -cc.skinWidth : 0f;
                model.localPosition = new Vector3(0, groundCompensation, 0);
            }
        }

        Transform clickArea = go.transform.FindRecursive("click_area");
        if (clickArea != null)
        {
            float targetHeight = CharacterModelHelper.GetControllerHeight(raceId);
            float targetRadius = CharacterModelHelper.GetControllerRadius(raceId);
            clickArea.localScale = new Vector3(Mathf.Max(0.6f, targetRadius * 2.2f), targetHeight, Mathf.Max(0.6f, targetRadius * 2.2f));
            clickArea.localPosition = new Vector3(0, targetHeight * 0.5f, 0);
        }

        foreach (Animator anim in go.GetComponentsInChildren<Animator>(true))
        {
            if (anim.GetComponent<L2CharacterAnimationEvents>() == null)
            {
                anim.gameObject.AddComponent<L2CharacterAnimationEvents>();
            }
        }

        if (go.GetComponent<L2CharacterAnimationEvents>() == null)
        {
            go.AddComponent<L2CharacterAnimationEvents>();
        }

        return go;
    }
}
