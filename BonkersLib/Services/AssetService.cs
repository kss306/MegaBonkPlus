using System;
using System.Reflection;
using BonkersLib.Core;
using BonkersLib.Utils;
using Il2CppInterop.Runtime;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using DotNetMemoryStream = System.IO.MemoryStream;
using Il2CppMemoryStream = Il2CppSystem.IO.MemoryStream;
using Object = UnityEngine.Object;

namespace BonkersLib.Services;

public class AssetService
{
    private readonly Dictionary<string, GameObject> _prefabCache = new();
    
    public AssetBundle LoadFromAssembly(Assembly assembly, string resourcePath)
    {
        byte[] rawData = null;
        
        using (var resourceStream = assembly.GetManifestResourceStream(resourcePath))
        {
            if (resourceStream == null)
            {
                ModLogger.LogError($"[AssetService] Resource nicht gefunden: {resourcePath}");
                foreach (var res in assembly.GetManifestResourceNames()) ModLogger.LogDebug($"- {res}");
                return null;
            }

            using (var memoryStream = new DotNetMemoryStream())
            {
                resourceStream.CopyTo(memoryStream);
                rawData = memoryStream.ToArray();
            }
        }

        if (rawData == null || rawData.Length == 0) return null;

        return MainThreadDispatcher.Evaluate(() =>
        {
            AssetBundle bundle = null;
            Il2CppMemoryStream il2CppStream = null;

            try
            {
                il2CppStream = new Il2CppMemoryStream(rawData);
                
                bundle = AssetBundle.LoadFromStream(il2CppStream);

                if (bundle)
                {
                    ModLogger.LogDebug($"[AssetService] AssetBundle geladen: {bundle.name}");
                }
                else
                {
                    ModLogger.LogError($"[AssetService] Fehler beim Laden von {resourcePath}");
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"[AssetService] Crash beim Laden: {ex.Message}");
            }
            finally
            {
                il2CppStream?.Dispose();
            }

            return bundle;
        });
    }

    public T LoadAsset<T>(AssetBundle bundle, string assetName) where T : Object
    {
        if (!bundle) return null;

        return MainThreadDispatcher.Evaluate(() =>
        {
            var il2cppType = Il2CppType.Of<T>();
            var request = bundle.LoadAssetAsync(assetName, il2cppType);
            var assetObject = request.asset;

            if (assetObject == null)
            {
                ModLogger.LogWarning($"[AssetService] Asset '{assetName}' nicht gefunden.");
                return null;
            }
            
            return assetObject.TryCast<T>();
        });
    }
    
    public void UnloadBundle(AssetBundle bundle, bool unloadAllLoadedObjects)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (bundle) bundle.Unload(unloadAllLoadedObjects);
        });
    }
    
    public GameObject CachePrefab(AssetBundle bundle, string assetName, string cacheKey)
    {
        return MainThreadDispatcher.Evaluate(() =>
        {
            if (_prefabCache.ContainsKey(cacheKey))
            {
                ModLogger.LogWarning($"[AssetService] Prefab '{cacheKey}' ist bereits gecacht.");
                return _prefabCache[cacheKey];
            }

            var prefab = LoadAsset<GameObject>(bundle, assetName);

            if (!prefab) return null;
            var masterCopy = Object.Instantiate(prefab);
            masterCopy.name = $"Cached_Prefab_{cacheKey}";
            masterCopy.transform.SetParent(null);
            Object.DontDestroyOnLoad(masterCopy);
            masterCopy.SetActive(false); 

            _prefabCache[cacheKey] = masterCopy;
            ModLogger.LogDebug($"[AssetService] Prefab '{cacheKey}' erfolgreich gecacht.");

            return masterCopy;
        });
    }
    
    public GameObject Spawn(string cacheKey, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        return MainThreadDispatcher.Evaluate(() =>
        {
            if (!_prefabCache.TryGetValue(cacheKey, out var originalPrefab))
            {
                ModLogger.LogError($"[AssetService] Konnte '{cacheKey}' nicht spawnen - nicht im Cache gefunden!");
                return null;
            }

            var instance = Object.Instantiate(originalPrefab, position, rotation, parent);
            instance.name = cacheKey;
            instance.SetActive(true); 
            
            return instance;
        });
    }
}