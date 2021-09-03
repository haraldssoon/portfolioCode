using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "States / Enemy / Bellusraptor / Chasing")]
public class BellusraptorChaseState : BellusraptorBaseState
{
    private float gameTimer;

    public override void Enter()
    {
        Bellusraptor.Anim.SetBool("IsRunning", true);

        FMODUnity.RuntimeManager.PlayOneShot(Bellusraptor.alertSound, Bellusraptor.transform.position);

        Bellusraptor.ChangeMovementSpeed(Bellusraptor.RunningSpeed);

        Bellusraptor.StartWalkSound();
    }

    public override void Run()
    {
        gameTimer += Time.deltaTime;

        if (Bellusraptor.IsDead)
        {
            Bellusraptor.StopMoving();
            return;
        }

        if (Bellusraptor.DistanceToPlayer < Bellusraptor.AttackRange && Bellusraptor.FieldOfView <= 0.8f)
        {
            Bellusraptor.stateMachine.TransitionTo<BellusraptorAttackState>();
        }

        if (gameTimer > 4f && !Bellusraptor.InLineOfSight())
        {
            Bellusraptor.stateMachine.TransitionTo<BellusraptorRoamingState>();
        }

        Bellusraptor.ChasePlayer();
       
    }

    public override void Exit()
    {
        gameTimer = 0f;

        Bellusraptor.Anim.SetBool("IsRunning", false);

        Bellusraptor.ChangeMovementSpeed(Bellusraptor.WalkingSpeed);

        Bellusraptor.StopWalkSound();
    }
}
