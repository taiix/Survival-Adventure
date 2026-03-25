using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for the Blacksmith upgrade shop.
/// Populates upgrade buttons dynamically from an UpgradeData list.
/// Addresses issues #14, #34, #55 (upgrade icons, weapon upgrade logic).
/// </summary>
public class BlacksmithUI : MonoBehaviour
{
    [Header("Upgrade List")]
    [SerializeField] private List<UpgradeData> availableUpgrades = new List<UpgradeData>();

    [Header("UI References")]
    [SerializeField] private Transform upgradeContainer;
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private Button closeButton;

    private void OnEnable()
    {
        RefreshUI();
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    public void RefreshUI()
    {
        if (upgradeContainer == null || upgradeButtonPrefab == null) return;

        // Clear existing buttons
        foreach (Transform child in upgradeContainer)
            Destroy(child.gameObject);

        foreach (UpgradeData data in availableUpgrades)
        {
            if (data == null) continue;
            GameObject btn = Instantiate(upgradeButtonPrefab, upgradeContainer);
            SetupUpgradeButton(btn, data);
        }
    }

    private void SetupUpgradeButton(GameObject btn, UpgradeData data)
    {
        // Set icon if there's an Image component named "Icon"
        Image iconImage = btn.transform.Find("Icon")?.GetComponent<Image>();
        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;

        // Set name label
        TMP_Text nameLabel = btn.transform.Find("Name")?.GetComponent<TMP_Text>();
        if (nameLabel != null)
            nameLabel.text = data.upgradeName;

        // Set cost label
        TMP_Text costLabel = btn.transform.Find("Cost")?.GetComponent<TMP_Text>();
        if (costLabel != null)
            costLabel.text = $"{data.goldCost}g";

        // Set description label
        TMP_Text descLabel = btn.transform.Find("Description")?.GetComponent<TMP_Text>();
        if (descLabel != null)
            descLabel.text = data.description;

        // Level label
        TMP_Text levelLabel = btn.transform.Find("Level")?.GetComponent<TMP_Text>();
        if (levelLabel != null)
        {
            int lvl = UpgradeManager.Instance != null ? UpgradeManager.Instance.GetLevel(data) : 0;
            levelLabel.text = $"Lv {lvl}/{data.maxLevel}";
        }

        // Buy button
        Button buyBtn = btn.GetComponent<Button>() ?? btn.GetComponentInChildren<Button>();
        if (buyBtn != null)
        {
            buyBtn.onClick.AddListener(() =>
            {
                if (UpgradeManager.Instance != null && UpgradeManager.Instance.TryPurchase(data))
                    RefreshUI();
                else
                    AudioManager.Instance?.PlayUIClick();
            });
        }
    }

    private void Close()
    {
        gameObject.SetActive(false);
        AudioManager.Instance?.PlayUIClick();
    }
}
