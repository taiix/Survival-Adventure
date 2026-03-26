using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;

    private void OnEnable()
    {
        EventManager.OnPlayerHealthChanged.AddListener(UpdateHealthUI);
        EventManager.OnPlayerStaminaChanged.AddListener(UpdateStaminaUI);
    }

    private void OnDisable()
    {
        EventManager.OnPlayerHealthChanged.RemoveListener(UpdateHealthUI);
        EventManager.OnPlayerStaminaChanged.RemoveListener(UpdateStaminaUI);
    }

    private void UpdateHealthUI(float currentHealth)
    {
        throw new NotImplementedException();
    }

    private void UpdateStaminaUI(float currentStamina)
    {
        staminaSlider.value = currentStamina;
    }
}
