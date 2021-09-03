using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Healthy;
using Events;

[CreateAssetMenu(menuName = "States / Enemy / Bellusraptor / Roaming")]
public class BellusraptorRoamingState : BellusraptorBaseState
{
    private int currentHealth;
    

    public override void Enter()
    {
        currentHealth = Bellusraptor.EnemyHealth;
        Bellusraptor.ChangeMovementSpeed(Bellusraptor.WalkingSpeed);
        Bellusraptor.Anim.SetBool("IsWalking", true);
    }
    public override void Run()
    {

        if (Bellusraptor.IsDead)
        {
            Bellusraptor.StopMoving();
            return;
        }

        Bellusraptor.StandingStillTimer += Time.deltaTime;

        if (Bellusraptor.EnemyHealth < currentHealth)
        {
            Bellusraptor.stateMachine.TransitionTo<BellusraptorChaseState>();
            //Bellusraptor.GetComponent<Health>().AlertEnemiesInRange(Bellusraptor.transform, Bellusraptor.AlertRange);
            AlertEnemyEvent e = new AlertEnemyEvent(Bellusraptor.gameObject, Bellusraptor.AlertRange);
            e.FireEvent();
        }

        if (Bellusraptor.InLineOfSight())
        {
            Bellusraptor.stateMachine.TransitionTo<BellusraptorChaseState>();
            //Bellusraptor.GetComponent<Health>().AlertEnemiesInRange(Bellusraptor.transform, Bellusraptor.AlertRange);
            AlertEnemyEvent e = new AlertEnemyEvent(Bellusraptor.gameObject, Bellusraptor.AlertRange);
            e.FireEvent();
        }

        if (Bellusraptor.IsIdling)
        {
            Bellusraptor.StopWalkSound();
            return;
        }
           

        if (!Bellusraptor.NavMeshAgent.hasPath)
        {
            if (!Bellusraptor.AlreadyIdled && Bellusraptor.BoolRandomizer())
            {
                Bellusraptor.StartCoroutine("PlayIdleAnim");
                return;
            }
            else
            {
                Bellusraptor.FindNewRoamingPosition();
                Bellusraptor.AlreadyIdled = false;
            }
            Bellusraptor.StandingStillTimer = 0;
        }

        if(Bellusraptor.NavMeshAgent.hasPath && Bellusraptor.StandingStillTimer >= 3f)
        {
            Bellusraptor.FindNewRoamingPosition();
            Bellusraptor.StandingStillTimer = 0;
        }

        Bellusraptor.MoveToPosition();
        
    }

    public override void Exit()
    {
        //safety ifall vi är i Idle-anim och den ser spelaren.
        Bellusraptor.IsIdling = false;
        Bellusraptor.Anim.SetBool("IsIdling", false);
        Bellusraptor.Anim.SetBool("IsWalking", false);
    }

}

