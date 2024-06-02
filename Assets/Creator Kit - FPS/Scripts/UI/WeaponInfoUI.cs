using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInfoUI : MonoBehaviour
{
    public static WeaponInfoUI Instance { get; private set; }

    public Text WeaponName;
    public Text WeaponClipContent;
    public Text AmmoTypeCount;
    
    void OnEnable()
    {
        Instance = this;
    }

    public void UpdateWeaponName(Weapon weapon)
    {
        WeaponName.text = weapon.name;
    }

   
    public void UpdateAmmoAmount(int amount)
    {
        AmmoTypeCount.text = amount.ToString();
    }
}
