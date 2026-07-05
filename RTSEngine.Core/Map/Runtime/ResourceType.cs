using System.Text.Json.Serialization;
namespace RTSEngine.Core.Map.Runtime;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResourceType
{
    None,
    Wood,
    Stone,
    Gold,
    Food,
}