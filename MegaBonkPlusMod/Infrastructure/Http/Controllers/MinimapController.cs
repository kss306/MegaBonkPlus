using MegaBonkPlusMod.GameLogic.Minimap;
using MegaBonkPlusMod.Infrastructure.Http.Attributes;

namespace MegaBonkPlusMod.Infrastructure.Http.Controllers;

[ApiController("/api/minimap")]
public class MinimapController : ApiControllerBase
{
    private readonly MinimapStreamer _minimapStreamer;

    public MinimapController(MinimapStreamer minimapStreamer)
    {
        _minimapStreamer = minimapStreamer;
    }

    [HttpGet("/stream")]
    public ApiResponse<object> GetMinimapStream()
    {
        try
        {
            var data = _minimapStreamer.GetData();
            return Ok(data, "Minimap data retrieved");
        }
        catch (System.Exception ex)
        {
            return ServerError<object>(ex.Message);
        }
    }
}