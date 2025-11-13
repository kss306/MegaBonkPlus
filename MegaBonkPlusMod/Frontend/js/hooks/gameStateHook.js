import { api } from '../api/apiClient.js';

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