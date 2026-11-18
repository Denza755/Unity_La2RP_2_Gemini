// ModernSpellEffects.cs – Unified HD effect system for Life Drain (Skill ID 1090)
// Placed in Assets/Scripts/Game/Manager/Effect/ within the Unity project.
// Implements cast ring, hit explosion, and heal flash using URP particle systems
// and procedural additive materials.

using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public static class ModernSpellEffects
{
    // Cached additive material for all particles
    private static Material _additiveMaterial;
    private static Texture2D _radialTexture;
    private const string MaterialName = "LifeDrain_AdditiveMat";

    // Ensure material exists with proper blend settings
    private static Material GetAdditiveMaterial()
    {
        if (_additiveMaterial != null) return _additiveMaterial;
        // Try to find existing material first (helps during hot‑reload)
        var existing = Resources.FindObjectsOfTypeAll<Material>();
        foreach (var mat in existing)
        {
            if (mat.name == MaterialName) { _additiveMaterial = mat; break; }
        }
        if (_additiveMaterial == null)
        {
            Shader unlit = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (unlit == null) unlit = Shader.Find("Sprites/Default"); // fallback
            _additiveMaterial = new Material(unlit) { name = MaterialName };
            // Additive transparency settings
            _additiveMaterial.SetFloat("_Surface", 1); // Transparent
            _additiveMaterial.SetFloat("_Blend", 2);   // Additive
            _additiveMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            _additiveMaterial.SetInt("_DstBlend", (int)BlendMode.One);
            _additiveMaterial.SetInt("_ZWrite", 0);
            _additiveMaterial.SetInt("_Cull", (int)CullMode.Off);
            _additiveMaterial.renderQueue = 3000;
        }
        return _additiveMaterial;
    }

    // Generate a 64×64 radial Gaussian texture (white centre -> transparent edge)
    private static Texture2D GenerateRadialTexture()
    {
        if (_radialTexture != null) return _radialTexture;
        int size = 64;
        _radialTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        _radialTexture.filterMode = FilterMode.Bilinear;
        Color32[] pixels = new Color32[size * size];
        Vector2 centre = new Vector2(size / 2f, size / 2f);
        float maxDist = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), centre) / maxDist;
                // Gaussian fall‑off (approximate)
                float intensity = Mathf.Exp(-dist * dist * 4f);
                byte v = (byte)(Mathf.Clamp01(intensity) * 255);
                pixels[y * size + x] = new Color32(v, v, v, v);
            }
        }
        _radialTexture.SetPixels32(pixels);
        _radialTexture.Apply();
        // Assign to material when first created
        GetAdditiveMaterial().mainTexture = _radialTexture;
        return _radialTexture;
    }

    // ---------- Public entry points ----------
    // Phase 1 – Cast ring under caster's feet
    public static void SpawnCastEffect(Entity caster, Skill skill)
    {
        if (caster == null) return;
        // Create root container under the world Effects object
        var parent = GameObject.Find("Effects")?.transform;
        if (parent == null) parent = new GameObject("Effects").transform;
        GameObject go = new GameObject("LifeDrain_CastRing");
        go.transform.parent = parent;
        go.transform.position = caster.transform.position + Vector3.up * 0.05f;
        // Main ring particle system
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.6f;
        main.startLifetime = 0.6f;
        main.startSpeed = 0f;
        main.startSize = 2f;
        main.startColor = new Color(1.8f, 0.12f, 0.18f, 0f); // HDR, will fade via ColorOverLifetime
        main.loop = false;
        main.playOnAwake = false;
        // Render mode – horizontal billboard (custom via shape?)
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
        renderer.material = GetAdditiveMaterial();
        // Color over lifetime – fade‑in then fade‑out
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1.8f, 0.12f, 0.18f, 0f), 0f),
                                    new GradientColorKey(new Color(1.8f, 0.12f, 0.18f, 1f), 0.1f),
                                    new GradientColorKey(new Color(1.8f, 0.12f, 0.18f, 0f), 0.6f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(0f, 0.6f) }
        );
        col.color = grad;
        // Add small rising spark particle system as child
        GameObject spark = new GameObject("LifeDrain_CastSpark");
        spark.transform.parent = go.transform;
        spark.transform.localPosition = Vector3.zero;
        var sparkPs = spark.AddComponent<ParticleSystem>();
        var sparkMain = sparkPs.main;
        sparkMain.duration = 0.6f;
        sparkMain.startLifetime = 0.4f;
        sparkMain.startSpeed = 0.3f;
        sparkMain.startSize = 0.2f;
        sparkMain.startColor = new Color(0.9f, 0.05f, 0.05f, 0.8f);
        sparkMain.loop = false;
        sparkMain.playOnAwake = false;
        var sparkRenderer = sparkPs.GetComponent<ParticleSystemRenderer>();
        sparkRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        sparkRenderer.material = GetAdditiveMaterial();
        // Turbulence noise via Noise module
        var noise = sparkPs.noise;
        noise.enabled = true;
        noise.strength = 0.2f;
        noise.frequency = 0.5f;
        // Trigger playing
        ps.Play();
        sparkPs.Play();
        // Cleanup after lifetime + small buffer
        Object.Destroy(go, 1.5f);
    }

    // Phase 2 – Hit explosion on target (m_u003_c / e_u003_a)
    public static void SpawnHitEffect(Entity target, Skill skill)
    {
        if (target == null) return;
        var parent = GameObject.Find("Effects")?.transform;
        if (parent == null) parent = new GameObject("Effects").transform;
        GameObject go = new GameObject("LifeDrain_HitExplosion");
        go.transform.parent = parent;
        go.transform.position = target.transform.position + Vector3.up * 0.85f;
        // Expanding ring
        var ring = go.AddComponent<ParticleSystem>();
        var main = ring.main;
        main.duration = 0.35f;
        main.startLifetime = 0.35f;
        main.startSpeed = 0f;
        main.startSize = 0.2f;
        main.startSize3D = true;
        main.startSizeX = 0.2f; main.startSizeY = 0.2f; main.startSizeZ = 0.01f;
        main.startColor = new Color(0.9f, 0.12f, 0.12f, 0f);
        main.loop = false;
        main.playOnAwake = false;
        var shape = ring.shape;
        shape.shapeType = ParticleSystemShapeType.Donut;
        shape.radius = 0.2f;
        shape.donutRadius = 0.2f;
        var sizeOver = ring.sizeOverLifetime;
        sizeOver.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.2f);
        sizeCurve.AddKey(1f, 1.3f);
        sizeOver.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        var col = ring.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.92f, 0.18f, 0.18f), 0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = grad;
        var rend = ring.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.material = GetAdditiveMaterial();
        // Spark burst
        GameObject sparks = new GameObject("LifeDrain_HitSparks");
        sparks.transform.parent = go.transform;
        sparks.transform.localPosition = Vector3.zero;
        var sparkPs = sparks.AddComponent<ParticleSystem>();
        var sparkMain = sparkPs.main;
        sparkMain.duration = 0.2f;
        sparkMain.startLifetime = 0.5f;
        sparkMain.startSpeed = 2f;
        sparkMain.startSize = 0.15f;
        sparkMain.startColor = new Color(0.9f, 0.12f, 0.12f);
        sparkMain.maxParticles = 30;
        sparkMain.loop = false;
        sparkMain.playOnAwake = false;
        var emit = sparkPs.emission;
        emit.enabled = true;
        emit.rateOverTime = 0f;
        emit.burstCount = 1;
        emit.SetBurst(0, new ParticleSystem.Burst(0f, 15, 20));
        var sparkRend = sparkPs.GetComponent<ParticleSystemRenderer>();
        sparkRend.renderMode = ParticleSystemRenderMode.Billboard;
        sparkRend.material = GetAdditiveMaterial();
        // Play both
        ring.Play();
        sparkPs.Play();
        Object.Destroy(go, 2f);
    }

    // Phase 3 – Heal flash on caster after drain return arrives
    public static void SpawnHealFlash(Entity caster, Skill skill)
    {
        if (caster == null) return;
        var parent = GameObject.Find("Effects")?.transform;
        if (parent == null) parent = new GameObject("Effects").transform;
        GameObject go = new GameObject("LifeDrain_HealFlash");
        go.transform.parent = parent;
        go.transform.position = caster.transform.position + Vector3.up * 0.95f;
        // Healing ring
        var ring = go.AddComponent<ParticleSystem>();
        var main = ring.main;
        main.duration = 0.4f;
        main.startLifetime = 0.4f;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0.6f, 0.2f, 0f);
        main.loop = false;
        main.playOnAwake = false;
        var col = ring.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.6f, 0.2f), 0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = grad;
        var rend = ring.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.material = GetAdditiveMaterial();
        // Upward spark burst
        GameObject sparks = new GameObject("Heal_Sparks");
        sparks.transform.parent = go.transform;
        sparks.transform.localPosition = Vector3.zero;
        var sparkPs = sparks.AddComponent<ParticleSystem>();
        var sparkMain = sparkPs.main;
        sparkMain.duration = 0.3f;
        sparkMain.startLifetime = 0.6f;
        sparkMain.startSpeed = 1.5f;
        sparkMain.startSize = 0.12f;
        sparkMain.startColor = new Color(1f, 0.8f, 0.5f);
        sparkMain.maxParticles = 12;
        sparkMain.loop = false;
        sparkMain.playOnAwake = false;
        var emit = sparkPs.emission;
        emit.enabled = true;
        emit.rateOverTime = 0f;
        emit.SetBurst(0, new ParticleSystem.Burst(0f, 10, 12));
        var sparkRender = sparkPs.GetComponent<ParticleSystemRenderer>();
        sparkRender.renderMode = ParticleSystemRenderMode.Billboard;
        sparkRender.material = GetAdditiveMaterial();
        // Play and auto‑destroy
        ring.Play();
        sparkPs.Play();
        Object.Destroy(go, 1.5f);
    }
}
