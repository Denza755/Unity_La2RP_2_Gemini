using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class LayerCullDistance : MonoBehaviour
{
    [SerializeField] private float _decoCullDistance = 30f;
    [SerializeField] private bool _useSphericalCulling = true;

    private readonly float[] _distances = new float[32];

    private void OnEnable()
    {
        ApplyToAllCameras();
    }

    private void Start()
    {
        ApplyToAllCameras();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyToAllCameras();
    }
#endif

    public void ApplyToAllCameras()
    {
        if (Camera.main != null)
        {
            ApplyCulling(Camera.main);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying && SceneView.lastActiveSceneView != null)
        {
            ApplyCulling(SceneView.lastActiveSceneView.camera);
        }
#endif
    }

    private void ApplyCulling(Camera camera)
    {
        if (camera == null) return;

        System.Array.Clear(_distances, 0, _distances.Length);

        int decoLayer = LayerMask.NameToLayer("Deco");
        if (decoLayer >= 0)
        {
            _distances[decoLayer] = _decoCullDistance;
        }

        camera.layerCullDistances = _distances;
        camera.layerCullSpherical = _useSphericalCulling;
    }
}