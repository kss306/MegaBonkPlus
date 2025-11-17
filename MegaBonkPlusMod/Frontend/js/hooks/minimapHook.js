import {api} from '../api/apiClient.js';

export async function getMinimapData() {
    try {
        const response = await api.get('/api/minimap/stream');
        return response.data;
    } catch (error) {
        console.error('Failed to load minimap data:', error);
        return null;
    }
}