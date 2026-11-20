using UnityEngine;

public class AttackIntention : IntentionBase
{
    public AttackIntention(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter(object arg0)
    {
        if (TargetManager.Instance == null || TargetManager.Instance.Target == null || TargetManager.Instance.Target.transform == null)
        {
            return;
        }

        Entity targetEntity = TargetManager.Instance.Target;
        if (targetEntity.IsDead)
        {
            _stateMachine.ChangeIntention(Intention.INTENTION_IDLE);
            return;
        }

        TargetManager.Instance.SetAttackTarget(targetEntity);

        float attackRange = WorldCombat.Instance.GetRealAttackRange(PlayerEntity.Instance, targetEntity);
        Vector3 targetPos = targetEntity.transform.position;
        Vector3 playerPos = PlayerEntity.Instance.transform.position;

        float distance2D = VectorUtils.Distance2D(playerPos, targetPos);
        float deltaY = Mathf.Abs(playerPos.y - targetPos.y);

        // Ground combat in L2: 2D attack range + height difference check (150 uu = ~3.0m in Unity, 4.8m for bows uphill)
        bool isBow = PlayerEntity.Instance != null && PlayerEntity.Instance.Gear != null && PlayerEntity.Instance.Gear.WeaponType == WeaponType.bow;
        float maxDeltaY = isBow ? 4.8f : 3.0f;
        bool inRange = distance2D <= (attackRange + 0.1f) && deltaY <= maxDeltaY;

        if (inRange)
        {
            PlayerController.Instance.StopMoving();
            PlayerController.Instance.ResetDestination(false);
            PlayerController.Instance.UpdateFinalAngleToLookAt(targetEntity.transform);

            if (_stateMachine.WaitingForServerReply)
            {
                _stateMachine.ChangeState(PlayerState.IDLE);
                return;
            }

            Debug.Log("Attacking target in range");
            _stateMachine.ChangeState(PlayerState.IDLE);
            _stateMachine.NotifyEvent(Event.READY_TO_ATTACK);
        }
        else
        {
            if (_stateMachine.WaitingForServerReply)
            {
                return;
            }

            MoveReason reason = MoveReason.ATTACK;

            // Stop comfortably within attack range (85% of attackRange, min 0.5m)
            // This prevents threshold boundary jitter between MoveTo and inRange checks
            float stopRange = Mathf.Max(0.5f, attackRange * 0.85f);

            PathFinderController.Instance.MoveTo(targetPos, stopRange, () =>
            {
                _stateMachine.ChangeIntention(Intention.INTENTION_FOLLOW, reason);
            });
        }
    }

    public override void Exit() { }
    public override void Update()
    {

    }
}