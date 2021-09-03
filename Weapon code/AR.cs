using Healthy;
using UnityEngine;
using TargetDummyHelper;
using Events;
using Healthy;

[CreateAssetMenu(menuName = "Weapon / AR")]
public class AR : Gun
{
    public override void Shoot()
    {
        if (ShouldReload())
        {
            Reload();
            return;
        }
        if (ReloadingTimer() && currentAmmoInClip > 0)
            if (ReadyToShoot() && PlayerController.Instance.Inputs.Player.Fire.ReadValue<float>() > 0.1f/*&& Input.GetKey(KeyCode.Mouse0)*/)
            {
                if (Physics.Raycast(Camera.main.transform.position, ShootDirection(), out RaycastHit hit, Mathf.Infinity, CollisionMask, QueryTriggerInteraction.Ignore))
                {
                    //Debug.Log("hit collision mask");
                    //Debug.Log(hit.transform.gameObject.name);
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

                    GameObject bulletHit = Instantiate(SpawnCorrectObjectFromHit(hit.transform.gameObject.layer), hit.point, Quaternion.LookRotation(hit.normal));
                    bulletHit.transform.parent = hit.transform;
                    Destroy(bulletHit, 5f);

                    DealDamageToTarget(hit);

                    //If Target is Target-Dummy
                    TargetDummy targetDummy = hit.transform.GetComponent<TargetDummy>();
                    if (targetDummy != null)
                    {
                        targetDummy.HitByBullet();
                    }
                }

                FMODUnity.RuntimeManager.PlayOneShot(shootSound);
                AlertEnemyEvent e = new AlertEnemyEvent(PlayerController.Instance.transform.gameObject, alertRange);
                e.FireEvent();

                currentAmmoInClip--;
                SetNextTimeToFire();
                SetReloadDelayTimerForFlash();
                //PlayerController.Instance.GetComponent<Health>().AlertEnemiesInRange(PlayerController.Instance.transform, alertRange);
            }

        if (currentAmmoInClip <= 0 && CurrentAmmoReserve <= 0)
        {
            anim.SetBool("IsShooting", false);
            NoAmmoInGunEvent e = new NoAmmoInGunEvent(this);
            e.FireEvent();
        }
    }

}




