using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Blacksmith NPC behavior - handles weapon and armor upgrades.
/// Manages shop UI lifecycle and delegates to BlacksmithShopBehaviour.
/// </summary>
public class BlacksmithBehavior : MonoBehaviour, INPCBehaviour
{
    [SerializeField] private GameObject blacksmithUIPanel;
    [SerializeField] private BlacksmithShopBehaviour shopBehaviour;

    public void OnInteract()
    {
        if (shopBehaviour != null)
        {
            shopBehaviour.enabled = true;
        }
    }

    public void OnInteractionEnd()
    {
        if (shopBehaviour != null)
        {
            shopBehaviour.enabled = false;
        }
    }

    public void OnInteractionUpdate()
    {
        // Called every frame while interacting
    }

    public GameObject GetUIPanel() => blacksmithUIPanel;
}