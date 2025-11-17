import {ENDPOINTS_TO_TRACK} from '../configs/mapConfig.js';
import {renderMinimap} from '../features/map/mapRenderer.js';
import {updateMapIcons} from '../features/map/mapIcons.js';
import {clearPlayerStats, renderPlayerStats} from '../features/ui/playerStats.js';
import {getFilterState} from '../features/ui/filters.js';
import {applyActionStates} from '../features/actions/applyActionState.js';
import {syncHotkeysFromServer} from '../features/hotkeys/hotkeyManager.js';
import {getAllTrackerData} from '../hooks/trackerHook.js';
import {getMinimapData} from '../hooks/minimapHook.js';
import {getActionStates} from '../hooks/actionStateHook.js';
import {getHotkeyConfig} from '../hooks/hotkeyHook.js';
import {getGameState} from '../hooks/gameStateHook.js';
import {createEmptyData} from '../utils/data.js';
import {getElem, toggleClass} from '../utils/dom.js';
import {getWeaponInventory} from '../hooks/actions/weaponDataHook.js';
import {getTomeInventory} from '../hooks/actions/tomeDataHook.js';
import {applyTomeInventory, applyWeaponInventory} from '../features/inventory/weaponActions.js';

const UPDATE_INTERVAL = 1000;

export function setupDashboard() {
    updateDashboard();
    setInterval(updateDashboard, UPDATE_INTERVAL);
}

async function updateDashboard() {
    try {
        const gameState = await getGameState();
        updateGameStateUI(gameState.isInGame);

        const [actionStates, hotkeyConfig] = await Promise.all([
            getActionStates(),
            getHotkeyConfig()
        ]);

        if (actionStates) applyActionStates(actionStates);
        if (hotkeyConfig) syncHotkeysFromServer(hotkeyConfig);

        if (!gameState.isInGame) {
            clearGameUI();
            applyWeaponInventory([]);
            applyTomeInventory([]);
            return;
        }

        await updateGameData();
    } catch (error) {
        console.error('Dashboard update error:', error);
    }
}

function updateGameStateUI(isInGame) {
    toggleClass(document.body, 'game-not-running', !isInGame);
}

async function updateGameData() {
    const [allTrackerData, minimapData, weaponInventory, tomeInventory] = await Promise.all([
        getAllTrackerData(),
        getMinimapData(),
        getWeaponInventory(),
        getTomeInventory()
    ]);

    if (!allTrackerData) {
        clearGameUI();
        applyWeaponInventory([]);
        applyTomeInventory([]);
        return;
    }

    const dataMap = buildDataMap(allTrackerData, minimapData);

    renderGameUI(dataMap);
    applyWeaponInventory(weaponInventory || []);
    applyTomeInventory(tomeInventory || []);
}

function buildDataMap(trackerData, minimapData) {
    const dataMap = {
        ...trackerData,
        minimap: minimapData
    };

    Object.keys(ENDPOINTS_TO_TRACK).forEach(key => {
        if (key !== 'minimap' && !dataMap[key]) {
            dataMap[key] = createEmptyData();
        }
    });

    return dataMap;
}

function renderGameUI(dataMap) {
    if (dataMap.minimap) {
        renderMinimap(dataMap.minimap);
    }

    renderPlayerStats(dataMap.player);

    const filters = getFilterState();
    updateMapIcons(dataMap, ENDPOINTS_TO_TRACK, filters);
}

function clearGameUI() {
    clearPlayerStats();
    clearMinimapContainer();
}

function clearMinimapContainer() {
    const minimapContainer = getElem('minimap-container');
    if (minimapContainer) {
        minimapContainer.innerHTML = '';
    }
}