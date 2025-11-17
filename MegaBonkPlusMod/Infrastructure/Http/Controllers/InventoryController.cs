using System;
using System.Collections.Generic;
using BonkersLib.Core;
using MegaBonkPlusMod.Infrastructure.Http.Attributes;
using MegaBonkPlusMod.Models;

namespace MegaBonkPlusMod.Infrastructure.Http.Controllers;

[ApiController("/api")]
public class InventoryController : ApiControllerBase
{
    [HttpGet("/weapons/all")]
    public ApiResponse<List<WeaponViewModel>> GetAllWeapons()
    {
        try
        {
            if (!BonkersAPI.Data.CurrentDataManager || BonkersAPI.Data.WeaponData == null)
                return ServerError<List<WeaponViewModel>>("Weapon data not initialized");

            var allWeapons = new List<WeaponViewModel>();

            foreach (var entry in BonkersAPI.Data.WeaponData)
            {
                var eWeapon = entry.Key;
                var wd = entry.Value;

                var vm = new WeaponViewModel
                {
                    id = eWeapon.ToString().ToLowerInvariant(),
                    name = wd.damageSourceName ?? wd.name,
                    description = wd.description
                };

                allWeapons.Add(vm);
            }

            allWeapons.Sort((a, b) =>
                string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

            return Ok(allWeapons, $"Retrieved {allWeapons.Count} weapons");
        }
        catch (Exception ex)
        {
            return ServerError<List<WeaponViewModel>>(ex.Message);
        }
    }

    [HttpGet("/weapons/inventory")]
    public ApiResponse<WeaponInventoryViewModel> GetWeaponInventory()
    {
        try
        {
            if (!BonkersAPI.Game.IsInGame)
                return ServerError<WeaponInventoryViewModel>("Not in game");

            var weaponService = BonkersAPI.Weapon;
            var dict = weaponService?.CurrentWeapons;

            var result = new WeaponInventoryViewModel();
            if (dict != null)
                foreach (var kvp in dict)
                {
                    var eWeapon = kvp.Key;
                    var wb = kvp.Value;
                    var wd = weaponService.GetWeaponDataFromWeaponBase(wb);

                    result.weapons.Add(new WeaponSlotViewModel
                    {
                        id = eWeapon.ToString().ToLowerInvariant(),
                        name = wd.damageSourceName ?? wd.name,
                        level = wb.level
                    });
                }

            return Ok(result, $"Retrieved {result.weapons.Count} equipped weapons");
        }
        catch (Exception ex)
        {
            return ServerError<WeaponInventoryViewModel>(ex.Message);
        }
    }

    [HttpGet("/tomes/all")]
    public ApiResponse<List<TomeViewModel>> GetAllTomes()
    {
        try
        {
            if (!BonkersAPI.Data.CurrentDataManager || BonkersAPI.Data.TomeData == null)
                return ServerError<List<TomeViewModel>>("Tome data not initialized");

            var allTomes = new List<TomeViewModel>();

            foreach (var entry in BonkersAPI.Data.TomeData)
            {
                var eTome = entry.Key;
                var td = entry.Value;
                var tomeName = td.GetInternalName();
                var tomeDesc = td.GetDescription();

                var tm = new TomeViewModel
                {
                    id = eTome.ToString().ToLowerInvariant(),
                    name = tomeName,
                    description = tomeDesc
                };

                allTomes.Add(tm);
            }

            allTomes.Sort((a, b) =>
                string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

            return Ok(allTomes, $"Retrieved {allTomes.Count} tomes");
        }
        catch (Exception ex)
        {
            return ServerError<List<TomeViewModel>>(ex.Message);
        }
    }

    [HttpGet("/tomes/inventory")]
    public ApiResponse<TomeInventoryViewModel> GetTomeInventory()
    {
        try
        {
            if (!BonkersAPI.Game.IsInGame)
                return ServerError<TomeInventoryViewModel>("Not in game");

            var tomeService = BonkersAPI.Tome;
            var dict = tomeService?.CurrentTomes;

            var result = new TomeInventoryViewModel();
            if (dict != null)
                foreach (var kvp in dict)
                {
                    var eTome = kvp.Key;
                    var td = BonkersAPI.Tome.GetTomeDataFromEnum(eTome);
                    var tomeName = td.GetInternalName();
                    var tomeLevel = td.GetLevel();

                    result.tomes.Add(new TomeSlotViewModel
                    {
                        id = eTome.ToString().ToLowerInvariant(),
                        name = tomeName,
                        level = tomeLevel
                    });
                }

            return Ok(result, $"Retrieved {result.tomes.Count} equipped tomes");
        }
        catch (Exception ex)
        {
            return ServerError<TomeInventoryViewModel>(ex.Message);
        }
    }
}