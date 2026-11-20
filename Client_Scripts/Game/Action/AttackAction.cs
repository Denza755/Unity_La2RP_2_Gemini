using UnityEngine;
using static AttackingState;

public class AttackAction : L2Action
{
    public AttackAction() : base() { }

    // Local action
    public override void UseAction()
    {
        // If has a target and attack key pressed
        if (TargetManager.Instance.HasTarget())
        {
            // Debug.LogWarning("Use attack action.");

            Entity targetEntity = TargetManager.Instance.Target;
            EntityType targetType = targetEntity.Identity.EntityType;

            bool canAttackWithoutCtrl = targetType == EntityType.Monster
                || targetEntity.Identity.IsAttackable
                || targetEntity.Identity.PvpFlag > 0
                || (targetEntity.Stats != null && targetEntity.Stats.Karma > 0);

            if (canAttackWithoutCtrl || InputManager.Instance.Ctrl)
            {
                TargetManager.Instance.SetAttackTarget(targetEntity);
                PlayerStateMachine.Instance.ChangeIntention(Intention.INTENTION_ATTACK);
            }
            else
            {
                PlayerStateMachine.Instance.ChangeIntention(Intention.INTENTION_INTERACT);
            }
        }
    }
}