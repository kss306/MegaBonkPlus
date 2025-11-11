import { updateDashboard } from './services/dashboardService.js';
import { setupTooltipListeners } from './services/mapService.js';
import { setupQuickActions } from './services/actionService.js';
import { initializeFilters } from './services/filterService.js';
import { setupAutoRestarter } from "./services/autoRestarterService.js";
import { setupItemSpawner } from "./services/itemSpawnerService.js";
import { setupModal } from "./services/modalService.js";
import { fetchData } from "./services/apiService.js";

console.log("Dashboard wird initialisiert...");

document.addEventListener('DOMContentLoaded', async () => {

    let allItems = [];
    try {
        const itemsFromBackend = await fetchData('/api/items/all');
        if (itemsFromBackend && Array.isArray(itemsFromBackend) && itemsFromBackend.length > 0) {
            allItems = itemsFromBackend;
        }
    } catch (e) {
        console.error("Konnte Item-Liste in app.js nicht laden:", e);
    }

    setupQuickActions();
    setupTooltipListeners();
    initializeFilters(updateDashboard);
    setupModal(allItems);
    setupAutoRestarter(allItems);
    setupItemSpawner(allItems);

    setInterval(updateDashboard, 1000);
    updateDashboard();

    console.log("Dashboard initialisiert.");
});