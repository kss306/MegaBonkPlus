using System;
using System.Text.Json;
using BonkersLib.Core;
using MegaBonkPlusMod.Infrastructure.Http.Attributes;
using MegaBonkPlusMod.Response;

namespace MegaBonkPlusMod.Infrastructure.Http.Controllers;

public class TimeScaleRequest
{
    public float TimeScale { get; set; }
}

[ApiController("/api/game")]
public class GameStateController : ApiControllerBase
{
    [HttpGet("/state")]
    public ApiResponse<GameStateResponse> GetGameState()
    {
        try
        {
            var gameState = BonkersAPI.Game;

            var response = new GameStateResponse
            {
                IsInGame = gameState.IsInGame,
                CurrentMap = gameState.StageName,
                MapTier = gameState.StageTier,
                StageTime = gameState.StageTime,
                TimeAlive = gameState.TimeAlive,
                BossCurses = gameState.BossCurses
            };

            return Ok(response, "Game state retrieved");
        }
        catch (Exception ex)
        {
            return ServerError<GameStateResponse>(ex.Message);
        }
    }

    [HttpGet("/time-scale")]
    public ApiResponse<float> GetTimeScale()
    {
        try
        {
            return Ok(BonkersAPI.Game.CurrentTimeScale, "Time scale retrieved");
        }
        catch (Exception ex)
        {
            return ServerError<float>(ex.Message);
        }
    }

    [HttpPost("/time-scale")]
    public ApiResponse<bool> SetTimeScale(JsonElement payload)
    {
        try
        {
            float newScale = 1.0f;
            bool found = false;

            if (payload.ValueKind == JsonValueKind.Object)
            {
                if (payload.TryGetProperty("timeScale", out var prop))
                {
                    newScale = prop.GetSingle();
                    found = true;
                }
                else if (payload.TryGetProperty("value", out prop))
                {
                    newScale = prop.GetSingle();
                    found = true;
                }
            }

            if (!found)
            {
                return BadRequest<bool>("Missing 'timeScale' or 'value' property in JSON");
            }

            BonkersAPI.Game.SetTimeScale(newScale, true);
            return Ok(true, $"TimeScale set to {newScale}");
        }
        catch (Exception ex)
        {
            return ServerError<bool>(ex.Message);
        }
    }
}