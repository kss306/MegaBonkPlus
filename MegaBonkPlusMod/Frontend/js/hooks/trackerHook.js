import {api} from '../api/apiClient.js';

export async function getAllTrackerData() {
    try {
        const response = await api.get('/api/trackers/all');
        return response.data;
    } catch (error) {
        console.error('Failed to load tracker data:', error);
        return null;
    }
}