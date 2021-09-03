using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Healthy;


[CreateAssetMenu(menuName = "States / Enemy / Bellusraptor / Attack")]
public class BellusraptorAttackState : BellusraptorBaseState
{

    public override void Enter()
    {

        //För att räkna attackTimern
        Bellusraptor.AttackTimeCounter = Bellusraptor.GameTimer;

        //attackerar spelaren direkt, efter det är det en delay på AttackTimer
        Bellusraptor.Attack();
    }
    public override void Run()
    {
        if (Bellusraptor.IsDead)
        {
            Bellusraptor.StopMoving();
            return;
        }

        //om spelaren är utanför attackRange, gå tillbaka till ChaseState
        if (Bellusraptor.DistanceToPlayer >= Bellusraptor.AttackRange)
        {
            Bellusraptor.stateMachine.TransitionTo<BellusraptorChaseState>();
        }
        //om spelaren är i range och attackSwingen är redo, gör attack.
        if (Bellusraptor.GameTimer >= (Bellusraptor.AttackTimeCounter + Bellusraptor.TimeBetweenAttacks))
        {
            Bellusraptor.Attack();
        }
        Bellusraptor.LookAtPlayer();
    }

    public override void Exit()
    {
        Bellusraptor.Anim.SetBool("IsBiting", false);
    }
}
