using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States / Enemy / Infectacles / Roaming")]
public class InfectaclesRoamingState : InfectaclesBaseState
{
    public override void Enter()
    {
        Infectacles.FindNewRoamingPosition();
    }
    public override void Run()
    {
        if(!Infectacles.NavMeshAgent.hasPath && !Infectacles.InLineOfSight())
        {
            Infectacles.FindNewRoamingPosition();
        }

        if (Infectacles.InLineOfSight())
        {
            Infectacles.stateMachine.TransitionTo<InfectaclesAttackState>();
        }

        //Infectacles.MoveToPosition();
    }

}
