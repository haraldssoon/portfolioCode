using JetBrains.Annotations;
using UnityEngine;
using Events;
using PlayerDataInfo;
using System.Collections.Generic;

namespace WeaponHelper{
    public class Weapon : MonoBehaviour
    {
        private static Weapon instance;

        [SerializeField] private Gun[] loadout;
        private List<int> loadoutList = new List<int>();

        private Transform weaponParent;
        private GameObject currentWeapon;
        private Gun currentGun;
        private int equipedWeapon;
        private GameObject grenadeObject;


        public Gun CurrentGun => currentGun;
        public GameObject CurrentWeapon { get; set; }
        public int EquipedWeapon { get => equipedWeapon;}
        public Gun[] Loadout { get => loadout; set => loadout = value; }
        public Transform WeaponParent { get => weaponParent; }

        [Header("Sound")]
        [FMODUnity.EventRef]
        [SerializeField] private string equipWeaponSound;
        [FMODUnity.EventRef]
        [SerializeField] private string pickupWeaponSound;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            instance = this;
            MeleeHitEvent.RegisterListener(HitEnemy);
            ThrowGrenadeEvent.RegisterListener(ThrowGrenade);
        }

        // Start is called before the first frame update
        void Start()
        {
            if(weaponParent == null)
            {
                //Debug.Log("no weaponparent");
                weaponParent = GameObject.FindGameObjectWithTag("PlayerWeapon").GetComponent<Transform>();
            }

            //EquipWeapon(0);
            PickupEvent.RegisterListener(ObtainAmmo);
            WeaponPickupEvent.RegisterListener(ObtainWeapon);
            WeaponUnEquipEvent.RegisterListener(UnEquipWeapon);
            PlayerController.Instance.Inputs.Player.Reload.started += ctx => ReloadCurrentGun();
            PlayerController.Instance.Inputs.Player.WeaponWheelUp.performed += ctx => EquipWeapon(0);
            PlayerController.Instance.Inputs.Player.WeaponwheelLeft.performed += ctx => EquipWeapon(1);
            PlayerController.Instance.Inputs.Player.WeaponWheelDown.performed += ctx => EquipWeapon(2);
            PlayerController.Instance.Inputs.Player.WeaponWheelRight.performed += ctx => EquipWeapon(3);
            PlayerController.Instance.Inputs.Player.WeaponWheelUpUp.performed += ctx => EquipWeapon(4);
            PlayerController.Instance.Inputs.Player.WeaponWheelLeftLeft.performed += ctx => EquipWeapon(5);
            PlayerController.Instance.Inputs.Player.WeaponWheelDownDown.performed += ctx => EquipWeapon(6);
            //PlayerController.Instance.Inputs.Player.WeaponWheelRightRight.performed += ctx => EquipWeapon(7);

        }

        private void OnDestroy()
        {
            PickupEvent.UnRegisterListener(ObtainAmmo);
            WeaponPickupEvent.UnRegisterListener(ObtainWeapon);
            ThrowGrenadeEvent.UnRegisterListener(ThrowGrenade);
            MeleeHitEvent.UnRegisterListener(HitEnemy);
            WeaponUnEquipEvent.UnRegisterListener(UnEquipWeapon);

        }

        // Update is called once per frame
        void Update()
        {
            CheckForPlayerInput();
            if(Camera.main != null)
            {
                //Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 100, Color.red);
            }
            //Debug.Log(currentGun.Anim.name);
        }
        public static Weapon Instance
        {
            get
            {
                if(instance == null)
                {
                    //Debug.Log("findingweapon");
                    instance = (Weapon)FindObjectOfType(typeof(Weapon));
                }
                return instance;

            }
        }

        public void ObtainAmmo(PickupEvent e)
        {
            //This if statement checks if the ammo recieved is greater than MaxAmmoReserve. If true, we set ammo to MaxAmmoReserve.
            if (loadout[e.weaponIndex].CurrentAmmoReserve + e.ammo > loadout[e.weaponIndex].MaxAmmoReserve)
            {
                loadout[e.weaponIndex].CurrentAmmoReserve = loadout[e.weaponIndex].MaxAmmoReserve;
                return;
            }

            loadout[e.weaponIndex].CurrentAmmoReserve += e.ammo;
        }

