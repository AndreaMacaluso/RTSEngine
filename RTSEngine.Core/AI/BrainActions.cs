using RTSEngine.Core.Helpers;

namespace RTSEngine.Core.AI;

public static class BrainActions
{
    public const string None = "none";
    public const string TrainVillager = "train_villager";
    public const string TrainMilitia = "train_militia";
    public const string BuildBarracks = "build_barracks";
    public const string BuildHouse = "build_house";
    public const string AssignGatherers = "assign_gatherers";
    public const string EngageEnemies = "engage_enemies";

    public static string? GetDefinition(string action)
    {
        return action switch
        {
            TrainVillager => EntityIds.Villager,
            TrainMilitia => EntityIds.Militia,
            BuildBarracks => EntityIds.Barracks,
            BuildHouse => EntityIds.House,
            _ => null
        };
    }
}
