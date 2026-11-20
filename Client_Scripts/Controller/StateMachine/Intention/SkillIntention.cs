using UnityEngine;

public class SkillIntention : IntentionBase
{
    private static SkillInfo _lastSkillIntention = null;
    private static Skillgrp _lastSkillIntentionGrp = null;
    public static bool CtrlPressed = false;
    public static bool ShiftPressed = false;

    public static int LastSkillId => _lastSkillIntention != null ? _lastSkillIntention.Id : 0;
    public static SkillInfo LastSkillInfo => _lastSkillIntention;
    public static Skillgrp LastSkillGrp => _lastSkillIntentionGrp;

    public static float GetRealSkillRange(Entity attacker, Entity target, Skillgrp grp)
    {
        float baseRange = (grp != null && grp.CastRange > 0) ? (grp.CastRange / 52.5f) : (40f / 52.5f);
        if (baseRange < (40f / 52.5f))
        {
            baseRange = 40f / 52.5f;
        }

        float attackerRadius = (attacker != null && attacker.Appearance != null && attacker.Appearance.CollisionRadius > 0)
            ? attacker.Appearance.CollisionRadius
            : (15f / 52.5f);

        float targetRadius = (target != null && target.Appearance != null && target.Appearance.CollisionRadius > 0)
            ? target.Appearance.CollisionRadius
            : (15f / 52.5f);

        return baseRange + attackerRadius + targetRadius;
    }

    public SkillIntention(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter(object arg0)
    {
        if (arg0 != null)
        {
            _lastSkillIntention = (SkillInfo)arg0;
            CtrlPressed = InputManager.Instance.Ctrl;
            ShiftPressed = InputManager.Instance.Shift;
            _lastSkillIntentionGrp = SkillgrpTable.Instance.GetSkillgrp(_lastSkillIntention.Id)?[_lastSkillIntention.Level];
        }

        if (_lastSkillIntention == null)
        {
            return;
        }

        //TODO: Use skill levels skillgrp
        if (_lastSkillIntentionGrp == null || _lastSkillIntentionGrp.Id != _lastSkillIntention.Id)
        {
            Debug.LogWarning($"Can't retrieve skillgrp for skill id: {_lastSkillIntentionGrp.Id}.");
            return;
        }

        if (SkillRequiresTarget()) // target required?
        {
            Transform target = TargetManager.Instance.Target?.transform;
            if (target == null)
            {
                if (SkillIsABuffOrHeal())
                {
                    TargetManager.Instance.SetTarget(PlayerEntity.Instance);
                }
                else
                {
                    Debug.Log("Skill requires a target but no target was selected.");
                    return;
                }
            }

            float skillRange = GetRealSkillRange(PlayerEntity.Instance, targetEntity, _lastSkillIntentionGrp);
            Entity targetEntityRef = TargetManager.Instance.Target;
            Vector3 targetPos = targetEntityRef.transform.position;

            float distance = VectorUtils.Distance2D(PlayerEntity.Instance.transform.position, targetPos);

            if (distance <= (skillRange + 0.1f) && !_stateMachine.WaitingForServerReply)
            {
                Debug.Log("Using skill on target");

                PlayerController.Instance.StopMoving();
                PlayerController.Instance.ResetDestination(false);
                _stateMachine.ChangeState(PlayerState.IDLE);

                if (!targetEntityRef.IsDead || IsCorpseSkill(_lastSkillIntention.Id))
                {
                    _stateMachine.NotifyEvent(Event.READY_TO_SKILL, _lastSkillIntention);
                }
            }
            else
            {
                // Move to target range
                PathFinderController.Instance.MoveTo(targetPos, skillRange * 0.85f, () =>
                {
                    _stateMachine.ChangeIntention(Intention.INTENTION_FOLLOW, MoveReason.SKILL);
                });
            }
        }
        else
        {
            _stateMachine.NotifyEvent(Event.READY_TO_SKILL, _lastSkillIntention);
        }
    }

    private bool SkillRequiresTarget()
    {
        Skillgrp skillgrp = _lastSkillIntentionGrp;
        bool hasRange = skillgrp.CastRange != -1;
        // bool isDamage = skillgrp.IconType == SkillIconType.Physical || skillgrp.IconType == SkillIconType.Magic;
        // bool isDebuff = skillgrp.IconType == SkillIconType.Debuff;
        // bool isBuff = skillgrp.IconType == SkillIconType.Buff;

        bool result = hasRange;// && (isDamage || isDebuff || isBuff);

        if (result)
        {
            Debug.Log($"Skill {_lastSkillIntentionGrp.Id} requires a target.");
        }
        else
        {
            Debug.Log($"Skill {_lastSkillIntentionGrp.Id} doesn't require a target.");
        }

        return result;
    }

    private bool SkillIsABuffOrHeal()
    {
        Skillgrp skillgrp = _lastSkillIntentionGrp;
        bool isBuff = skillgrp.IconType == SkillType.Buff;
        bool isHeal = skillgrp.OperateType == SkillOperateType.Heal;
        //TODO: 
        // Create an operate type for heals so that they can behave like buffs 
        // (auto target self when no target selected)
        bool result = isBuff || isHeal;
        if (result)
        {
            Debug.Log($"Skill {_lastSkillIntentionGrp.Id} is a buff/heal.");
        }
        else
        {
            Debug.Log($"Skill {_lastSkillIntentionGrp.Id} isn't a buff/heal.");
        }

        return result;
    }

    public static bool IsCorpseSkill(int skillId)
    {
        switch (skillId)
        {
            case 42: // Sweeper
            case 2098: // Harvesting
            case 1016: // Resurrection
            case 1017: // Mass Resurrection
            case 1044: // Blessed Resurrection
            case 1049: // Corpse Life Drain
            case 1050: // Corpse Plague
            case 1146: // Corpse Burst
            case 1147: // Corpse Vampire
            case 1254:
            case 1256:
            case 1258:
            case 1303:
            case 2014:
            case 2042:
            case 2062:
                return true;
            default:
                return false;
        }
    }

    public override void Exit() { }
    public override void Update()
    {

    }
}