        private void ObtainWeapon(WeaponPickupEvent e)
        {
            FMODUnity.RuntimeManager.PlayOneShot(pickupWeaponSound);
            loadout[e.weaponIndex].weaponIsEnabled = true;
            if(CurrentGun == null)
            {
                EquipWeapon(e.weaponIndex);
                UI.CanvasHandler.Instance.EnableUIElements();
            }

            //If you have selected in options menu -> equp weapon on pickup.
            if (PlayerData.instance.EquipWeaponOnPickup)
            {
                EquipWeapon(e.weaponIndex);
                UI.CanvasHandler.Instance.EnableUIElements();
            }
        }

        public void UnEquipWeapon(WeaponUnEquipEvent e)
        {
            loadout[e.weaponIndex].weaponIsEnabled = false;
            currentGun = null;
            Destroy(currentWeapon);
        }


        public void UnEquipAllWeapons()
        {
            for (int i = 0; i < loadout.Length; i++)
            {
                //loadoutList.Add(i);
                loadout[i].weaponIsEnabled = false;
                //WeaponUnEquipEvent e = new WeaponUnEquipEvent(weapon.weaponIndex);
                //e.FireEvent();
            }

            //foreach (int weaponInt in loadoutList)
            //{
            //    WeaponUnEquipEvent e = new WeaponUnEquipEvent(weaponInt);
            //    e.FireEvent();
            //}
        }

