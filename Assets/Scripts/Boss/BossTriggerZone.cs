using UnityEngine;

/// <summary>
/// Trigger volume that starts a boss encounter when the player enters.
/// Place as a trigger collider in the boss arena.
/// Addresses issues #23, #38 (boss system, boss intro).
/// </summary>
[RequireComponent(typeof(Collider))]
public class BossTriggerZone : MonoBehaviour
{
    [SerializeField] private BossEncounterManager encounterManager;
    [SerializeField] private BossIntroUI introUI;
    [SerializeField] private string bossName = "The Iron Knight";
    [SerializeField] private string bossSubtitle = "Guardian of the Dungeon";
    [SerializeField] private bool triggerOnce = true;

    private bool triggered;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && triggered) return;

        triggered = true;

        // Show intro UI
        introUI?.Show(bossName, bossSubtitle);

        // Start encounter
        if (encounterManager != null)
            encounterManager.TriggerEncounter();
    }
}
