using UnityEngine;
using Healthy;
using Events;

[CreateAssetMenu(menuName = "Weapon / RPG")]
public class RPG : Gun
{
    
    public GameObject rocketSpawnPosition;

    public override void Shoot()
    {
        if (ShouldReload())
        {
            Reload();
            return;
        }
        //1 skott bara? 
        if (ReloadingTimer() && ReadyToShoot() && currentAmmoInClip > 0 && PlayerController.Instance.Inputs.Player.Fire.triggered)
        //if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Physics.Raycast(Camera.main.transform.position, ShootDirection(), out RaycastHit hit, Mathf.Infinity, CollisionMask, QueryTriggerInteraction.Ignore))
            {
                Debug.Log("hit collision mask");
                Debug.Log(hit.transform.gameObject.name);
                if (hit.transform.gameObject.layer == 20)
                {
                    Debug.Log("NPC layer raycast");
                    anim.gameObject.GetComponent<Animator>().enabled = false;
                    anim.SetBool("IsShooting", false);
                    SetNextTimeToFire();
                    return;
                }

                PlayMuzzleFlash();

                anim.gameObject.GetComponent<Animator>().enabled = true;
                anim.SetBool("IsShooting", true);

                rocketSpawnPosition = GameObject.FindWithTag("RocketSpawnPosition");
                // Måste fixa spawnpoint för Rocket, satt nu bara för att testa att den funkar
                GameObject rocketProjectile = Instantiate(environmentBulletHole, rocketSpawnPosition.transform.position, Camera.main.transform.rotation);
                Destroy(rocketProjectile, 5f);

                FMODUnity.RuntimeManager.PlayOneShot(shootSound);
                AlertEnemyEvent e = new AlertEnemyEvent(PlayerController.Instance.transform.gameObject, alertRange);
                e.FireEvent();

                currentAmmoInClip--;
                Debug.Log(currentAmmoInClip + " ammo in clip");
                SetNextTimeToFire();
                //PlayerController.Instance.GetComponent<Health>().AlertEnemiesInRange(PlayerController.Instance.transform, alertRange);
            }
        }
    }
}




