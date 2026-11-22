// Used by MONSTERS
using UnityEngine;

public class NetworkMonsterCombat : NetworkCombat
{
    public NewMonsterAnimationController MonsterAnimationController
    {
        get
        {
            return _referenceHolder != null ? (NewMonsterAnimationController)_referenceHolder.NewAnimationController : null;
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
        var ncc = GetComponent<NetworkCharacterControllerReceive>();
        if (ncc != null)
        {
            ncc.ResetDestination();
        }
        MonsterAnimationController?.Die();
    }

    public override void OnRevive()
    {
        base.OnRevive();
        MonsterAnimationController?.Wait();
    }

    protected override void OnHit(Hit hit)
    {
        base.OnHit(hit);
    }

    public override bool AttackOnce(float hitTime, float atkEndTime, bool hitSuccess, Entity attackTarget)
    {
        if (base.AttackOnce(hitTime, atkEndTime, hitSuccess, attackTarget))
        {
            MonsterAnimationController?.Attack();
            return true;
        }
        else
        {
            return false;
        }
    }
}