import {setupDashboard} from './core/dashboard.js';
import {setupTooltipListeners} from './features/map/mapTooltips.js';
import {setupQuickActions} from './features/ui/quickActions.js';
import {initializeFilters} from './features/ui/filters.js';
import {setupModal} from './features/ui/modalService.js';
import {setupItemSpawner} from './features/items/itemSpawner.js';
import {setupAutoRestarter} from './features/items/autoRestarter.js';
import {setupHotkeys} from './features/hotkeys/hotkeyManager.js';
import {getAllItems} from './hooks/actions/itemHook.js';

async function initializeApp() {
    console.log('Initializing MegaBonkPlus Frontend...');

    const allItems = await getAllItems();

    setupModal(allItems);
    setupTooltipListeners();
    setupQuickActions();
    initializeFilters();
    setupItemSpawner(allItems);
    setupAutoRestarter(allItems);
    setupHotkeys(allItems);

    setupDashboard();
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeApp);
} else {
    initializeApp();
}