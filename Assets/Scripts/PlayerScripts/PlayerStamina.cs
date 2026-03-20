using System;

public sealed class PlayerStamina
{
    private readonly float maxStamina;
    private readonly float drainRate;
    private readonly float regenRate;
    private readonly float regenDelay;

    private float currentStamina;
    private float timeSinceLastDrain;

    public PlayerStamina(float maxStamina, float drainRate, float regenRate, float regenDelay)
    {
        this.maxStamina = maxStamina;
        this.drainRate = drainRate;
        this.regenRate = regenRate;
        this.regenDelay = regenDelay;

        currentStamina = maxStamina;
        timeSinceLastDrain = regenDelay;
    }

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;
    public bool HasStamina => currentStamina > 0f;

    public event Action<float, float> OnStaminaChanged;

    public void UseStamina(float deltaTime)
    {
        if (currentStamina <= 0f)
        {
            return;
        }

        currentStamina = Math.Max(0f, currentStamina - drainRate * deltaTime);
        timeSinceLastDrain = 0f;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void Update(float deltaTime)
    {
        timeSinceLastDrain += deltaTime;

        if (timeSinceLastDrain < regenDelay || currentStamina >= maxStamina)
        {
            return;
        }

        currentStamina = Math.Min(maxStamina, currentStamina + regenRate * deltaTime);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
}