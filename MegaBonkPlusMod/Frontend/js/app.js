import { fetchData } from './services/apiService.js';
import { ENDPOINTS_TO_TRACK } from './config.js';
import { renderMinimap, updateMapIcons, setupTooltipListeners } from './services/mapService.js';
import * as TextRenderers from './services/uiService.js';

let filterStates = {};

function updateFilterState() {
    const toggles = document.querySelectorAll('.filter-list input[type="checkbox"]');
    toggles.forEach(toggle => {
        filterStates[toggle.id] = toggle.checked;
    });
}

async function updateDashboard() {

    const allKeys = Object.keys(ENDPOINTS_TO_TRACK);

    const allData = await Promise.all(
        allKeys.map(key => fetchData(ENDPOINTS_TO_TRACK[key].endpoint))
    );

    const dataMap = {};
    allKeys.forEach((key, index) => { dataMap[key] = allData[index]; });

    if (dataMap['minimap']) {
        renderMinimap(dataMap['minimap']);
    }

    allKeys.forEach(key => {
        const config = ENDPOINTS_TO_TRACK[key];
        const data = dataMap[key];

        const renderFunc = TextRenderers[config.textRenderer];

        if (renderFunc) {
            renderFunc(data);
        }
    });

    updateMapIcons(dataMap, ENDPOINTS_TO_TRACK);
}

console.log("Dashboard gestartet...");
setupTooltipListeners();
setInterval(updateDashboard, 1000);
updateDashboard();