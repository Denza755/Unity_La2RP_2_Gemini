using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class ViewerParticleGroup : MonoBehaviour
{
    const int CloneLimit = 64;
    Renderer[] _slots;
    float[] _spawnTimes;
    bool[] _active;
    float _duration;
    float _startedAt = -1f;
    bool _burst;
    float _rate = 15f;
    int _maxCount;
    int _spawned;
    int _index;
    uint _meshRand;
    uint _spriteRand;
    bool _hasMeshSpawn;
    bool _hasSpin;
    float _lastEmit = -999f;

    public static ViewerParticleGroup Spawn(
        Transform parent,
        string name,
        ViewerEmitterData emitter,
        Mesh mesh,
        Material sharedMaterial,
        Vector3 worldPos)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = worldPos;
        var group = root.AddComponent<ViewerParticleGroup>();
        group.Build(emitter, mesh, sharedMaterial);
        group.Play();
        return group;
    }

    void Build(ViewerEmitterData emitter, Mesh mesh, Material sharedMaterial)
    {
        _maxCount = Mathf.Clamp(emitter.MaxParticles, 1, CloneLimit);
        // Match the working ParticleGroup prefab: even with UC
        // AutomaticInitialSpawning=False and InitialParticlesPerSecond=10000,
        // _isBurstSpawning is false. The controller enables at most one slot
        // per FixedUpdate, producing the verified staggered color/fade phases.
        _burst = false;
        _rate = emitter.InitialParticlesPerSecond > 0.1f
            ? emitter.InitialParticlesPerSecond
            : 15f;
        _duration = emitter.MaxLifetime + 0.15f;
        _hasMeshSpawn = emitter.Kind == ViewerEmitterKind.Mesh &&
                        (Mathf.Abs(emitter.StartLocationZ.Min) + Mathf.Abs(emitter.StartLocationZ.Max) +
                         Mathf.Abs(emitter.StartVelocityZ.Min) + Mathf.Abs(emitter.StartVelocityZ.Max) > 0.0001f);
        _hasSpin = emitter.SpinParticles;
        _meshRand = (uint)Random.Range(1, int.MaxValue);
        _spriteRand = (uint)Random.Range(1, int.MaxValue);
        ViewerDiagnostics.Group(emitter, _maxCount, _burst, _rate, _duration);

        Mesh useMesh = mesh != null ? mesh : Quad();
        _slots = new Renderer[_maxCount];
        _spawnTimes = new float[_maxCount];
        _active = new bool[_maxCount];
        for (int i = 0; i < _maxCount; i++)
        {
            var go = new GameObject(emitter.DisplayName + "_" + i);
            go.transform.SetParent(transform, false);
            go.SetActive(false);
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = useMesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = sharedMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            _slots[i] = renderer;
        }
    }

    public void Play()
    {
        _startedAt = Time.time;
        _spawned = 0;
        _index = 0;
        _lastEmit = -999f;
        for (int i = 0; i < _slots.Length; i++)
        {
            _active[i] = false;
            if (_slots[i] != null)
                _slots[i].gameObject.SetActive(false);
        }

        if (_burst)
        {
            for (int i = 0; i < _maxCount; i++)
                Activate(Time.time);
        }
    }

    void FixedUpdate()
    {
        if (_startedAt < 0f || _burst || _spawned >= _maxCount)
            return;

        float now = Time.time;
        if (now - _lastEmit < 1f / Mathf.Max(_rate, 0.01f))
            return;

        _lastEmit = now;
        Activate(now);
    }

    void Update()
    {
        if (_startedAt < 0f)
            return;

        float now = Time.time;
        bool any = false;
        for (int i = 0; i < _slots.Length; i++)
        {
            if (!_active[i])
                continue;
            if (now - _spawnTimes[i] > _duration)
            {
                _active[i] = false;
                if (_slots[i] != null)
                    _slots[i].gameObject.SetActive(false);
                continue;
            }
            any = true;
        }

        if (!any && (_burst || _spawned >= _maxCount) && now - _startedAt > 0.05f)
            Destroy(gameObject);
        else if (now - _startedAt > _duration + 1.5f)
            Destroy(gameObject);
    }

    void Activate(float now)
    {
        if (_slots.Length == 0)
            return;
        int slot = _index % _slots.Length;
        _index++;
        _spawned++;
        Renderer renderer = _slots[slot];
        if (renderer == null)
            return;

        renderer.gameObject.SetActive(true);
        renderer.enabled = true;
        _active[slot] = true;
        _spawnTimes[slot] = now;

        L2AppRand.ResolveGpuInstanceRandBits(
            _hasMeshSpawn, _hasSpin, _meshRand, _spriteRand, slot,
            out float meshSpawnBits, out float startSpinBits,
            out float spriteMotionBits, out float spriteSpinBits);

        Material mat = renderer.material;
        if (mat.HasProperty("_StartTime"))
            mat.SetFloat("_StartTime", now);
        if (mat.HasProperty("_Seed"))
            mat.SetFloat("_Seed", Random.Range(-100f, 100f));
        if (mat.HasProperty("_MeshSpawnRandStateBits"))
            mat.SetFloat("_MeshSpawnRandStateBits", meshSpawnBits);
        if (mat.HasProperty("_StartSpinRandStateBits"))
            mat.SetFloat("_StartSpinRandStateBits", startSpinBits);
        if (mat.HasProperty("_SpriteMotionRandStateBits"))
            mat.SetFloat("_SpriteMotionRandStateBits", spriteMotionBits);
        if (mat.HasProperty("_SpriteSpinRandStateBits"))
            mat.SetFloat("_SpriteSpinRandStateBits", spriteSpinBits);
        if (mat.HasProperty("_OwnerWorldPos"))
            mat.SetVector("_OwnerWorldPos", transform.position);
        ViewerDiagnostics.Activate(name, slot, _spawned, now, _startedAt, mat);
    }

    static Mesh _quad;
    static Mesh Quad()
    {
        if (_quad != null)
            return _quad;
        _quad = new Mesh { name = "ViewerSpriteQuad" };
        _quad.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f)
        };
        _quad.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
        _quad.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        _quad.RecalculateNormals();
        _quad.RecalculateBounds();
        return _quad;
    }
}
