using System;
using BonkersLib.Core;
using MegaBonkPlusMod.Infrastructure.Http.Attributes;
using MegaBonkPlusMod.Response;

namespace MegaBonkPlusMod.Infrastructure.Http.Controllers;

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
}