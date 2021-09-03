using UnityEngine;
using Healthy;
using Events;

[CreateAssetMenu(menuName = "Weapon / Shotgun")]
public class Shotgun : Gun
{
    public override void Shoot()
    {
        if (ShouldReload())
        {
            Reload();
            return;
        }
        if (ReloadingTimer() && currentAmmoInClip > 0)
            if (ReadyToShoot() && PlayerController.Instance.Inputs.Player.Fire.triggered)
            {
                for (int i = 0; i < 8; i++)
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

                        GameObject bulletHit = Instantiate(SpawnCorrectObjectFromHit(hit.transform.gameObject.layer), hit.point, Quaternion.LookRotation(hit.normal));
                        bulletHit.transform.parent = hit.transform;
                        Destroy(bulletHit, 5f);

                        DealDamageToTarget(hit);
                    } 
                }

                PlayMuzzleFlash();

                anim.gameObject.GetComponent<Animator>().enabled = true;
                anim.SetBool("IsShooting", true);

                FMODUnity.RuntimeManager.PlayOneShot(shootSound);
                AlertEnemyEvent e = new AlertEnemyEvent(PlayerController.Instance.transform.gameObject, alertRange);
                e.FireEvent();

                currentAmmoInClip--;
                SetNextTimeToFire();
                SetReloadDelayTimerForFlash();
                //PlayerController.Instance.GetComponent<Health>().AlertEnemiesInRange(PlayerController.Instance.transform, alertRange);
            }
    }

}


