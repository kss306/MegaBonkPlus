using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MegaBonkPlusMod.Models;

public class TrackedObjectData
{
    public TrackedObjectData()
    {
        CustomProperties = new Dictionary<string, object>();
    }

    [JsonPropertyName("position")] public PositionData Position { get; set; }

    [JsonPropertyName("customProperties")] public Dictionary<string, object> CustomProperties { get; set; }
}