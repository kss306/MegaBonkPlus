using System.Collections.Generic;
using BonkersLib.Utils;

namespace BonkersLib.Services.World;

public class ItemService
{
    private List<ItemData> _cachedRawItems = new();

    internal void CacheAllRawItems()
    {
        _cachedRawItems.Clear();
        var allGameItems = AlwaysManager.Instance.dataManager.itemData;

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

    internal void Update() { }
}