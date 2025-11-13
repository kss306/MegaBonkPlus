using System.Collections.Generic;
using MegaBonkPlusMod.GameLogic.Trackers;
using MegaBonkPlusMod.GameLogic.Trackers.Base;
using MegaBonkPlusMod.Infrastructure.Http.Attributes;

namespace MegaBonkPlusMod.Infrastructure.Http.Controllers;

[ApiController("/api/trackers")]
public class TrackerController : ApiControllerBase
{
    private readonly IReadOnlyDictionary<string, BaseTracker> _trackers;

    public TrackerController(IReadOnlyDictionary<string, BaseTracker> trackers)
    {
        _trackers = trackers;
    }
    
    [HttpGet("/all")]
    public ApiResponse<Dictionary<string, object>> GetAllTrackerData()
    {
        try
        {
            var allData = new Dictionary<string, object>();

            foreach (var kvp in _trackers)
            {
                try
                {
                    allData[kvp.Key] = kvp.Value.GetData();
                }
                catch (System.Exception ex)
                {
                    allData[kvp.Key] = new { error = ex.Message };
                }
            }

            return Ok(allData, $"Retrieved data from {allData.Count} trackers");
        }
        catch (System.Exception ex)
        {
            return ServerError<Dictionary<string, object>>(ex.Message);
        }
    }

    [HttpGet("/player")]
    public ApiResponse<object> GetPlayerData() => GetTrackerByKey(TrackerKeys.Player);

    [HttpGet("/chests")]
    public ApiResponse<object> GetChestData() => GetTrackerByKey(TrackerKeys.Chests);

    [HttpGet("/shrines")]
    public ApiResponse<List<object>> GetShrineData()
    {
        try
        {
            var shrineData = new List<object>();
            
            foreach (var key in new[] { 
                TrackerKeys.ChargeShrines, 
                TrackerKeys.GreedShrines, 
                TrackerKeys.MoaiShrines, 
                TrackerKeys.CursedShrines, 
                TrackerKeys.MagnetShrines, 
                TrackerKeys.ChallengeShrines 
            })
            {
                if (_trackers.TryGetValue(key, out var tracker))
                {
                    var data = tracker.GetData();
                    if (data != null)
                    {
                        shrineData.Add(data);
                    }
                }
            }
            
            return Ok(shrineData, "Shrine data retrieved");
        }
        catch (System.Exception ex)
        {
            return ServerError<List<object>>(ex.Message);
        }
    }

    [HttpGet("/enemies")]
    public ApiResponse<object> GetEnemyData() => GetTrackerByKey(TrackerKeys.Bosses);

    [HttpGet("/shady-guys")]
    public ApiResponse<object> GetShadyGuyData() => GetTrackerByKey(TrackerKeys.ShadyGuys);

    [HttpGet("/boss-spawners")]
    public ApiResponse<object> GetBossSpawnerData() => GetTrackerByKey(TrackerKeys.BossSpawner);

    [HttpGet("/microwave")]
    public ApiResponse<object> GetMicrowaveData() => GetTrackerByKey(TrackerKeys.Microwaves);

    private ApiResponse<object> GetTrackerByKey(string key)
    {
        try
        {
            if (_trackers.TryGetValue(key, out var tracker))
            {
                var data = tracker.GetData();
                return Ok(data, $"{key} data retrieved");
            }
            return NotFound<object>($"Tracker '{key}' not found");
        }
        catch (System.Exception ex)
        {
            return ServerError<object>(ex.Message);
        }
    }
}