using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel for selecting and viewing available biomes.
/// Addresses issue #39 (biome selection UI).
/// </summary>
public class BiomeSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform biomeButtonContainer;
    [SerializeField] private GameObject biomeButtonPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text selectedBiomeLabel;
    [SerializeField] private TMP_Text selectedBiomeDescription;
    [SerializeField] private Image selectedBiomePreview;

    private void OnEnable()
    {
        PopulateBiomes();
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    private void PopulateBiomes()
    {
        if (biomeButtonContainer == null || biomeButtonPrefab == null) return;

        foreach (Transform child in biomeButtonContainer)
            Destroy(child.gameObject);

        if (BiomeManager.Instance == null) return;

        foreach (BiomeData biome in BiomeManager.Instance.AllBiomes)
        {
            if (biome == null) continue;
            GameObject btn = Instantiate(biomeButtonPrefab, biomeButtonContainer);
            SetupBiomeButton(btn, biome);
        }
    }

    private void SetupBiomeButton(GameObject btn, BiomeData biome)
    {
        bool unlocked = BiomeManager.Instance.IsBiomeUnlocked(biome);

        TMP_Text nameLabel = btn.transform.Find("Name")?.GetComponent<TMP_Text>();
        if (nameLabel != null)
            nameLabel.text = unlocked ? biome.biomeName : "???";

        Image preview = btn.transform.Find("Preview")?.GetComponent<Image>();
        if (preview != null && biome.preview != null)
            preview.sprite = biome.preview;

        Button button = btn.GetComponent<Button>() ?? btn.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.interactable = unlocked;
            button.onClick.AddListener(() =>
            {
                ShowBiomeDetails(biome);
                BiomeManager.Instance?.SelectBiome(biome);
                AudioManager.Instance?.PlayUIClick();
            });
        }

        // Lock overlay
        Transform lockOverlay = btn.transform.Find("Lock");
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(!unlocked);
    }

    private void ShowBiomeDetails(BiomeData biome)
    {
        if (selectedBiomeLabel != null)
            selectedBiomeLabel.text = biome.biomeName;
        if (selectedBiomeDescription != null)
            selectedBiomeDescription.text = biome.description;
        if (selectedBiomePreview != null && biome.preview != null)
            selectedBiomePreview.sprite = biome.preview;
    }

    private void Close()
    {
        gameObject.SetActive(false);
        AudioManager.Instance?.PlayUIClick();
    }
}
