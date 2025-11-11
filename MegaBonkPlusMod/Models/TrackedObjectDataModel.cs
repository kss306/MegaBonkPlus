using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MegaBonkPlusMod.Models;

public class TrackedObjectDataModel
{
    public TrackedObjectDataModel()
    {
        CustomProperties = new Dictionary<string, object>();
    }
    
    [JsonPropertyName("instanceId")] public int InstanceId { get; set; }
    
    [JsonPropertyName("position")] public PositionDataModel Position { get; set; }

    [JsonPropertyName("customProperties")] public Dictionary<string, object> CustomProperties { get; set; }
}