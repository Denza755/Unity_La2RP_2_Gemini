using UnityEngine;
using System.Collections.Generic;
using System;

public class ParticleEffectTable
{
    private static ParticleEffectTable _instance;
    public static ParticleEffectTable Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ParticleEffectTable();
            }

            return _instance;
        }
    }

    public Dictionary<string, GameObject> ParticleEffects { get; private set; }
    public GameObject[,] SoulshotHitParticles { get; private set; }
    public GameObject[] DefaultHitParticles { get; private set; }
    public GameObject LocatorParticle { get; private set; }
    public GameObject LocatorReachedParticle { get; private set; }

    public void Initialize()
    {
        CacheEffects();
    }

    public void CacheEffects()
    {
        ParticleEffects = new Dictionary<string, GameObject>();

        LoadSkillParticles();

        // Locator?
        LocatorParticle = LoadEffectByName("e_u093_a");
        LocatorReachedParticle = LoadEffectByName("e_u093_b");

        // Hit particles
        DefaultHitParticles = new GameObject[2];
        DefaultHitParticles[0] = LoadEffectByName("p_u002_a"); // Load whenever any hit occurs (with and without crit)
        DefaultHitParticles[1] = LoadEffectByName("p_u004_a"); // Load when soulshot disabled and crit occurs
        SoulshotHitParticles = new GameObject[12, 2];
        SoulshotHitParticles[0, 0] = LoadEffectByName("shot_N_atk"); // SS No-Grade no-crit
        SoulshotHitParticles[0, 1] = LoadEffectByName("shot_N_cr"); // SS No-Grade crit
        SoulshotHitParticles[1, 0] = LoadEffectByName("shot_D_atk"); // SS D-Grade no-crit
        SoulshotHitParticles[1, 1] = LoadEffectByName("shot_D_cr"); // SS D-Grade crit
        SoulshotHitParticles[2, 0] = LoadEffectByName("shot_C_atk"); // SS C-Grade no-crit
        SoulshotHitParticles[2, 1] = LoadEffectByName("shot_C_cr"); // SS C-Grade crit
        SoulshotHitParticles[3, 0] = LoadEffectByName("shot_B_atk"); // SS B-Grade no-crit
        SoulshotHitParticles[3, 1] = LoadEffectByName("shot_B_cr"); // SS B-Grade crit
        SoulshotHitParticles[4, 0] = LoadEffectByName("shot_A_atk"); // SS A-Grade no-crit
        SoulshotHitParticles[4, 1] = LoadEffectByName("shot_A_cr"); // SS A-Grade crit
        SoulshotHitParticles[5, 0] = LoadEffectByName("shot_S_atk"); // SS S-Grade no-crit
        SoulshotHitParticles[5, 1] = LoadEffectByName("shot_S_cr"); // SS S-Grade crit
    }

    private void LoadSkillParticles()
    {
        // Skills particle effects
        foreach (KeyValuePair<int, L2SkillEffect> kvp in SkillEffectTable.Instance.SkillEffects)
        {
            if (kvp.Value.CastingActions != null && kvp.Value.CastingActions.Count > 0)
            {
                kvp.Value.CastingActions.ForEach((emitter) =>
                {
                    string effectClass = emitter.EffectClass;

                    GameObject particle = LoadEffectEmitter(effectClass);
                    if (particle != null)
                    {
                        ParticleEffects[effectClass] = particle;
                        // Debug.Log($"Loaded particle effect {particle} in ParticleEffectTable.");
                    }


                    effectClass = emitter.SecondaryEffectClass;

                    if (effectClass != null)
                    {
                        particle = LoadEffectEmitter(effectClass);
                        if (particle != null)
                        {
                            ParticleEffects[effectClass] = particle;
                            // Debug.Log($"Loaded secondary particle effect {particle} in ParticleEffectTable.");
                        }
                    }
                });
            }

            if (kvp.Value.ShotActions != null && kvp.Value.ShotActions.Count > 0)
            {
                kvp.Value.ShotActions.ForEach((emitter) =>
                {
                    string effectClass = emitter.EffectClass;

                    GameObject particle = LoadEffectEmitter(effectClass);
                    if (particle != null)
                    {
                        ParticleEffects[effectClass] = particle;
                        // Debug.Log($"Loaded particle effect {particle} in ParticleEffectTable.");
                    }
                });
            }


            if (kvp.Value.ExplosionActions != null && kvp.Value.ExplosionActions.Count > 0)
            {
                kvp.Value.ExplosionActions.ForEach((emitter) =>
                {
                    string effectClass = emitter.EffectClass;

                    GameObject particle = LoadEffectEmitter(effectClass);
                    if (particle != null)
                    {
                        ParticleEffects[effectClass] = particle;
                        Debug.Log($"Loaded particle effect {particle} in ParticleEffectTable.");
                    }
                });
            }
        }
    }

    private GameObject LoadEffectEmitter(string effectClass)
    {
        if (effectClass == null || effectClass.Length == 0)
        {
            return null;
        }

        if (ParticleEffects.ContainsKey(effectClass))
        {
            return null;
        }

        return LoadEffectByName(effectClass);
    }

    private static readonly Dictionary<string, string> _effectPathAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "bl_curse_poison_ta", "Data/Effects/curse_poison_deprecated/bl_curse_poison_ta/bl_curse_poison_ta" },
        { "bl_curse_poison_ca", "Data/Effects/curse_poison_deprecated/bl_curse_poison_ca/bl_curse_poison_ca" },
        { "el_ice_bolt_ca", "Data/Effects/el_ice_bolt_deprecated/el_ice_bolt_ca/el_ice_bolt_ca" },
        { "el_ice_bolt_ta", "Data/Effects/el_ice_bolt_deprecated/el_ice_bolt_ta/el_ice_bolt_ta" },
        { "el_wind_strike_ca", "Data/Effects/el_wind_strike_deprecated/el_wind_strike_ca/el_wind_strike_ca" },
        { "wh_heal_ca", "Data/Effects/wh_heal_deprecated/wh_heal_ca/wh_heal_ca" },
        { "wh_heal_ta", "Data/Effects/wh_heal_ta/wh_heal_ta" },
        { "wh_shield_ta", "Data/Effects/shield_deprecate/wh_shield_ta/wh_shield_ta" },
        { "wh_cure_poison_ta", "Data/Effects/cure_posion/wh_cure_poison_ta/bl_cure_poison_ta" },
        { "su_might_ta", "Data/Effects/might_deprecate/wh_might_ta/wh_might_ta" },
        { "m_u003_a", "Data/Effects/vampiric/m_u003_b/m_u003_b" },
        { "m_u003_b", "Data/Effects/vampiric/m_u003_b/m_u003_b" },
        { "m_u003_c", "Data/Effects/vampiric/m_u003_c/m_u003_c" },
        { "bl_berserker_spirit_ca", "Data/Effects/mo_war_cry_ca/mo_war_cry_ca" },
        { "sa_mass_frenzy_ca", "Data/Effects/mo_war_cry_ca/mo_war_cry_ca" },
        { "dw_spoil_ca", "Data/Effects/curse_weakness/curse_weakness_composite" },
        { "dw_spoil_ta", "Data/Effects/curse_weakness/d_mon_fire_ta/d_mon_fire_ta" },
        { "m_u025_a", "Data/Effects/e_u004_a/e_u004_a" },
        { "m_u025_b", "Data/Effects/e_u005_b/e_u005_b/e_u005_b" },
        { "m_u025_c", "Data/Effects/e_u005_b/e_u005_b/e_u005_b" },
        { "m_u028_a", "Data/Effects/e_u028_c/e_u028_c" },
        { "m_u028_b", "Data/Effects/e_u028_c/e_u028_c" },
        { "m_u028_c", "Data/Effects/e_u028_c/e_u028_c" },
        { "s_u006_a", "Data/Effects/s_u001_a/s_u001_a" },
        { "s_u006_b", "Data/Effects/s_u001_b/s_u001_b" },
        { "s_u019_a", "Data/Effects/s_u003_a/s_u003_a" },
        { "s_u019_b", "Data/Effects/s_u003_b/s_u003_b" },
        { "s_u019_c", "Data/Effects/s_u003_d/s_u003_d" },
        { "s_devotion_of_shine_ta", "Data/Effects/wh_heal_ta/wh_heal_ta" }
    };

    private GameObject LoadEffectByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        string cleanName = name;
        if (cleanName.StartsWith("ClassLineageEffect.", StringComparison.OrdinalIgnoreCase))
        {
            cleanName = cleanName.Substring("ClassLineageEffect.".Length);
        }

        // 1. Direct standard path Data/Effects/{cleanName}/{cleanName}
        string path = $"Data/Effects/{cleanName}/{cleanName}";
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
            return prefab;

        // 2. Direct flat path Data/Effects/{cleanName}
        path = $"Data/Effects/{cleanName}";
        prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
            return prefab;

        // 3. Direct path in Data/Particles/{cleanName}
        path = $"Data/Particles/{cleanName}";
        prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
            return prefab;

        path = $"Data/Particles/{cleanName}/{cleanName}";
        prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
            return prefab;

        // 4. Known aliases and nested folders
        if (_effectPathAliases.TryGetValue(cleanName, out string aliasPath))
        {
            prefab = Resources.Load<GameObject>(aliasPath);
            if (prefab != null)
                return prefab;
        }

        // Debug.LogWarning($"Can't find particle effect {name} (cleaned: {cleanName}).");
        return null;
    }
}
