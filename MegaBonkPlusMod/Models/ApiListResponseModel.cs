using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MegaBonkPlusMod.Models;

public class ApiListResponseModel<T>
{
    public ApiListResponseModel(List<T> items)
    {
        Items = items;
        Count = items.Count;
    }

    public ApiListResponseModel()
    {
        Items = new List<T>();
        Count = 0;
    }

    [JsonPropertyName("count")] public int Count { get; set; }

    [JsonPropertyName("items")] public List<T> Items { get; set; }
}