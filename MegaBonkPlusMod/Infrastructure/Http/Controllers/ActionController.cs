
using System.Collections.Generic;
using System.Text.Json;
using MegaBonkPlusMod.Actions.Base;
using MegaBonkPlusMod.Infrastructure.Http.Attributes;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Infrastructure.Http.Controllers;

[ApiController("/api/actions")]
public class ActionController : ApiControllerBase
{
    private readonly ActionHandler _actionHandler;

    public ActionController(ActionHandler actionHandler)
    {
        _actionHandler = actionHandler;
    }

    [HttpGet("/state")]
    public ApiResponse<Dictionary<string, object>> GetActionStates()
    {
        try
        {
            var states = _actionHandler.GetActionStates();
            return Ok(states, "Action states retrieved");
        }
        catch (System.Exception ex)
        {
            return ServerError<Dictionary<string, object>>(ex.Message);
        }
    }

    [HttpPost("/execute")]
    public ApiResponse ExecuteAction(JsonElement payload)
    {
        if (!payload.TryGetProperty("action", out var actionElement))
        {
            return BadRequest("Missing 'action' property");
        }

        string actionName = actionElement.GetString();
        string resultMessage = null;

        try
        {
            MainThreadActionQueue.QueueAction(() => 
            { 
                resultMessage = _actionHandler.HandleAction(actionName, payload); 
            });

            System.Threading.Thread.Sleep(50);

            return Ok(resultMessage ?? $"Action '{actionName}' queued successfully");
        }
        catch (System.Exception ex)
        {
            return ServerError(ex.Message);
        }
    }
}