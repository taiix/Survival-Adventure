using System;
using UnityEngine;

public sealed class PlayerStamina : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField, Min(1f)] private float maxStamina = 100f;
    [SerializeField, Min(0.1f)] private float drainRate = 20f;
    [SerializeField, Min(0.1f)] private float regenRate = 10f;
    [SerializeField, Min(0f)] private float regenDelay = 1.5f;

    public float MaxStamina => maxStamina;
    public float CurrentStamina { get; private set; }
    public bool HasStamina => CurrentStamina > 0f;

    public event Action<float, float> OnStaminaChanged;

    private float timeSinceLastDrain;

    private void Awake()
    {
        CurrentStamina = maxStamina;
        timeSinceLastDrain = regenDelay;
    }

    public void UseStamina(float deltaTime)
    {
        if (CurrentStamina <= 0f)
        {
            return;
        }

        CurrentStamina = Mathf.Max(0f, CurrentStamina - drainRate * deltaTime);
        timeSinceLastDrain = 0f;
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
    }

    private void Update()
    {
        timeSinceLastDrain += Time.deltaTime;

        if (timeSinceLastDrain >= regenDelay && CurrentStamina < maxStamina)
        {
            CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + regenRate * Time.deltaTime);
            OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
        }
    }
}
