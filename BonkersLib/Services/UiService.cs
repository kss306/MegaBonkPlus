using BonkersLib.Utils;
using UnityEngine;

namespace BonkersLib.Services;

public class UiService
{
    private InventoryHud _inventoryHud;
    private KillsAndGoldCounter _xpAndGoldCounter;

    public void RefreshUi()
    {
        _inventoryHud?.Refresh();
        _xpAndGoldCounter.UpdateGoldCounter();
        _xpAndGoldCounter.UpdateKillCounter();
    }

    internal void OnGameStarted()
    {
        _inventoryHud = null;
        _xpAndGoldCounter = null;

        _inventoryHud = Object.FindObjectOfType<InventoryHud>(true);
        _xpAndGoldCounter = Object.FindObjectOfType<KillsAndGoldCounter>(true);

        if (!_inventoryHud || !_xpAndGoldCounter)
            ModLogger.LogError("[UiService] Could not find required UI elements. Please check your scene setup.");
    }
}