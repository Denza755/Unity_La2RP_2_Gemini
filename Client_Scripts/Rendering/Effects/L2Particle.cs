using UnityEngine;

public class L2Particle : MonoBehaviour
{
    [SerializeField] private Transform _owner;
    [SerializeField] private Vector3 _surfaceNormal;
    [SerializeField] private PooledEffect _pooledEffect = new PooledEffect();
    [SerializeField] private ParticleGroup[] _particleGroups;

    public PooledEffect PooledEffect 
    { 
        get 
        { 
            if (_pooledEffect == null)
            {
                _pooledEffect = new PooledEffect();
                _pooledEffect.ResetTimerCallback = () => ResetTimer();
            }
            return _pooledEffect; 
        } 
    }
    public Vector3 SurfaceNormal { get { return _surfaceNormal; } set { _surfaceNormal = value; } }

    private void Awake()
    {
        if (_pooledEffect == null)
        {
            _pooledEffect = new PooledEffect();
        }
        _pooledEffect.ResetTimerCallback = () =>
        {
            ResetTimer();
        };
    }

    public void ResetTimer()
    {
        if (_particleGroups == null || _particleGroups.Length == 0)
        {
            _particleGroups = GetComponentsInChildren<ParticleGroup>();
        }

        if (_particleGroups != null)
        {
            for (int i = 0; i < _particleGroups.Length; i++)
            {
                if (_particleGroups[i] == null) continue;
                if (_owner != null)
                {
                    _particleGroups[i].OwnerPosition = _owner.position;
                }
                _particleGroups[i].SurfaceNormal = _surfaceNormal;
                _particleGroups[i].ResetTimer(_pooledEffect != null ? _pooledEffect.HitTime : 1f);
            }
        }
    }
}
