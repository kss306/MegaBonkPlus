using System.Text.Json;
using MegaBonkPlusMod.Core;
using MegaBonkPlusMod.Infrastructure.Http.Attributes;

namespace MegaBonkPlusMod.Infrastructure.Http.Controllers;

[ApiController("/api/hotkeys")]
public class HotkeyController : ApiControllerBase
{
    [HttpGet("")]
    public ApiResponse<object> GetHotkeyConfig()
    {
        try
        {
            var config = HotkeyManager.GetCurrentConfig();
            return Ok(config, "Hotkey configuration retrieved successfully");
        }
        catch (System.Exception ex)
        {
            return ServerError<object>(ex.Message);
        }
    }

    [HttpPost("")]
    public ApiResponse SetHotkeyConfig(JsonElement payload)
    {
        try
        {
            HotkeyManager.UpdateConfig(payload);
            return Ok("Hotkeys updated");
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}