import {api} from '../api/apiClient.js';
import {showToast} from '../toast/toastService.js';

export async function getHotkeyConfig() {
    try {
        const response = await api.get('/api/hotkeys');
        return response.data;
    } catch (error) {
        showToast('error', `Failed to load hotkeys: ${error.message}`);
        return { enabled: true, hotkeys: [] };
    }
}

export async function updateHotkeyConfig(config) {
    try {
        const response = await api.post('/api/hotkeys', config);
        showToast('success', response.message);
        return true;
    } catch (error) {
        showToast('error', `Failed to save hotkeys: ${error.message}`);
        return false;
    }
}