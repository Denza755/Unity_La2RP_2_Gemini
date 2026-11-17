
// Used by MONSTERS
using UnityEngine;

public class NetworkMonsterEntity : NetworkEntity
{
    public NewMonsterAnimationController MonsterAnimationController { get { return (NewMonsterAnimationController)_referenceHolder.NewAnimationController; } }

    public override void Initialize()
    {
        base.Initialize();

        EntityLoaded = true;
    }

    public override void OnStopMoving()
    {
        base.OnStopMoving();
        if (MonsterAnimationController != null)
        {
            MonsterAnimationController.Wait();
        }
    }

    public override void OnStartMoving(bool walking)
    {
        base.OnStartMoving(walking);
        if (MonsterAnimationController != null)
        {
            if (walking)
            {
                MonsterAnimationController.Walk();
            }
            else
            {
                MonsterAnimationController.Run();
            }
        }
    }
}
