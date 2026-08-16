using RTSEngine.Core.Entities.Definitions;

namespace RTSEngine.Core.Entities.States;

public class CombatState
{
    public CombatPhase Phase { get; set; }
    public int? TargetEntityId { get; set; }
    public int CooldownTicks { get; set; }
    public int AttackDamage { get; set; }
    public int AttackRange { get; set; }
    public int AttackCooldownTicks { get; set; }

    public bool IsOnCooldown => CooldownTicks > 0;

    public CombatState(UnitDefinition definition)
    {
        AttackDamage = definition.AttackDamage;
        AttackRange = definition.AttackRange;
        AttackCooldownTicks = definition.AttackCooldownTicks;
    }

    public void TickCooldown()
    {
        if (CooldownTicks > 0)
        {
            CooldownTicks--;
        }
    }

    public void ResetCooldown()
    {
        CooldownTicks = AttackCooldownTicks;
    }

    public void Clear()
    {
        Phase = CombatPhase.Idle;
        TargetEntityId = null;
        CooldownTicks = 0;
    }
}
