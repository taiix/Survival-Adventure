using UnityEngine;

[CreateAssetMenu(fileName = "ConsumablesItem", menuName = "Scriptable Objects/ConsumablesItem")]
public class ConsumablesItem : ScriptableObject
{
    [Space]
    [Header("Consumables Specific")]
    public int healthRestore;
}
