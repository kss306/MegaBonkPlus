using BonkersLib.Core;
using BonkersLib.Utils;
using UnityEngine;

namespace BonkersLib.Modding;

public static class CharacterSwapper
{
    public static void HijackAmogWithKratos()
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (!AlwaysManager.Instance || AlwaysManager.Instance.dataManager == null)
            {
                ModLogger.LogError("[CharacterSwapper] AlwaysManager oder DataManager nicht gefunden!");
                return;
            }

            string resourcePath = "BonkersLib.Assets.kratos"; 
            var bundle = BonkersAPI.Asset.LoadFromAssembly(typeof(CharacterSwapper).Assembly, resourcePath);
            
            if (!bundle) return;

            GameObject kratosPrefab = BonkersAPI.Asset.LoadAsset<GameObject>(bundle, "kratos_rigged");

            if (!kratosPrefab)
            {
                ModLogger.LogError("[CharacterSwapper] Kratos Prefab nicht gefunden!");
                return;
            }

            var dataManager = AlwaysManager.Instance.dataManager;
            
            if (!dataManager.characterData.ContainsKey(ECharacter.Amog))
            {
                ModLogger.LogError("[CharacterSwapper] ECharacter.Amog nicht in DataManager gefunden!");
                return;
            }

            var amogData = dataManager.characterData[ECharacter.Amog];
            GameObject originalAmogPrefab = amogData.prefab;

            if (originalAmogPrefab)
            {
                var originalAnimator = originalAmogPrefab.GetComponentInChildren<Animator>();
                var kratosAnimator = kratosPrefab.GetComponentInChildren<Animator>();

                if (originalAnimator && kratosAnimator)
                {
                    if (originalAnimator.runtimeAnimatorController)
                    {
                        kratosAnimator.runtimeAnimatorController = originalAnimator.runtimeAnimatorController;

                        ModLogger.LogInfo($"[CharacterSwapper] Controller '{originalAnimator.runtimeAnimatorController.name}' auf Kratos übertragen.");
                    }
                    else
                    {
                        ModLogger.LogError("[CharacterSwapper] Original Amog Prefab hat keinen RuntimeAnimatorController!");
                    }
                }
                else
                {
                    ModLogger.LogError("[CharacterSwapper] Animator auf Original oder Kratos nicht gefunden.");
                }
            }

            amogData.prefab = kratosPrefab;
            amogData.name = "Kratos (Amog Swap)"; 

            ModLogger.LogInfo("[CharacterSwapper] Amog wurde erfolgreich durch Kratos ersetzt!");
        });
    }
}