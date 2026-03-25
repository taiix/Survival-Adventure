using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Market UI controller – lists purchasable items and handles buy logic.
/// Addresses issues #35, #36, #37 (market UI, consumable items, purchase logic).
/// </summary>
public class MarketUI : MonoBehaviour
{
    [Header("Stock")]
    [SerializeField] private List<ShopItemData> stock = new List<ShopItemData>();

    [Header("UI References")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject shopItemButtonPrefab;
    [SerializeField] private TMP_Text goldDisplay;
    [SerializeField] private Button closeButton;

    private PlayerHealth playerHealth;

    private void OnEnable()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerHealth>();
        RefreshGoldDisplay();
        PopulateShop();

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged += OnGoldChanged;
    }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged -= OnGoldChanged;
    }

    private void OnGoldChanged(int newAmount)
    {
        RefreshGoldDisplay();
    }

    private void RefreshGoldDisplay()
    {
        if (goldDisplay != null && GoldManager.Instance != null)
            goldDisplay.text = $"Gold: {GoldManager.Instance.Gold}";
    }

    private void PopulateShop()
    {
        if (itemContainer == null || shopItemButtonPrefab == null) return;

        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        foreach (ShopItemData item in stock)
        {
            if (item == null) continue;
            GameObject btn = Instantiate(shopItemButtonPrefab, itemContainer);
            SetupShopButton(btn, item);
        }
    }

    private void SetupShopButton(GameObject btn, ShopItemData item)
    {
        Image icon = btn.transform.Find("Icon")?.GetComponent<Image>();
        if (icon != null && item.icon != null) icon.sprite = item.icon;

        TMP_Text nameLabel = btn.transform.Find("Name")?.GetComponent<TMP_Text>();
        if (nameLabel != null) nameLabel.text = item.itemName;

        TMP_Text costLabel = btn.transform.Find("Cost")?.GetComponent<TMP_Text>();
        if (costLabel != null) costLabel.text = $"{item.goldCost}g";

        TMP_Text descLabel = btn.transform.Find("Description")?.GetComponent<TMP_Text>();
        if (descLabel != null) descLabel.text = item.description;

        Button buyBtn = btn.GetComponent<Button>() ?? btn.GetComponentInChildren<Button>();
        if (buyBtn != null)
        {
            buyBtn.onClick.AddListener(() => TryPurchase(item));
        }
    }

    private void TryPurchase(ShopItemData item)
    {
        if (GoldManager.Instance == null || !GoldManager.Instance.TrySpend(item.goldCost))
        {
            AudioManager.Instance?.PlayUIClick();
            return;
        }

        AudioManager.Instance?.PlayPurchase();
        ApplyPurchaseEffect(item);
    }

    private void ApplyPurchaseEffect(ShopItemData item)
    {
        switch (item.itemType)
        {
            case ItemData.ItemType.Healing:
                playerHealth?.Heal(item.healAmount);
                break;
            case ItemData.ItemType.Gold:
                GoldManager.Instance?.AddGold(item.goldCost > 0 ? item.goldCost : 10);
                break;
        }
    }

    private void Close()
    {
        gameObject.SetActive(false);
        AudioManager.Instance?.PlayUIClick();
    }
}
