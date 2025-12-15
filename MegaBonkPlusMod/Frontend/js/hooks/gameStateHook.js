import {api} from '../api/apiClient.js';

export async function getGameState() {
    try {
        const response = await api.get('/api/game/state');
        return response.data;
    } catch (error) {
        console.error('Failed to load game state:', error);
        return {
            isInGame: false,
            currentMap: 'N/A',
            mapTier: -1,
            stageTime: 0,
            timeAlive: 0,
            bossCurses: 0
        };
    }
}

export async function setTimeScale(amount) {
    try {
        await api.post('/api/game/time-scale', { timeScale: parseFloat(amount) });
        return true;
    } catch (error) {
        console.error('Failed to set time scale', error);
        return false;
    }
}

export async function getTimeScale() {
    try {
        const response = await api.get('/api/game/time-scale');
        return response.data;
    } catch (error) {
        console.error('Failed to get time scale', error);
        return 1.0;
    }
}