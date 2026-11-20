using UnityEngine;

public class FollowIntention : IntentionBase
{
    public static MoveReason MoveReason { get; private set; }

    public FollowIntention(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter(object arg0)
    {
        if (_stateMachine.State == PlayerState.SITTING || _stateMachine.State == PlayerState.SIT_WAIT || _stateMachine.State == PlayerState.STANDING)
        {
            _stateMachine.ChangeIntention(Intention.INTENTION_STAND);
            return;
        }

        MoveReason = (MoveReason)arg0;

        // If following to attack, check if we are ALREADY within attack range of the target
        if (MoveReason == MoveReason.ATTACK && TargetManager.Instance != null && TargetManager.Instance.HasTarget())
        {
            Entity targetEntity = TargetManager.Instance.Target;
            if (targetEntity != null && !targetEntity.IsDead)
            {
                float attackRange = WorldCombat.Instance.GetRealAttackRange(PlayerEntity.Instance, targetEntity);
                float distance2D = VectorUtils.Distance2D(PlayerEntity.Instance.transform.position, targetEntity.transform.position);
                float deltaY = Mathf.Abs(PlayerEntity.Instance.transform.position.y - targetEntity.transform.position.y);

                if (distance2D <= (attackRange + 0.1f) && deltaY <= 3.0f)
                {
                    PlayerController.Instance.StopMoving();
                    PlayerController.Instance.ResetDestination(false);
                    _stateMachine.ChangeIntention(Intention.INTENTION_ATTACK);
                    return;
                }
            }
        }

        // If player is NOT running to destination and has no movement input, do not enter moving state!
        if (PlayerController.Instance != null && !PlayerController.Instance.RunningToDestination && !InputManager.Instance.Move)
        {
            PlayerController.Instance.IntentionToRun = false;
            if (MoveReason == MoveReason.ATTACK)
            {
                _stateMachine.ChangeIntention(Intention.INTENTION_ATTACK);
            }
            else
            {
                _stateMachine.ChangeIntention(Intention.INTENTION_IDLE);
            }
            return;
        }

        PlayerController.Instance.IntentionToRun = true;

        if (_stateMachine.IsInMovableState())
        {
            RunOrWalk(MoveReason);
        }
        else if (!_stateMachine.WaitingForServerReply)
        {
            _stateMachine.SetWaitingForServerReply(true);
            NetworkCharacterControllerShare.Instance.ForceShareMoveDirection();
        }
        else
        {
            PlayerController.Instance.StopMoving();
        }
    }

    public void RunOrWalk(MoveReason moveReason)
    {
        _stateMachine.ChangeState(PlayerState.MOVING, moveReason);
    }

    public override void Exit() { }

    public override void Update()
    {
        if (MoveReason == MoveReason.ATTACK)
        {
            if (TargetManager.Instance == null || !TargetManager.Instance.HasTarget())
            {
                PlayerController.Instance.StopMoving();
                PlayerController.Instance.ResetDestination(false);
                _stateMachine.ChangeIntention(Intention.INTENTION_IDLE);
                return;
            }

            Entity targetEntity = TargetManager.Instance.Target;
            if (targetEntity == null || targetEntity.IsDead)
            {
                PlayerController.Instance.StopMoving();
                PlayerController.Instance.ResetDestination(false);
                _stateMachine.ChangeIntention(Intention.INTENTION_IDLE);
                return;
            }

            float attackRange = WorldCombat.Instance.GetRealAttackRange(PlayerEntity.Instance, targetEntity);
            float distance2D = VectorUtils.Distance2D(PlayerEntity.Instance.transform.position, targetEntity.transform.position);
            float deltaY = Mathf.Abs(PlayerEntity.Instance.transform.position.y - targetEntity.transform.position.y);

            // Reached attack range during movement:
            if (distance2D <= (attackRange + 0.05f) && deltaY <= 3.0f)
            {
                PlayerController.Instance.StopMoving();
                PlayerController.Instance.ResetDestination(true);
                _stateMachine.ChangeIntention(Intention.INTENTION_ATTACK);
                return;
            }
        }
        else if (MoveReason == MoveReason.SKILL)
        {
            if (TargetManager.Instance == null || !TargetManager.Instance.HasTarget())
            {
                PlayerController.Instance.StopMoving();
                PlayerController.Instance.ResetDestination(false);
                _stateMachine.ChangeIntention(Intention.INTENTION_IDLE);
                return;
            }

            Entity targetEntity = TargetManager.Instance.Target;
            if (targetEntity == null || (targetEntity.IsDead && !SkillIntention.IsCorpseSkill(SkillIntention.LastSkillId)))
            {
                PlayerController.Instance.StopMoving();
                PlayerController.Instance.ResetDestination(false);
                _stateMachine.ChangeIntention(Intention.INTENTION_IDLE);
                return;
            }

            Skillgrp grp = SkillIntention.LastSkillGrp;
            float skillRange = SkillIntention.GetRealSkillRange(PlayerEntity.Instance, targetEntity, grp);
            float distance2D = VectorUtils.Distance2D(PlayerEntity.Instance.transform.position, targetEntity.transform.position);
            float deltaY = Mathf.Abs(PlayerEntity.Instance.transform.position.y - targetEntity.transform.position.y);

            // Reached skill range during follow:
            if (distance2D <= (skillRange + 0.15f) && deltaY <= 4.0f)
            {
                PlayerController.Instance.StopMoving();
                PlayerController.Instance.ResetDestination(true);
                _stateMachine.ChangeIntention(Intention.INTENTION_SKILL, SkillIntention.LastSkillInfo);
                return;
            }
        }
    }
}