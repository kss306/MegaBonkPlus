using Il2CppSystem.Collections.Generic;

namespace BonkersLib.Utils;

public static class CollectionExtensions
{
    public static Dictionary<TKey, TValue> ToSafeCopy<TKey, TValue>(this Dictionary<TKey, TValue> source)
    {
        var result = new Dictionary<TKey, TValue>();

        if (source == null) return result;

        foreach (var kvp in source)
        {
            if (!result.ContainsKey(kvp.Key))
            {
                result.Add(kvp.Key, kvp.Value);
            }
        }

        return result;
    }
}