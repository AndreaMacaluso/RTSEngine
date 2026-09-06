using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Map.Runtime;
namespace RTSEngine.Core.Entities.Definitions;

public class UnitDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int MaxHealth { get; set; }
    public float MovementSpeed { get; set; }
    public List<ResourceCost> Costs { get; init; } = [];
    public int ProductionTimeTicks { get; set; }
    public int GatherCapacity { get; set; }
    public List<string> BuildableBuildings { get; set; } = [];

    public UnitCategory Category { get; set; } = UnitCategory.Infantry;

    public int MeleeAttack { get; set; }
    public int RangedAttack { get; set; }
    public int AttackRange { get; set; } = 1;
    public int AttackCooldownTicks { get; set; } = 4;

    public int MeleeArmor { get; set; }
    public int RangedArmor { get; set; }

    public EntityCategory? BonusVs { get; set; }
    public int BonusDamage { get; set; }

    public bool CanGather => GatherCapacity > 0;
    public bool CanBuild => BuildableBuildings.Count > 0;
    public bool CanAttack => MeleeAttack > 0 || RangedAttack > 0;
}
