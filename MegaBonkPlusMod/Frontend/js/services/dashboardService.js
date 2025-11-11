import { fetchData } from './apiService.js';
import { ENDPOINTS_TO_TRACK } from '../configs/mapConfig.js';
import { renderMinimap, updateMapIcons } from './mapService.js';
import * as TextRenderers from './uiService.js';
import {applyActionStates} from "./actionService.js";
import {getFilterState} from "./filterService.js";

export async function updateDashboard() {
    const allKeys = Object.keys(ENDPOINTS_TO_TRACK);

    const dataPromises = await Promise.all(
        allKeys.map(key => fetchData(ENDPOINTS_TO_TRACK[key].endpoint))
    );

    const actionStatePromise = fetchData('/api/actions/state');

    const allResults = await Promise.all([...dataPromises, actionStatePromise]);

    const actionStates = allResults.pop();
    const allData = allResults;

    const dataMap = {};
    allKeys.forEach((key, index) => {
        dataMap[key] = allData[index];
    });
    
    let isInGame = false;
    if (dataMap['player'] && dataMap['player'].IsInGame !== undefined) {
        isInGame = dataMap['player'].IsInGame;
        
        dataMap['player'] = dataMap['player'].PlayerDataList;
    }
    document.body.classList.toggle('game-not-running', !isInGame);

    if (dataMap['minimap']) {
        renderMinimap(dataMap['minimap']);
    }

    if (actionStates) {
        applyActionStates(actionStates);
    }

    allKeys.forEach(key => {
        const config = ENDPOINTS_TO_TRACK[key];
        const data = dataMap[key];
        const renderFunc = TextRenderers[config.textRenderer];
        if (renderFunc) {
            renderFunc(data);
        }
    });

    const filters = getFilterState();

    updateMapIcons(dataMap, ENDPOINTS_TO_TRACK, filters);
}