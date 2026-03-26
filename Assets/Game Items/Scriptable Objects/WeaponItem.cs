using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItem", menuName = "Scriptable Objects/WeaponItem")]
public class WeaponItem : ItemBase
{
    [Space]
    [Header("Weapon Specific")]
    public int minDamage;
    public int maxDamage;

    public float attackSpeed;
}
