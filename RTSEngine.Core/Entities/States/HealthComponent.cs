namespace RTSEngine.Core.Entities.States;

public class HealthComponent
{
    public int MaxHealth { get; }
    public int CurrentHealth { get; set; }
    public bool IsDead => CurrentHealth <= 0;

    public HealthComponent(int maxHealth, int startHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = startHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        CurrentHealth -= amount;
        if (CurrentHealth < 0) CurrentHealth = 0;
    }
}
