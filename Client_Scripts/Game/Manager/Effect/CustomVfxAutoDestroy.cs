using UnityEngine;

public class CustomVfxAutoDestroy : MonoBehaviour
{
    public float maxLifetime = 5.0f;

    private float _spawnTime;
    private ParticleSystem[] _particles;
    private TrailRenderer[] _trails;

    private void Awake()
    {
        _spawnTime = Time.time;
        _particles = GetComponentsInChildren<ParticleSystem>();
        _trails = GetComponentsInChildren<TrailRenderer>();
    }

    private void Start()
    {
        if (_particles != null)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i] != null && !_particles[i].isPlaying)
                {
                    _particles[i].Play();
                }
            }
        }
    }

    private void Update()
    {
        float elapsed = Time.time - _spawnTime;
        if (elapsed >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (elapsed > 0.25f)
        {
            bool isAlive = false;
            if (_particles != null)
            {
                for (int i = 0; i < _particles.Length; i++)
                {
                    if (_particles[i] != null && (_particles[i].IsAlive(true) || _particles[i].isPlaying))
                    {
                        isAlive = true;
                        break;
                    }
                }
            }

            if (!isAlive && _trails != null)
            {
                for (int i = 0; i < _trails.Length; i++)
                {
                    if (_trails[i] != null && _trails[i].positionCount > 0)
                    {
                        isAlive = true;
                        break;
                    }
                }
            }

            if (!isAlive)
            {
                Destroy(gameObject);
            }
        }
    }

    public static CustomVfxAutoDestroy Attach(GameObject go, float lifetime)
    {
        if (go == null) return null;
        CustomVfxAutoDestroy comp = go.GetComponent<CustomVfxAutoDestroy>();
        if (comp == null)
        {
            comp = go.AddComponent<CustomVfxAutoDestroy>();
        }
        comp.maxLifetime = lifetime;
        return comp;
    }
}

public class CustomVfxProjectile : MonoBehaviour
{
    private Transform _targetTransform;
    private Vector3 _targetOffset;
    private float _duration;
    private float _startTime;
    private Vector3 _startPos;
    private GameObject _hitPrefab;
    private Transform _effectContainer;

    public void Launch(Transform target, Vector3 targetOffset, float duration, GameObject hitPrefab, Transform container)
    {
        _targetTransform = target;
        _targetOffset = targetOffset;
        _duration = Mathf.Max(0.08f, duration);
        _startTime = Time.time;
        _startPos = transform.position;
        _hitPrefab = hitPrefab;
        _effectContainer = container;
    }

    private void Update()
    {
        float elapsed = Time.time - _startTime;
        float progress = Mathf.Clamp01(elapsed / _duration);

        Vector3 currentTargetPos = _targetTransform != null 
            ? _targetTransform.position + _targetOffset 
            : _startPos + transform.forward * 10f;

        transform.position = Vector3.Lerp(_startPos, currentTargetPos, progress);
        Vector3 dir = currentTargetPos - transform.position;
        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(dir.normalized);
        }

        if (progress >= 1.0f)
        {
            if (_hitPrefab != null && _targetTransform != null)
            {
                GameObject hitObj = Instantiate(_hitPrefab, currentTargetPos, Quaternion.identity, _effectContainer);
                CustomVfxAutoDestroy.Attach(hitObj, 4.0f);
            }

            var particles = GetComponentsInChildren<ParticleSystem>();
            foreach (var p in particles)
            {
                p.Stop();
            }

            CustomVfxAutoDestroy.Attach(gameObject, 0.8f);
            enabled = false;
        }
    }
}
