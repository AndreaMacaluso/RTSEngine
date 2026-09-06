using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Entities.States;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities;

namespace RTSEngine.Core.Systems;

public static class DamageCalculator
{
    public static int CalculateDamage(CombatState attacker, IHittable target)
    {
        return target switch
        {
            Unit u => CalculateDamage(attacker, u),
            Building b => CalculateDamage(attacker, b),
            _ => 1
        };
    }

    public static int CalculateDamage(CombatState attacker, Entities.Entity target)
    {
        return target is IHittable hittable
            ? CalculateDamage(attacker, hittable)
            : 1;
    }

    public static int CalculateDamage(CombatState attacker, Unit target)
    {
        return CalculateDamageCore(
            attacker,
            target.Definition.Category,
            target.Combat.MeleeArmor,
            target.Combat.RangedArmor);
    }

    public static int CalculateDamage(CombatState attacker, Building target)
    {
        return CalculateDamageCore(
            attacker,
            EntityCategory.Building,
            target.Definition.MeleeArmor,
            target.Definition.RangedArmor);
    }

    public static int CalculateDamage(
        int attackDamage,
        bool isRanged,
        int targetRangedArmor)
    {
        return Math.Max(attackDamage - targetRangedArmor, 1);
    }

    private static int CalculateDamageCore(
        CombatState attacker,
        EntityCategory targetCategory,
        int targetMeleeArmor,
        int targetRangedArmor)
    {
        bool isRanged = attacker.IsRanged;
        int attackDamage = attacker.GetAttackDamage(isRanged);
        int targetArmor = isRanged ? targetRangedArmor : targetMeleeArmor;

        int baseDamage = Math.Max(attackDamage - targetArmor, 1);

        int bonusDamage = 0;
        if (attacker.BonusVs.HasValue && attacker.BonusVs.Value == targetCategory)
        {
            bonusDamage = attacker.BonusDamage;
        }

        return baseDamage + bonusDamage;
    }
}
