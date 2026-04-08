using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generic slot component for displaying and managing slot data.
/// Reusable for stores, teleporters, inventories, and other grid-based UIs.
/// </summary>
public class Slot : MonoBehaviour
{
    private Image sprite;
    
    private object slotData;
    private System.Action<object> onSlotSelected;

    private void OnEnable()
    {
        if (sprite == null)
        {
            sprite = GetComponent<Image>();
            
            if (sprite == null && transform.childCount > 0)
            {
                sprite = transform.GetChild(0).GetComponent<Image>();
            }
        }
    }

    /// <summary>Initialize the slot with data and optional callback.</summary>
    public void Initialize(object data, Sprite displaySprite, System.Action<object> onSelected = null)
    {
        slotData = data;
        onSlotSelected = onSelected;
        
        if (sprite != null && displaySprite != null)
        {
            sprite.sprite = displaySprite;
        }
    }

    /// <summary>Initialize slot with an ItemBase (legacy support).</summary>
    public void InitializeWithItem(ItemBase item, System.Action<object> onSelected = null)
    {
        if (item == null)
        {
            Debug.LogWarning("Slot: Item is null!");
            return;
        }

        Initialize(item, item.itemIcon, onSelected);
    }

    /// <summary>Get the slot's display position.</summary>
    public Transform GetSlotPosition()
    {
        if (sprite == null)
        {
            Debug.LogWarning($"Slot.GetSlotPosition: sprite is null on {gameObject.name}");
            return null;
        }
        return sprite.rectTransform;
    }

    /// <summary>Get the slot's data.</summary>
    public object GetSlotData()
    {
        return slotData;
    }

    /// <summary>Get the slot's data as a specific type.</summary>
    public T GetSlotData<T>() where T : class
    {
        return slotData as T;
    }

    /// <summary>Called when slot is selected by grid navigation.</summary>
    public void OnSelected()
    {
        onSlotSelected?.Invoke(slotData);
    }

    /// <summary>Update the sprite display.</summary>
    public void SetDisplaySprite(Sprite newSprite)
    {
        if (sprite != null && newSprite != null)
        {
            sprite.sprite = newSprite;
        }
    }

    /// <summary>Clear the slot.</summary>
    public void Clear()
    {
        slotData = null;
        onSlotSelected = null;
        
        if (sprite != null)
        {
            sprite.sprite = null;
        }
    }
}