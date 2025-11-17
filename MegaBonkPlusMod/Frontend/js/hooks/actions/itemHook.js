import {api} from '../../api/apiClient.js';
import {showToast} from '../../toast/toastService.js';

export async function spawnItems(items) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'spawn_items',
            items
        });
        showToast('success', response.message);
        return true;
    } catch (error) {
        1
        showToast('error', error.message || 'Spawn items failed');
        return false;
    }
}

export async function getAllItems() {
    try {
        const response = await api.get('/api/items/all');
        return response.data;
    } catch (error) {
        showToast('error', `Failed to load items: ${error.message}`);
        return [];
    }
}