using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Healthy;

[CreateAssetMenu(menuName = "States / Enemy / Infectacles / Attack")]
public class InfectaclesAttackState : InfectaclesBaseState
{
    public override void Enter()
    {
        Infectacles.AttackTimeCounter = Infectacles.GameTimer;
    }

    public override void Run()
    {
        //kollar ifall spelaren är för långt borta och borde springa efter
        if(Infectacles.DistanceToPlayer > Infectacles.VisionRange)
        {
            Infectacles.stateMachine.TransitionTo<InfectaclesChaseState>();
        }

        //kollar om den behöver ladda om, isåfall gör det.
        if(Infectacles.AmmoInClip <= 0)
        {
            Infectacles.Reload();
            return;
        }

        //kollar ifall den kan skjuta igen, isåfall gör det.
        if(Infectacles.GameTimer >= (Infectacles.AttackTimeCounter + Infectacles.TimeBetweenAttacks))
        {
            Infectacles.Shoot();
        }
        else if(InBetweenShots())
        {
            //Sätter AIn's sikte till positions som spelaren var på mellan mellan förra skotten och nästa skott. 
            //Ger advantage till spelaren att röra på sig
            Infectacles.DelayedPlayerPosition = Infectacles.Player.position;
        }

        //om AIn ser spelaren, stå på preferredDistance. Annars kolla om den kan side-step. Annars gå närmare spelaren.
        if (!Infectacles.InLineOfSight())
        {
            if (!Infectacles.SideStepToLOS())
            {
                //för att gå närmare ifall man är utanför LOS
                int tempStandardDistance = 2;
                Infectacles.ChangeTargetDestination(Infectacles.Player.transform.position - (Infectacles.DirectionToTarget * tempStandardDistance));
            }

        }
        else 
        {
            Infectacles.ChangeTargetDestination(Infectacles.StandardDistance);
        }

        Infectacles.MoveToPosition();
        Infectacles.LookAtPlayer();
    }

    private bool InBetweenShots()
    {   
        //ful kod, hur gör man det bättre?
        if (Infectacles.GameTimer >= (Infectacles.AttackTimeCounter + (Infectacles.TimeBetweenAttacks / 2)) - 0.1f &&
            Infectacles.GameTimer >= (Infectacles.AttackTimeCounter + (Infectacles.TimeBetweenAttacks / 2)) + 0.1f)
            return true;
        else 
            return false;
    }
}
