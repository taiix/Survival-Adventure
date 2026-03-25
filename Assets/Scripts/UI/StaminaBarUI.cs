using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the player's stamina bar in the HUD.
/// </summary>
public class StaminaBarUI : MonoBehaviour
{
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullColor = Color.yellow;
    [SerializeField] private Color emptyColor = new Color(0.4f, 0.4f, 0f);

    private PlayerController playerController;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        // PlayerStaminaController is private inside PlayerController, so we can't
        // hook an event easily. Instead we poll each frame.
        if (playerController == null) return;

        // Access stamina through the component — requires exposing a getter.
        // We query the StaminaReader helper if available, otherwise skip.
        StaminaReader reader = playerController.GetComponent<StaminaReader>();
        if (reader == null) return;

        float ratio = reader.StaminaRatio;
        if (staminaSlider != null) staminaSlider.value = ratio;
        if (fillImage != null) fillImage.color = Color.Lerp(emptyColor, fullColor, ratio);
    }
}
