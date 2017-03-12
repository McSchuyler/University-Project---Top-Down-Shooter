using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour {

    public Transform weaponHold; //position to hold the weapon
    public Weapon[] allWeapon; //Array of all types of weapon
    Weapon equippedWeapon; // reference to weapon script

    void Start()
    {
       
    }

    //To equip weapon
    public void EquipWeapon(Weapon weaponToEquip)
    {
        if(equippedWeapon != null) //if player already equipped a weapon
        {
            Destroy(equippedWeapon.gameObject); //destory the weapon to prevent stacking the weapon
        }
        equippedWeapon = Instantiate (weaponToEquip, weaponHold.position, weaponHold.rotation) as Weapon; //create the weapon
        equippedWeapon.transform.parent = weaponHold; //set the weapon become a child of weaponHold and transform of the weaponHold
    }

    //Select weapon type
    public void EquipWeaponType(int weaponIndex)
    {
        EquipWeapon(allWeapon[weaponIndex]); //pass on type of weapon to equip
    }

    //pass script function from weapon script to player script
    public void OnTriggerHold()
    {
        if(equippedWeapon != null) //if player already equipped a weapon
        {
            equippedWeapon.OnTriggerHold(); //call function on weapon script called "OnTriggerHold"
        }
    }

    //pass script function from weapon script to player script
    public void OnTriggerRelease()
    {
        if (equippedWeapon != null) //if player already equipped a weapon
        {
            equippedWeapon.OnTriggerRelease(); //call function on weapon script called "OnTriggerRelease"
        }
    }

    //pass script function from weapon script to player script
    public float WeaponHeight
    {
        get
        {
            return weaponHold.position.y; //get weapon's height to get more accurate shooting
        }
    }

    //pass script function from weapon script to player script
    public void Aim(Vector3 aimPoint)
    {
        if(equippedWeapon != null) //if player already equipped a weapon
        {
            equippedWeapon.Aim(aimPoint); //call function on weapon script called "aim"
        }
    }

    //pass script function from weapon script to player script
    public void Reload()
    {
        if (equippedWeapon != null) //if player already equipped a weapon
        {
            equippedWeapon.Reload(); //call function on weapon script called "Reload" 
        }
    }
}
