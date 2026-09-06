using RTSEngine.Core.Entities.Definitions;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;

namespace RTSEngine.Core.Entities.States;

public class CombatState
{
    public CombatPhase Phase { get; set; }
    public int? TargetEntityId { get; set; }
    public int CooldownTicks { get; set; }

    public int MeleeAttack { get; set; }
    public int RangedAttack { get; set; }
    public int AttackRange { get; set; }
    public int AttackCooldownTicks { get; set; }
    public int MeleeArmor { get; set; }
    public int RangedArmor { get; set; }
    public EntityCategory? BonusVs { get; set; }
    public int BonusDamage { get; set; }
    public GridPosition? TargetGroundPosition { get; set; }

    public bool IsOnCooldown => CooldownTicks > 0;
    public bool IsRanged => AttackRange > 1;

    public CombatState() { }

    public CombatState(UnitDefinition definition)
    {
        MeleeAttack = definition.MeleeAttack;
        RangedAttack = definition.RangedAttack;
        AttackRange = definition.AttackRange;
        AttackCooldownTicks = definition.AttackCooldownTicks;
        MeleeArmor = definition.MeleeArmor;
        RangedArmor = definition.RangedArmor;
        BonusVs = definition.BonusVs;
        BonusDamage = definition.BonusDamage;
    }

    public CombatState(BuildingDefinition definition)
    {
        RangedAttack = definition.Attack;
        AttackRange = definition.AttackRange;
        AttackCooldownTicks = definition.AttackCooldownTicks;
        MeleeArmor = definition.MeleeArmor;
        RangedArmor = definition.RangedArmor;
    }

    public int GetAttackDamage(bool isRangedAttack)
    {
        return isRangedAttack ? RangedAttack : MeleeAttack;
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
        TargetGroundPosition = null;
        CooldownTicks = 0;
    }
}
