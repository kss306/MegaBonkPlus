import {api} from '../../api/apiClient.js';
import {showToast} from '../../toast/toastService.js';

export async function getAllTomes(suppressError = false) {
    try {
        const response = await api.get('/api/tomes/all');
        return response.data || [];
    } catch (error) {
        if (!suppressError) {
            showToast('error', `Failed to load tomes: ${error.message}`);
        }
        return [];
    }
}

export async function getTomeInventory() {
    try {
        const response = await api.get('/api/tomes/inventory');
        return response.data?.tomes || [];
    } catch (error) {
        showToast('error', `Failed to load tome inventory: ${error.message}`);
        return [];
    }
}