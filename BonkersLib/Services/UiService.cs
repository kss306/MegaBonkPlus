using BonkersLib.Core;
using BonkersLib.Utils;
using UnityEngine;

namespace BonkersLib.Services;

public class UiService
{
    private InventoryHud _inventoryHud;
    private KillsAndGoldCounter _xpAndGoldCounter;

    public void RefreshUi()
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            if (_inventoryHud)
                _inventoryHud.Refresh();

            if (_xpAndGoldCounter)
            {
                _xpAndGoldCounter.UpdateGoldCounter();
                _xpAndGoldCounter.UpdateKillCounter();
            }
        });
    }

    internal void OnGameStarted()
    {
        _inventoryHud = null;
        _xpAndGoldCounter = null;

        _inventoryHud = Object.FindObjectOfType<InventoryHud>(true);
        _xpAndGoldCounter = Object.FindObjectOfType<KillsAndGoldCounter>(true);

        if (!_inventoryHud || !_xpAndGoldCounter)
            ModLogger.LogDebug("[UiService] Could not find required UI elements. Please check your scene setup.");
    }
}