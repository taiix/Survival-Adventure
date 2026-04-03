using UnityEngine;

/// <summary>
/// Defines behavior for different NPC types.
/// Implement this for Blacksmith, Teleporter, Merchant, Innkeeper, etc.
/// </summary>
public interface INPCBehaviour
{
    /// <summary>Called when the player interacts with the NPC.</summary>
    void OnInteract();

    /// <summary>Called when the interaction ends (player closes UI or walks away).</summary>
    void OnInteractionEnd();

    /// <summary>Optional: Get the UI panel to display (can return null).</summary>
    GameObject GetUIPanel();

    /// <summary>Optional: Called every frame while interacting.</summary>
    void OnInteractionUpdate();
}