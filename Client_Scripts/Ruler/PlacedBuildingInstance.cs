using UnityEngine;

public class PlacedBuildingInstance : MonoBehaviour
{
    [SerializeField] private int _serverBuildingId;
    [SerializeField] private string _prefabName;
    [SerializeField] private string _ownerName;

    public int ServerBuildingId
    {
        get => _serverBuildingId;
        set => _serverBuildingId = value;
    }

    public string PrefabName
    {
        get => _prefabName;
        set => _prefabName = value;
    }

    public string OwnerName
    {
        get => _ownerName;
        set => _ownerName = value;
    }
}
