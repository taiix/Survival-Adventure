public sealed class PlayerStaminaController
{
    private readonly PlayerStamina stamina;

    public bool HasStamina => stamina.HasStamina;
    public float CurrentStamina => stamina.CurrentStamina;
    public float MaxStamina => stamina.MaxStamina;

    public PlayerStaminaController(
        float maxStamina,
        float drainRate,
        float regenRate,
        float regenDelay)
    {
        stamina = new PlayerStamina(maxStamina, drainRate, regenRate, regenDelay);
    }

    public void DrainStamina(float deltaTime)
    {
        stamina.UseStamina(deltaTime);
    }

    public void Update(float deltaTime)
    {
        stamina.Update(deltaTime);
    }
}