
using UnityEngine;

public class AttackingState : StateBase
{
    private float _lastAttackTime = 0f;

    public AttackingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter(object obj0)
    {
        _lastAttackTime = Time.time;
        NewPlayerAnimationController.Instance.Attack();
    }

    public override void Update()
    {
        if (InputManager.Instance.Move)
        {
            _stateMachine.ChangeIntention(Intention.INTENTION_MOVE);
        }
        else if (TargetManager.Instance.HasAttackTarget() && TargetManager.Instance.AttackTarget.Status.IsDead)
        {
            _stateMachine.ChangeIntention(Intention.INTENTION_IDLE);
        }
        else
        {
            // Automatically follow target if it moved
            Entity target = PlayerCombat.Instance.AttackTarget;
            Entity player = PlayerEntity.Instance;

            if (target == null || target.IsDead)
            {
                return;
            }

            float attackRange = WorldCombat.Instance.GetRealAttackRange(player, target);
            float distance = VectorUtils.Distance2D(player.transform.position, target.transform.position);
            float deltaY = Mathf.Abs(player.transform.position.y - target.transform.position.y);

            if ((distance > (attackRange + 0.15f) || deltaY > 3.0f) && !_stateMachine.WaitingForServerReply)
            {
                // Stop comfortably within attack range (85% of attackRange, min 0.5m)
                float stopRange = Mathf.Max(0.5f, attackRange * 0.85f);
                PathFinderController.Instance.MoveTo(target.transform.position, stopRange, () =>
                {
                    _stateMachine.ChangeIntention(Intention.INTENTION_FOLLOW, MoveReason.ATTACK);
                });
            }
        }
    }

    public override void HandleEvent(Event evt, object arg0)
    {
        switch (evt)
        {
            case Event.ATTACK_ALLOWED:
                if (Time.time - _lastAttackTime > 0.05f)
                {
                    _lastAttackTime = Time.time;
                    NewPlayerAnimationController.Instance.Attack();
                }
                break;
            case Event.CLICK_TO_MOVE:
                _stateMachine.ChangeIntention(Intention.INTENTION_MOVE_TO, (Vector3)arg0);
                break;
            case Event.ACTION_ALLOWED:
                NetworkCharacterControllerShare.Instance.ForceShareMoveDirection();
                if (_stateMachine.Intention == Intention.INTENTION_MOVE)
                {
                    if (!InputManager.Instance.Move)
                    {
                        _stateMachine.ChangeIntention(Intention.INTENTION_IDLE);
                        return;
                    }

                    _stateMachine.ChangeState(PlayerState.MOVING);
                }
                if (_stateMachine.Intention == Intention.INTENTION_MOVE_TO)
                {
                    if (!PlayerController.Instance.IntentionToRun)
                    {
                        _stateMachine.ChangeIntention(Intention.INTENTION_IDLE);
                        return;
                    }

                    //Set state as running first to change to movable state
                    _stateMachine.ChangeState(PlayerState.MOVING);

                    _stateMachine.ChangeIntention(Intention.INTENTION_MOVE_TO); //not giving an argument will use last position as destination
                }
                if (_stateMachine.Intention == Intention.INTENTION_FOLLOW)
                {
                    _stateMachine.ChangeState(PlayerState.MOVING, FollowIntention.MoveReason);
                }
                if (_stateMachine.Intention == Intention.INTENTION_IDLE)
                {
                    _stateMachine.ChangeState(PlayerState.IDLE);
                }
                if (_stateMachine.Intention == Intention.INTENTION_SIT)
                {
                    _stateMachine.ChangeState(PlayerState.SITTING);
                }
                break;
            // Auto attack stop event
            case Event.CANCEL:
                _stateMachine.ChangeState(PlayerState.IDLE);

                if (_stateMachine.Intention == Intention.INTENTION_FOLLOW)
                {
                    _stateMachine.ChangeIntention(Intention.INTENTION_ATTACK);
                }
                break;
            case Event.DEAD:
                _stateMachine.ChangeState(PlayerState.DEAD);
                break;
        }
    }

    public override void Exit()
    {
        // PlayerCombat.Instance.StopAttackStance();
        PlayerController.Instance.StopLookAt();
    }
}