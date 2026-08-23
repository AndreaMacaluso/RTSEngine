using  RTSEngine.Core.Entities.Definitions;
using  RTSEngine.Core.Entities.Loader;

namespace RTSEngine.Tests.Resources;

public sealed class UnitsTests
{

    [Fact]
    public void Load_ShouldLoadDefinitions()
    {
        var loader =
            new UnitDefinitionLoader();

        var definitions =
            loader.Load("Data/Units/units.json");

        Assert.NotEmpty(definitions);

        Assert.Contains(
            definitions,
            d => d.Id == "villager");
    }
  
    [Fact]
    public void Get_ShouldReturnDefinition()
    {
        var repository =
            new UnitDefinitionRepository(
                [
                    new UnitDefinition
                    {
                        Id = "villager",
                        Name = "Villager"
                    }
                ]);

        var definition =
            repository.Get("villager");

        Assert.Equal(
            "Villager",
            definition.Name);
    }
}