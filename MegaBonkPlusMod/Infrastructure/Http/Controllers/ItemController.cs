using System;
using System.Collections.Generic;
using System.Linq;
using BonkersLib.Core;
using MegaBonkPlusMod.Infrastructure.Http.Attributes;
using MegaBonkPlusMod.Models;

namespace MegaBonkPlusMod.Infrastructure.Http.Controllers;

[ApiController("/api/items")]
public class ItemController : ApiControllerBase
{
    [HttpGet("/all")]
    public ApiResponse<List<ItemViewModel>> GetAllItems()
    {
        try
        {
            var rawItems = BonkersAPI.Item.GetAllRawItems();
            var items = rawItems.Select(itemData => new ItemViewModel
            {
                id = itemData.eItem.ToString().ToLowerInvariant(),
                name = itemData.name,
                description = itemData.description,
                inItemPool = itemData.inItemPool,
                rarity = itemData.rarity.ToString()
            }).ToList();

            return Ok(items, $"Retrieved {items.Count} items");
        }
        catch (Exception ex)
        {
            return ServerError<List<ItemViewModel>>(ex.Message);
        }
    }
}