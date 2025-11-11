import { updateDashboard } from './services/dashboardService.js';
import { setupTooltipListeners } from './services/mapService.js';
import { setupQuickActions } from './services/actionService.js';
import { initializeFilters } from './services/filterService.js';
import {setupAutoRestarter} from "./services/autoRestarterService.js";

console.log("Dashboard wird initialisiert...");

setupQuickActions();
setupTooltipListeners();
initializeFilters(updateDashboard);
setInterval(updateDashboard, 1000);
updateDashboard();
setupAutoRestarter();

console.log("Dashboard initialisiert.");