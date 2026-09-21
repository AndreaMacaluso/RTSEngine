using System.Text.Json;
using System.Text.Json.Serialization;
using RTSEngine.Core.Entities.Buildings;
using RTSEngine.Core.Entities.Resources;
using RTSEngine.Core.Entities.Units;
using RTSEngine.Core.Players;

namespace RTSEngine.Core.State;

public sealed class GameSnapshot
{
    public int Tick { get; init; }
    public string State { get; init; } = "";
    public object[] Units { get; init; } = [];
    public object[] Buildings { get; init; } = [];
    public object[] Resources { get; init; } = [];
    public object[] Players { get; init; } = [];

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false
    };

    public static GameSnapshot Capture(GameWorld world)
    {
        return new GameSnapshot
        {
            Tick = world.CurrentTick,
            State = world.State.ToString(),
            Units = world.Entities.Units
                .Select(u => (object)new
                {
                    u.Id,
                    u.OwnerId,
                    u.Position.X,
                    u.Position.Y,
                    Health = u.Health.CurrentHealth,
                    MaxHealth = u.Health.MaxHealth,
                    Task = u.CurrentTask.ToString()
                })
                .ToArray(),
            Buildings = world.Entities.Buildings
                .Select(b => (object)new
                {
                    b.Id,
                    b.OwnerId,
                    b.Position.X,
                    b.Position.Y,
                    Health = b.Health.CurrentHealth,
                    MaxHealth = b.Health.MaxHealth,
                    b.IsCompleted,
                    ConstructionProgress = b.ConstructionProgress
                })
                .ToArray(),
            Resources = world.Entities.Resources
                .Select(r => (object)new
                {
                    r.Id,
                    r.Position.X,
                    r.Position.Y,
                    Type = r.ResourceType.ToString(),
                    r.Amount
                })
                .ToArray(),
            Players = world.Players
                .Select(p => (object)new
                {
                    p.Id,
                    p.Name,
                    p.Score,
                    Wood = p.Economy.Get(Map.Runtime.ResourceType.Wood),
                    Food = p.Economy.Get(Map.Runtime.ResourceType.Food),
                    Gold = p.Economy.Get(Map.Runtime.ResourceType.Gold),
                    Stone = p.Economy.Get(Map.Runtime.ResourceType.Stone),
                    PopCurrent = p.Population.Current,
                    PopCap = p.Population.Capacity
                })
                .ToArray()
        };
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, SerializerOptions);
    }

    public static GameSnapshot? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<GameSnapshot>(json, SerializerOptions);
    }
}
