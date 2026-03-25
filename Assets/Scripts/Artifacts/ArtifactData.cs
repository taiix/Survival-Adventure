using UnityEngine;

/// <summary>
/// ScriptableObject for a Rare Artifact that enhances special abilities.
/// Addresses issue #31 (Rare artifact system).
/// </summary>
[CreateAssetMenu(fileName = "NewArtifact", menuName = "Survival Adventure/Artifact Data")]
public class ArtifactData : ScriptableObject
{
    public enum ArtifactEffect
    {
        DamageMultiplier,
        InfiniteStamina,
        LifeSteal,
        DashDamage,
        ReflectDamage
    }

    [Header("Identity")]
    public string artifactName = "Ancient Relic";
    [TextArea] public string description;
    public Sprite icon;

    [Header("Effect")]
    public ArtifactEffect effect;
    [Min(0f)] public float effectValue = 0.1f;
}
