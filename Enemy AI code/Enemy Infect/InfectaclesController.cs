using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Healthy;
using UnityEngine.AI;

public class InfectaclesController : EnemyController
{
    [SerializeField] private float aimSpread;
    [SerializeField] [Range(0f, 3f)] private float reloadTime;
    [SerializeField] private int maxAmmoInClip;
    [SerializeField] private int sideSteps;

    private bool reloading;

    private float reloadTimeCounter;

    private int ammoInClip;

    private Vector3 delayedPlayerPosition;

    public bool Reloading => reloading;
    public float ReloadTime => reloadTime;

    public int AmmoInClip { get => ammoInClip; set => ammoInClip = value; }
    public float ReloadTimeCounter { get => reloadTimeCounter; set => reloadTimeCounter = value; }

    public Vector3 DelayedPlayerPosition { get => delayedPlayerPosition; set => delayedPlayerPosition = value; }


    private void Awake()
    {
        ammoInClip = maxAmmoInClip;
        reloading = false;
    }

    public void Shoot()
    {
        var shootDirection = ((delayedPlayerPosition - transform.position).normalized + new Vector3(Random.Range(-aimSpread, aimSpread), Random.Range(-aimSpread, aimSpread), Random.Range(-aimSpread, aimSpread)));

        //draws shot
        Debug.DrawRay(transform.position, shootDirection * AttackRange, Color.red, TimeBetweenAttacks);

        //gör om till line-cast
        if (Physics.Raycast(transform.position, shootDirection, out RaycastHit hit, AttackRange, SightMask, QueryTriggerInteraction.Ignore))
        {
            Health targetHealth = hit.transform.GetComponent<Health>();
            if (targetHealth != null)
                targetHealth.Damage(AttackDamage);
        }
        ammoInClip--;
        AttackTimeCounter = GameTimer;
    }

    public void Reload()
    {
        if (GameTimer >= (ReloadTimeCounter + ReloadTime) && reloading)
        {
            ammoInClip = maxAmmoInClip;
            reloading = false;
            return;
        } 
        else if (!reloading)
        {
            ReloadTimeCounter = GameTimer;
            reloading = true;
        }

    }

    public bool SideStepToLOS()
    {
        int randomizer = Random.Range(0,1);

        if(randomizer == 0)
        {
            if (AttemptRightSideStep())
                return true;

            if (AttemptLeftSideStep())
                return true;
        }
        else
        {
            if (AttemptLeftSideStep())
                return true;

            if (AttemptRightSideStep())
                return true;
        }
        return false;
    }


    private bool AttemptRightSideStep()
    {
        for (int i = 0; i < sideSteps; i++)
        {
            Physics.Linecast(transform.position + (transform.right * i), Player.transform.position, out RaycastHit hit, SightMask);
            if (hit.collider != null && hit.transform.gameObject.layer == Player.gameObject.layer)
            {
                int extraStepsAroundCorner = 2;
                ChangeTargetDestination(transform.position + (transform.right * (i + extraStepsAroundCorner)));
                return true;
            }
        }
        return false;
    }

    private bool AttemptLeftSideStep()
    {
        for (int i = 0; i > -sideSteps; i--)
        {
            Physics.Linecast(transform.position + (transform.right * i), Player.transform.position, out RaycastHit hit, SightMask);
            if (hit.collider != null && hit.transform.gameObject.layer == Player.gameObject.layer)
            {
                int extraStepsAroundCorner = 2;
                ChangeTargetDestination(transform.position + (transform.right * (i - extraStepsAroundCorner)));
                return true;
            }
        }
        return false;
    }

}