        private void CheckForPlayerInput()
        {

            //Pistol
            if (Input.GetKeyDown(KeyCode.Alpha1) && loadout[0].weaponIsEnabled)
            {
                //FMODUnity.RuntimeManager.PlayOneShot(equipWeaponSound);
                EquipWeapon(0);
            }
            //Shotgun
            if (Input.GetKeyDown(KeyCode.Alpha2) && loadout[1].weaponIsEnabled)
            {
                //FMODUnity.RuntimeManager.PlayOneShot(equipWeaponSound);
                EquipWeapon(1);
            }
            //AR
            if (Input.GetKeyDown(KeyCode.Alpha3) && loadout[2].weaponIsEnabled)
            {
                //FMODUnity.RuntimeManager.PlayOneShot(equipWeaponSound);
                EquipWeapon(2);
            }
            //RPG
            if (Input.GetKeyDown(KeyCode.Alpha4) && loadout[3].weaponIsEnabled)
            {
                //FMODUnity.RuntimeManager.PlayOneShot(equipWeaponSound);
                EquipWeapon(3);
            }
            //Grenade
            if (Input.GetKeyDown(KeyCode.Alpha5) && loadout[4].weaponIsEnabled)
            {
                //FMODUnity.RuntimeManager.PlayOneShot(equipWeaponSound);
                EquipWeapon(4);
            }
            //Knife
            if (Input.GetKeyDown(KeyCode.Alpha6) && loadout[5].weaponIsEnabled)
            {
                //FMODUnity.RuntimeManager.PlayOneShot(equipWeaponSound);
                EquipWeapon(5);
            }
            //RailGUn
            if (Input.GetKeyDown(KeyCode.Alpha7) && loadout[6].weaponIsEnabled)
            {
                //FMODUnity.RuntimeManager.PlayOneShot(equipWeaponSound);
                EquipWeapon(6);
            }

            ScrollWheelWeaponSwitch();

            //Letar efter Mouse0, GetKey/GetKeyDown beroende på vapen
            if(CurrentGun != null)
            {
                currentGun.Shoot();
                
            }

            // FOR TESTING - REMOVE LATER
            //if (Input.GetKeyDown(KeyCode.L))
            //{
            //    for (int i = 0; i < loadout.Length; i++)
            //    {
            //        loadout[i].weaponIsEnabled = true;

            //        WeaponPickupEvent e = new WeaponPickupEvent(i);
            //        e.FireEvent();

            //        PickupEvent pe = new PickupEvent(100, i);
            //        pe.FireEvent();
            //    }
            //}

            //if (Input.GetKeyDown(KeyCode.K))
            //{
            //    for (int i = 0; i < loadout.Length; i++)
            //    {
            //        loadout[i].weaponIsEnabled = false;
            //    }
            //}

        }
        private void ReloadCurrentGun()
        {
            if(currentGun)
                currentGun.Reload();
        }
        public void HideCurrentWeapon()
        {
            Destroy(currentWeapon);
        }
        public void EquipWeapon(int weaponIndex)
        {
            
            if (!loadout[weaponIndex].weaponIsEnabled)
                return;
            if (currentWeapon != null)
            {
                if (currentWeapon.name == loadout[weaponIndex].name + "(Clone)")
                {
                    return;
                }
                else
                {
                    Destroy(currentWeapon);
                }

            }
            if (weaponParent == null)
            {
                //Debug.Log("No weaponparent");
                weaponParent = GameObject.FindGameObjectWithTag("PlayerWeapon").GetComponent<Transform>();
            }

            GameObject newWeapon = Instantiate(loadout[weaponIndex].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
            currentWeapon = newWeapon;
            currentGun = loadout[weaponIndex];
            equipedWeapon = weaponIndex;
            FMODUnity.RuntimeManager.PlayOneShot(equipWeaponSound);
            //sätter animatorer
            currentGun.Anim = newWeapon.GetComponentInChildren<Animator>();
            PlayerController.Instance.Anim = currentGun.Anim;

            WeaponUIEvent e = new WeaponUIEvent(weaponIndex);
            e.FireEvent();
            if (CurrentGun.GetType() == typeof(Grenade) && CurrentGun.CurrentAmmoInClip == 0 && CurrentGun.CurrentAmmoReserve == 0)
            {
                HideCurrentWeapon();
            }
        }

        public int GetCurrentWeaponIndex()
        {
            for (int i = 0; i < loadout.Length; i++)
            {

                if (loadout[i] == currentGun)
                {
                    return i;
                }
            }
            return -1;
        }

        private void ScrollWheelWeaponSwitch()
        {
            if (CurrentGun == null)
            {
                return;
            }


            int scrollWheelPosition = 0;
            int weaponIndex = 0;

            if (Input.mouseScrollDelta.y != scrollWheelPosition)
            {
                for (int i = 0; i < loadout.Length; i++)
                {
                    
                    if (loadout[i] == currentGun)
                    {
                        weaponIndex = i;
                    }
                }

                scrollWheelPosition = (int)Input.mouseScrollDelta.y;
                weaponIndex = NextGunAvailable(weaponIndex, scrollWheelPosition);
                EquipWeapon(weaponIndex);
            }
        }


        private int NextGunAvailable(int weaponIndex, int upOrDown)
        {
            int scrollInput = upOrDown > 0 ? 1 : -1;
            for (int i = weaponIndex + scrollInput; i != weaponIndex; i += scrollInput)
            {
                //Debug.Log("for");
                if (i > loadout.Length - 1)
                {
                    i = 0;
                }
                else if (i < 0)
                {
                    i = loadout.Length - 1;
                }

                if (loadout[i].weaponIsEnabled)
                {
                    return i;
                }
            }


            //Debug.Log("jag ska itne vara här");
            return weaponIndex;
        }
        private void HitEnemy(MeleeHitEvent e)
        {
            if(currentGun.GetType() == typeof(Knife))
            {
                Knife k = (Knife)currentGun;
                k.HitEnemy();
            }
        }
        private void ThrowGrenade(ThrowGrenadeEvent e)
        {
            //Debug.Log("throwing grenade");
            if (currentGun.GetType() == typeof(Grenade))
            {
                Grenade g = (Grenade)currentGun;
                g.ThrowGrenade();
            }
        }
    }
}

