using System.Collections.Generic;
using UnityEngine;

public class BlacksmithShopBehaviour : MonoBehaviour
{
    [SerializeField] private List<ItemSlot> itemSlots;
    private void Awake()
    {
        GetAllItemSlots(this.gameObject.transform);
    }
    private void OnEnable()
    {
        //When the shop is opened
        itemSlots = new();
        GetAllItemSlots(this.gameObject.transform);
    }

    private void OnDisable()
    {
        //when the shop is closed
        itemSlots.Clear();
    }


    private void GetAllItemSlots(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Debug.Log(i);
            parent = parent.GetChild(i);

            foreach (Transform child in parent)
            {
                if (child.TryGetComponent(out ItemSlot slot))
                {
                    itemSlots.Add(slot);
                }
            }
        }
    }
}
