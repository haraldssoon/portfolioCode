using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States / Enemy / Infectacles / Chase")]
public class InfectaclesChaseState : InfectaclesBaseState
{

    public override void Run()
    {
        if (Infectacles.InLineOfSight())
        {
            Infectacles.stateMachine.TransitionTo<InfectaclesAttackState>();
        }
  
        Infectacles.ChasePlayer();
        Infectacles.transform.LookAt(Infectacles.CurrentGoalPosition);

        
    }

}
