using UnityEngine;
using TMPro;

/// <summary>
/// Displays the player's gold / material count in the HUD.
/// Addresses issues #33, #53 (currency UI, gold/materials counter).
/// </summary>
public class GoldUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldLabel;
    [SerializeField] private string prefix = "Gold: ";

    private void OnEnable()
    {
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged += Refresh;
            Refresh(GoldManager.Instance.Gold);
        }
    }

    private void OnDisable()
    {
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged -= Refresh;
    }

    private void Refresh(int amount)
    {
        if (goldLabel != null)
            goldLabel.text = prefix + amount.ToString();
    }
}
