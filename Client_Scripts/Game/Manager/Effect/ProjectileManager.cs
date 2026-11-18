using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    private static ProjectileManager _instance;
    public static ProjectileManager Instance { get { return _instance; } }

    private List<PooledEffect> _activeProjectiles;

    [SerializeField] private float _hitHeightOffsetMultiplier = 1.0f;

    public List<PooledEffect> ActiveProjectiles
    {
        get
        {
            if (_activeProjectiles == null)
            {
                _activeProjectiles = new List<PooledEffect>();
            }

            return _activeProjectiles;
        }
        private set
        {
            _activeProjectiles = value;
        }
    }


    #region Initialization
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
    #endregion

    #region Projectile Management tasks
    private void Update()
    {
        ManageProjectiles();
    }

    private void ManageProjectiles()
    {
        if (ActiveProjectiles.Count > 0)
        {
            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--)
            {
                PooledEffect effect = ActiveProjectiles[i];

                if (effect == null || effect.Caster == null || effect.Target == null || effect.GameObject == null)
                {
                    Debug.LogWarning("Projectile doesn't have a caster or target.");
                    ActiveProjectiles.RemoveAt(i);
                    continue;
                }

                UpdateEffectTargetPosition(effect);

                float lerpRatio = (Time.time - effect.StartTime) / (effect.HitTime - effect.StartTime);
                lerpRatio = Mathf.Min(1, lerpRatio);

                Vector3 newPos = Vector3.Lerp(effect.StartingPosition, effect.TargetPosition, lerpRatio);
                Vector3 flightDir = effect.TargetPosition - effect.StartingPosition;
                if (flightDir.sqrMagnitude > 0.001f)
                {
                    Vector3 dir = flightDir.normalized;
                    Vector3 up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
                    Quaternion rot = Quaternion.LookRotation(dir, up);
                    if (IsPhysicalArrow(effect))
                    {
                        Vector3 forwardAxis = effect.ArrowForwardAxis != Vector3.zero ? effect.ArrowForwardAxis : Vector3.left;
                        rot *= Quaternion.FromToRotation(forwardAxis, Vector3.forward);
                    }
                    effect.GameObject.transform.rotation = rot;
                }
                effect.GameObject.transform.position = newPos;

                if (lerpRatio >= 1)
                {
                    ActiveProjectiles.RemoveAt(i);

                    if (IsPhysicalArrow(effect))
                    {
                        float randomPosX = Random.Range(-1f, 1f);
                        float randomPosY = Random.Range(-1f, 1f);

                        if (effect.HitSuccess)
                        {
                            effect.GameObject.transform.position += new Vector3(randomPosX, randomPosY, randomPosY) * 0.075f;

                            // Pierce target
                            if (effect.Target.AnimationController.RootBone != null)
                            {
                                effect.GameObject.transform.parent = effect.Target.AnimationController.RootBone; //rootbone
                            }
                            else
                            {
                                effect.GameObject.transform.parent = effect.Target.transform;
                            }
                        }
                        else
                        {
                            // Pierce the ground
                            effect.GameObject.transform.position += Vector3.up * 0.2f + new Vector3(randomPosX, randomPosY * 0.1f, randomPosY) * 0.2f;
                            effect.GameObject.transform.eulerAngles = new Vector3(effect.GameObject.transform.eulerAngles.x + randomPosX * 5f, effect.GameObject.transform.eulerAngles.y + randomPosX * 5f, effect.GameObject.transform.eulerAngles.z + 50 + randomPosX * 15f);
                        }

                        //TODO: Maybe spawn hit target particle? / ignore hit effect first targe when arrow is shot from damage packet?
                    }
                    else
                    {
                        if (effect.IsDrainReturn)
                        {
                            effect.OnArrival?.Invoke();
                            SpellDynamicLight.TriggerHitFlash(effect.Target, effect.Skill, effect.EffectClass, effect.TargetPosition);
                        }
                        else
                        {
                            //TODO: ignore hit effect on first target when skill is shot from skill shot packet?
                            WorldCombat.Instance.SkillProjectileHitTarget(effect.Caster, effect.Target, effect.Skill);
                            SpellDynamicLight.TriggerHitFlash(effect.Target, effect.Skill, effect.EffectClass, effect.TargetPosition);
                        }
                    }
                }
            }
        }
    }
    #endregion

    private void UpdateEffectTargetPosition(PooledEffect effect)
    {
        Vector3 targetPosition = effect.Target.transform.position; // shoot at the ground if missed

        if (effect.HitSuccess)
        {
            targetPosition += Vector3.up * effect.Target.Appearance.CollisionHeight * _hitHeightOffsetMultiplier;
        }

        effect.TargetPosition = targetPosition;
    }

    public static bool IsPhysicalArrow(PooledEffect effect)
    {
        if (effect == null || !effect.IsArrow) return false;
        if (effect.GameObject == null) return false;

        // Magic spell effects and skill particles have L2Particle or ParticleGroup components
        if (effect.GameObject.GetComponent<L2Particle>() != null ||
            effect.GameObject.GetComponentInChildren<ParticleGroup>() != null)
        {
            return false;
        }

        return true;
    }

    public void AddProjectile(PooledEffect effect)
    {
        UpdateEffectTargetPosition(effect);
        Vector3 flightDir = effect.TargetPosition - effect.StartingPosition;
        if (flightDir.sqrMagnitude > 0.001f)
        {
            Vector3 dir = flightDir.normalized;
            Vector3 up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
            Quaternion rot = Quaternion.LookRotation(dir, up);
            if (IsPhysicalArrow(effect))
            {
                Vector3 forwardAxis = effect.ArrowForwardAxis != Vector3.zero ? effect.ArrowForwardAxis : Vector3.left;
                rot *= Quaternion.FromToRotation(forwardAxis, Vector3.forward);
            }
            effect.GameObject.transform.rotation = rot;
        }
        else
        {
            effect.GameObject.transform.LookAt(effect.TargetPosition);
            if (IsPhysicalArrow(effect))
            {
                Vector3 forwardAxis = effect.ArrowForwardAxis != Vector3.zero ? effect.ArrowForwardAxis : Vector3.left;
                effect.GameObject.transform.rotation *= Quaternion.FromToRotation(forwardAxis, Vector3.forward);
            }
        }

        ActiveProjectiles.Add(effect);

        if (!IsPhysicalArrow(effect))
        {
            SpellDynamicLight.AttachProjectileLight(effect);
        }
    }

    public void DestroyAllProjectiles()
    {
        if (_activeProjectiles == null || _activeProjectiles.Count == 0)
        {
            return;
        }

        foreach (PooledEffect pooledEffect in _activeProjectiles)
        {
            if (pooledEffect != null && pooledEffect.GameObject != null)
            {
                Destroy(pooledEffect.GameObject);
            }
        }

        _activeProjectiles.Clear();
    }
}
