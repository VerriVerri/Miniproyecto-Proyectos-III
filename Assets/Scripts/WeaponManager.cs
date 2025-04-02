using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;


public enum WeaponType
{
    None,
    Pistol,
    Shotgun,
    RPG
}

public class WeaponManager : MonoBehaviour
{
    public Guns guns;
    public WeaponType primaryWeapon = WeaponType.None;
    public WeaponType secondaryWeapon = WeaponType.None;

    public TextMeshProUGUI ammoLeft;
    public TextMeshProUGUI ammoTotal;
    [Header("Pistol ammo")]
    public uint pistolAmmo;
    public uint pistolAmmoLeft;
    public Image pistolIcon;
    public Image pistolIcon2;
    [Header("Shotgun ammo")]
    public uint shotgunAmmo;
    public uint shotgunAmmoLeft;
    public Image shotgunIcon;
    public Image shotgunIcon2;
    [Header("RPG ammo")]
    public uint missileAmmo = 3;
    public uint missileAmmoLeft;
    public Image RPGIcon;
    public Image RPGIcon2;

    private Coroutine swapCooldownCoroutine;

    private void Start()
    {
        OnNewWeapon(WeaponType.Pistol);
        OnNewWeapon(WeaponType.Shotgun);
        OnNewWeapon(WeaponType.RPG);
    }
    private void Update()
    {
        UpdateUI();
        if (Input.GetKeyDown(KeyCode.G))
            {
            SwapWeapon();
            }


    }
    public void PickupWeapon(WeaponType newWeapon)
    {
        if (primaryWeapon == WeaponType.None || primaryWeapon == newWeapon)
        {
            primaryWeapon = newWeapon;
            OnNewWeapon(newWeapon);
            guns.shootMode = (int)newWeapon;
        }
        else
        {
            secondaryWeapon = newWeapon;
            OnNewWeapon(newWeapon);
        }
    }
    public void SwapWeapon()
    {
        if (primaryWeapon != WeaponType.None && secondaryWeapon != WeaponType.None)
        {
            WeaponType pivotWeapon = primaryWeapon;
            primaryWeapon = secondaryWeapon;
            secondaryWeapon = pivotWeapon;
            OnWeaponChanged();

        }
    }
    public void destroyWeapon()
    {
        primaryWeapon = WeaponType.None;
    }

    public void OnNewWeapon(WeaponType newWeapon)
    {
        switch (newWeapon)
        {
            case WeaponType.Pistol:
                pistolAmmoLeft = pistolAmmo;
                
                break;
            case WeaponType.Shotgun:
                shotgunAmmoLeft = shotgunAmmo;
                
                break;
            case WeaponType.RPG:
                missileAmmoLeft = missileAmmo;
                
                break;
        }
    }

    public void OnShootWeapon(WeaponType weapon) 
    {
        switch (weapon)
        {
            case WeaponType.Pistol:
                pistolAmmoLeft -= 1;
                
                break;
            case WeaponType.Shotgun:
                shotgunAmmoLeft -= 1;
                
                break;
            case WeaponType.RPG:
                missileAmmoLeft -= 1;
               
                break;
        }
    }
    public void OnWeaponChanged()
    {
        switch (primaryWeapon)
        {
            case WeaponType.Pistol:
                StartSwapCoolDown(0.19f, 0);
                guns.shootMode = 1;
                
                break;
            case WeaponType.Shotgun:
                StartSwapCoolDown(0.31f, 0);
                guns.shootMode = 2;
                
                break;
            case WeaponType.RPG:
                StartSwapCoolDown(0.71f, 0);
                guns.shootMode = 3;
                
                break;
        }
    }
    public void StartSwapCoolDown(float i, int j)
    {
        // Stop the previous coroutine if it's still running
        if (swapCooldownCoroutine != null)
        {
            StopCoroutine(swapCooldownCoroutine);
        }

        // Start a new coroutine and store its reference
        swapCooldownCoroutine = StartCoroutine(SwapCoolDown(i, j));
    }
    private IEnumerator SwapCoolDown(float i, int j)
    {
        guns.canShoot = false;
        yield return new WaitForSeconds(i);
        guns.canShoot = true;
    }
    public void UpdateUI()
    {
        switch (guns.shootMode)
        {
            case 1:
                ammoTotal.text = pistolAmmo.ToString();
                ammoLeft.text = pistolAmmoLeft.ToString();
                pistolIcon.enabled = true; shotgunIcon.enabled = false; RPGIcon.enabled = false;
                break;
            case 2:
                ammoTotal.text = shotgunAmmo.ToString();
                ammoLeft.text = shotgunAmmoLeft.ToString();
                pistolIcon.enabled = false; shotgunIcon.enabled = true; RPGIcon.enabled = false;
                break;
            case 3:
                ammoTotal.text = missileAmmo.ToString();
                ammoLeft.text = missileAmmoLeft.ToString();
                pistolIcon.enabled = false; shotgunIcon.enabled = false; RPGIcon.enabled = true;
                break;
        }
        switch (secondaryWeapon)
        {
            case WeaponType.None:
                pistolIcon2.enabled = false; shotgunIcon2.enabled = false; RPGIcon2.enabled = false;
                return;
            case WeaponType.Pistol:
                pistolIcon2.enabled = true; shotgunIcon2.enabled = false; RPGIcon2.enabled = false;
                return;
            case WeaponType.Shotgun:
                pistolIcon2.enabled = false; shotgunIcon2.enabled = true; RPGIcon2.enabled = false;
                return;
            case WeaponType.RPG:
                pistolIcon2.enabled = false; shotgunIcon2.enabled = false; RPGIcon2.enabled = true;
                return;

        }

    }
}