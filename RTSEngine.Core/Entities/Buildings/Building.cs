using RTSEngine.Core.Entities;
namespace RTSEngine.Core.Entities.Buildings;

public sealed class Building : Entity
{
    public override bool IsBlocking => true; 
    public BuildingDefinition Definition { get; private set; }
}