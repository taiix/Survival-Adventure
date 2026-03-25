using UnityEngine;
using TMPro;

/// <summary>
/// Shows the boss's name above the health bar during an encounter.
/// Addresses issue #41 (boss name display).
/// </summary>
public class BossNameDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text nameLabel;

    private void OnEnable()
    {
        if (BossEncounterManager.Instance != null)
        {
            BossEncounterManager.Instance.OnBossEncounterStart += HandleEncounterStart;
            BossEncounterManager.Instance.OnBossEncounterEnd   += HandleEncounterEnd;
        }
    }

    private void OnDisable()
    {
        if (BossEncounterManager.Instance != null)
        {
            BossEncounterManager.Instance.OnBossEncounterStart -= HandleEncounterStart;
            BossEncounterManager.Instance.OnBossEncounterEnd   -= HandleEncounterEnd;
        }
    }

    private void HandleEncounterStart()
    {
        // The name was already set via BossIntroUI; mirror it here if desired.
        gameObject.SetActive(true);
    }

    private void HandleEncounterEnd()
    {
        gameObject.SetActive(false);
    }

    public void SetName(string bossName)
    {
        if (nameLabel != null)
            nameLabel.text = bossName;
    }
}
