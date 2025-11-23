import {setupDashboard} from './core/dashboard.js';
import {gameData} from './core/gameData.js';
import {setupTooltipListeners} from './features/map/mapTooltips.js';
import {setupQuickActions} from './features/ui/quickActions.js';
import {initializeFilters} from './features/ui/filters.js';
import {setupModal} from './features/ui/modalService.js';
import {setupItemSpawner} from './features/items/itemSpawner.js';
import {setupAutoRestarter} from './features/items/autoRestarter.js';
import {setupHotkeys} from './features/hotkeys/hotkeyManager.js';
import {setupWeaponActions} from './features/inventory/weaponActions.js';
import {setupCheatActions} from "./features/cheats/cheatsActions.js";

async function initializeApp() {
    console.log('Initializing MegaBonkPlus Frontend...');

    await gameData.initialize();

    const allItems = gameData.getItems();

    setupModal(allItems);
    setupTooltipListeners();
    setupQuickActions();
    initializeFilters();
    setupItemSpawner(allItems);
    setupAutoRestarter(allItems);
    setupHotkeys(allItems);
    setupWeaponActions();
    setupCheatActions();

    setupDashboard();
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeApp);
} else {
    initializeApp();
}