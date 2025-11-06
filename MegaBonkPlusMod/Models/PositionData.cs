using System;
using System.Text.Json.Serialization;
using UnityEngine;

namespace MegaBonkPlusMod.Models;

public struct PositionData
{
    [JsonPropertyName("x")] public float X { get; set; }

    [JsonPropertyName("y")] public float Y { get; set; }

    [JsonPropertyName("z")] public float Z { get; set; }

    public static PositionData FromVector3(Vector3 vector)
    {
        return new PositionData
        {
            X = (float)Math.Round(vector.x, 2),
            Y = (float)Math.Round(vector.y, 2),
            Z = (float)Math.Round(vector.z, 2)
        };
    }
}