using UnityEngine;
using UI;
using Unity.Collections;
using Healthy;


[CreateAssetMenu(fileName = "New gun", menuName = "Gun")]
public abstract class Gun : ScriptableObject
{

    [SerializeField] protected int maxAmmoClip;
    [SerializeField] protected int currentAmmoInClip;
    [SerializeField] protected int maxAmmoReserve;
    [SerializeField] protected int startAmmoReserve;
    [SerializeField] private int damagePerBullet;
    [SerializeField] protected float fireRate;
    [SerializeField] protected float reloadTime;
    [SerializeField] protected float spread;
    [SerializeField] protected float reloadMuzzleDelay;
    [SerializeField] protected Animator anim;

    [SerializeField] protected float alertRange = 7;

    [SerializeField] public int CurrentAmmoReserve { get; set; }

    public LayerMask CollisionMask;
    public GameObject prefab;
    public GameObject environmentBulletHole;
    public GameObject bloodSplat;

    protected CameraShake cameraShake;

    protected float nextTimeToFire;
    protected float nextTimeToReload;
    protected float delayTimerForFlash;

    //[SerializeField] private Transform weaponParent;
    [SerializeField] private GameObject muzzleFlashPrefab;
    private GameObject muzzlePosition;

    private GameObject spawnHitStuff;
    private CanvasHandler playerStats;


    public int CurrentAmmoInClip => currentAmmoInClip;
    public int MaxAmmoReserve => maxAmmoReserve;
    public int MaxAmmoInClip => maxAmmoClip;

    public Animator Anim { get => anim; set => anim = value; }
    public int DamagePerBullet { get => damagePerBullet; set => damagePerBullet = value; }
    public int StartAmmoReserve { get => startAmmoReserve; }

    public bool weaponIsEnabled;

    [Header("Sound")]
    [FMODUnity.EventRef]
    [SerializeField] protected string shootSound;
    [FMODUnity.EventRef]
    [SerializeField] protected string reloadSound;
    [FMODUnity.EventRef]
    [SerializeField] protected string hitSound;

    abstract public void Shoot();

    private void OnEnable()
    {
        currentAmmoInClip = maxAmmoClip;
        CurrentAmmoReserve = startAmmoReserve;
        nextTimeToFire = 0;
        nextTimeToReload = 1;
        weaponIsEnabled = false;
    }

    private void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("Canvas").GetComponent<CanvasHandler>();
    }

    public virtual void Reload()
    {
        if (CurrentAmmoReserve <= 0 || (currentAmmoInClip >= maxAmmoClip) || !ReloadingTimer() || !DelayForFlash())
        {
            return;
        }
        else
        {
            if (anim != null)
                anim.SetBool("IsReloading", true);


            SetNextTimeToReload();
            int ammoNeededFromReserve = maxAmmoClip - currentAmmoInClip;

            if (CurrentAmmoReserve - ammoNeededFromReserve < 0)
            {
                currentAmmoInClip += CurrentAmmoReserve;
                CurrentAmmoReserve = 0;
            }
            else
            {
                CurrentAmmoReserve -= ammoNeededFromReserve;
                currentAmmoInClip = maxAmmoClip;
            }

            FMODUnity.RuntimeManager.PlayOneShot(reloadSound);
        }
    }

    public void UpdateAmmoUI()
    {
        playerStats.UpdateVisualAmmo(CurrentAmmoInClip.ToString());
        playerStats.UpdateVisualAmmoReserve(CurrentAmmoReserve.ToString());
    }

    public void PlayMuzzleFlash()
    {
        GameObject go = GameObject.FindGameObjectWithTag("MuzzleFlash");
        Quaternion newRotation = go.transform.rotation;
        newRotation *= Quaternion.Euler(0, -90, 0);


        if (go != null)
        {
            //Debug.Log(go.name);
            Instantiate(muzzleFlashPrefab, go.transform.position, newRotation, go.transform);
        }
    }

    protected bool ShouldReload()
    {
        if (CurrentAmmoInClip == 0 && CurrentAmmoReserve > 0)
            return true;
        return false;
    }
    public void ResetAmmo()
    {
        currentAmmoInClip = MaxAmmoInClip;
        CurrentAmmoReserve = startAmmoReserve;
    }

    public void SetAmmo(int clipAmmo, int ReserveAmmo)
    {
        currentAmmoInClip = clipAmmo;
        CurrentAmmoReserve = ReserveAmmo;
    }
    protected GameObject SpawnCorrectObjectFromHit(LayerMask layer)
    {
        if (layer == LayerMask.NameToLayer("Enemy"))
        {
            return spawnHitStuff = bloodSplat;
        }
        else
        {
            return spawnHitStuff = environmentBulletHole;
        }
    }

    protected Vector3 ShootDirection()
    {
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        float z = Random.Range(-spread, spread);

        Vector3 shootDirection = Camera.main.transform.forward + new Vector3(x, y, z);

        return shootDirection;
    }

    protected bool ReadyToShoot()
    {
        if (Time.time >= nextTimeToFire)
        {
            if (anim != null)
                anim.SetBool("IsShooting", false);
            return true;
        }
        else
        {
            return false;
        }
    }

    // Gamla koden för att göra dmg. Keep for safety. Låg direkt i koden för varje vapen
    //Health targetHealth = hit.transform.GetComponent<Health>();
    //if (targetHealth != null)
    //{
    //    targetHealth.Damage(damagePerBullet);
    //    Debug.Log(targetHealth);

    //}

    protected void DealDamageToTarget(RaycastHit hit)
    {
        GameObject target = Health.FindParentWithTag(hit.transform.gameObject, "Enemy");
        if (target != null)
        {
            target.GetComponent<Health>().Damage(DamagePerBullet);
            return;
        }

        //vents är inte upplagda som Enemy, behöver detta för att dubbelkolla.
        Health targetHealth = hit.transform.GetComponent<Health>();
        if (targetHealth != null)
            targetHealth.Damage(DamagePerBullet);


    }

    //adds the interval-timer between fired shots
    protected float SetNextTimeToFire()
    {
        return nextTimeToFire = Time.time + fireRate;
    }

    //Timer for reload
    protected bool ReloadingTimer()
    {
        if (Time.time >= nextTimeToReload)
        {
            if (anim != null)
                anim.SetBool("IsReloading", false);

            return true;
        }
        else
        {
            return false;
        }
    }

    protected void SetReloadDelayTimerForFlash()
    {
        delayTimerForFlash = Time.time + reloadMuzzleDelay;
    }

    protected bool DelayForFlash()
    {
        if (Time.time >= delayTimerForFlash)
            return true;
        return false;
    }

    protected float SetNextTimeToReload()
    {
        return nextTimeToReload = Time.time + reloadTime;
    }

    // ANIMATIONS
    //public void LoadAnimationTimes()
    //{
    //    AnimationClip[] animClips = anim.runtimeAnimatorController.animationClips;
    //    foreach (AnimationClip animClip in animClips)
    //    {
    //        if (animClip.length != nextTimeToFire)
    //        {
    //            Debug.Log(animClip.name + animClip.length);
    //        }
    //    }
    //}

    //protected float CalculateAnimTime()
    //{
    //    return 0;
    //}

    //protected void PlayAnim(string animation, float multiplier)
    //{
    //    anim.SetFloat("AnimSpeed", multiplier);
    //    anim.SetBool(animation, true);
    //}
}
