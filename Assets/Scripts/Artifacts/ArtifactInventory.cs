using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all collected artifacts and applies their passive effects.
/// Addresses issue #31 (Rare artifact system).
/// </summary>
public class ArtifactInventory : MonoBehaviour
{
    public static ArtifactInventory Instance { get; private set; }

    [SerializeField, Min(1)] private int maxSlots = 5;

    private readonly List<ArtifactData> equippedArtifacts = new List<ArtifactData>();

    public IReadOnlyList<ArtifactData> Artifacts => equippedArtifacts;
    public int MaxSlots => maxSlots;

    public event Action OnArtifactsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool TryAdd(ArtifactData artifact)
    {
        if (artifact == null) return false;
        if (equippedArtifacts.Count >= maxSlots) return false;
        if (equippedArtifacts.Contains(artifact)) return false;

        equippedArtifacts.Add(artifact);
        ApplyArtifact(artifact);
        AudioManager.Instance?.PlayLevelUp();
        OnArtifactsChanged?.Invoke();
        return true;
    }

    public bool HasEffect(ArtifactData.ArtifactEffect effect)
    {
        foreach (ArtifactData a in equippedArtifacts)
            if (a.effect == effect) return true;
        return false;
    }

    public float GetTotalEffectValue(ArtifactData.ArtifactEffect effect)
    {
        float total = 0f;
        foreach (ArtifactData a in equippedArtifacts)
            if (a.effect == effect) total += a.effectValue;
        return total;
    }

    private void ApplyArtifact(ArtifactData artifact)
    {
        PlayerStats stats = PlayerStats.Instance;
        if (stats == null) return;

        switch (artifact.effect)
        {
            case ArtifactData.ArtifactEffect.DamageMultiplier:
                // Add a flat bonus equal to baseDamage * effectValue to avoid exponential stacking
                stats.AddDamageBoost(stats.BaseDamage * artifact.effectValue);
                break;
        }
    }
}
