using System.Collections.Generic;
using BonkersLib.Core;
using BonkersLib.Utils;

namespace BonkersLib.Services.World;

public class ItemService
{
    private readonly List<ItemData> _cachedRawItems = new();

    internal void CacheAllRawItems()
    {
        MainThreadDispatcher.Enqueue(BuildCache);
    }

    public List<ItemData> GetAllRawItems()
    {
        return MainThreadDispatcher.Evaluate(() =>
        {
            if (_cachedRawItems.Count == 0)
            {
                BuildCache();
            }

            return new List<ItemData>(_cachedRawItems);
        });
    }

    private void BuildCache()
    {
        _cachedRawItems.Clear();

        var allGameItems = BonkersAPI.Data.ItemData;

        if (allGameItems != null)
        {
            foreach (var kvp in allGameItems)
            {
                var itemData = kvp.Value;
                if (!itemData) continue;
                itemData.description = itemData.GetDescription();
                _cachedRawItems.Add(itemData);
            }
        }

        ModLogger.LogDebug($"[ItemService] {_cachedRawItems.Count} raw items cached");
    }
}