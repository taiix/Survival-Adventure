using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private ItemBase item;
    [SerializeField] private Image sprite;

    private void OnEnable()
    {
        if (sprite == null) sprite = transform.GetChild(0).GetComponent<Image>();
        if (item != null) sprite.sprite = item.itemIcon;
    }

    private void Upgrade() { 
        UpgradeEvents.OnUpgradePurchased?.Invoke();
    }

    public Transform GetSlotPosition()
    {
        if (sprite == null) return null;
        return sprite.rectTransform;
    }

    public ItemBase GetItem()
    {
        return item;
    }
}
