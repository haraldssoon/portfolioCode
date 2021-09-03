using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Healthy;

public class BellusraptorController : EnemyController
{
    public void Attack()
    {
        //Dot-vectors
        Vector3 enemyForward = transform.TransformDirection(Vector3.forward);
        Vector3 differenceEnemyToPlayer = Player.position - transform.position;

        Anim.SetBool("IsBiting", true);
        PlayBiteSound();

        if (PlayerIsInDamageCone(enemyForward.normalized, differenceEnemyToPlayer.normalized))
        {
            Health playerHealth = Player.GetComponent<Health>();
            playerHealth.Damage(AttackDamage);

            if (!DI_System.CheckIfObjectInSight(transform))
            {
                DI_System.CreateIndicator(transform);
            }
        }

        //resetar attackTimern, tar nu timeBetweenAttacks sekunder innan den kan slå igen.
        AttackTimeCounter = GameTimer;
    }

    public bool PlayerIsInDamageCone(Vector3 enemyDirection, Vector3 playerPosition)
    {
        //Kollar om spelaren är inom konen för att kunna göra skada
        if (Vector3.Dot(enemyDirection, playerPosition) >= FieldOfView && Vector3.Distance(transform.position, Player.position) <= AttackRange)
            return true;
        return false;
    }


}
