using System.Collections.Generic;
using BonkersLib.Core;
using BonkersLib.Utils;

namespace BonkersLib.Services.World;

public class ItemService
{
    private readonly List<ItemData> _cachedRawItems = new();

    internal void CacheAllRawItems()
    {
        _cachedRawItems.Clear();
        var allGameItems = BonkersAPI.Data.ItemData;

        foreach (var item in allGameItems)
        {
            var tempItem = item.value;
            tempItem.description = tempItem.GetDescription();
            _cachedRawItems.Add(tempItem);
        }

        ModLogger.LogDebug($"[ItemService] {_cachedRawItems.Count} raw items cached");
    }

    public List<ItemData> GetAllRawItems()
    {
        return _cachedRawItems;
    }

    internal void Update()
    {
    }
}