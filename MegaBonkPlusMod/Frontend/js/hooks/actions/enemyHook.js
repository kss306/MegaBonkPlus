import {api} from '../../api/apiClient.js';
import {showToast} from '../../toast/toastService.js';

export async function killAllEnemies(payload = {}) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'kill_all_enemies',
            ...payload
        });
        showToast('success', response.message);
        return true;
    } catch (error) {
        showToast('error', error.message || 'Kill enemies failed');
        return false;
    }
}

export async function getKillEnemiesState() {
    try {
        const response = await api.get('/api/actions/state');
        return response.data?.kill_all_enemies || {looping: false};
    } catch (error) {
        showToast('error', `Failed to load enemy state: ${error.message}`);
        return {looping: false};
    }
}