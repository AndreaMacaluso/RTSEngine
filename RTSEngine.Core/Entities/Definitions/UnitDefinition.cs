using  RTSEngine.Core.Map.Runtime;
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
    public int AttackDamage { get; set; }
    public int AttackRange { get; set; } = 1;
    public int AttackCooldownTicks { get; set; } = 4;
    public bool CanGather => GatherCapacity > 0;
    public bool CanBuild => BuildableBuildings.Count > 0;
    public bool CanAttack => AttackDamage > 0;
}