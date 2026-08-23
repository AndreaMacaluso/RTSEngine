using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Core.Entities.States;

public class HealthState
{
    public int MaxHealth { get; }
    public int CurrentHealth { get; set; }
    public bool IsDead => CurrentHealth <= 0;

    public HealthState(UnitDefinition definition)
    {
        MaxHealth = definition.MaxHealth;
        CurrentHealth = definition.MaxHealth; 
    }
    
    // current health is zero for building since they start as foundations
    public HealthState(BuildingDefinition definition)
    {
        MaxHealth = definition.MaxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        CurrentHealth -= amount;
        if (CurrentHealth < 0) CurrentHealth = 0;
    }
}
