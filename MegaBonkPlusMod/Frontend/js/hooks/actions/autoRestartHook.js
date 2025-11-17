import {api} from '../../api/apiClient.js';
import {showToast} from '../../toast/toastService.js';

export async function setAutoRestart(enabled, itemIds = []) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'set_auto_restart_config',
            enabled,
            itemIds
        });
        showToast('success', response.message);
        return true;
    } catch (error) {
        showToast('error', error.message || 'Auto-restart configuration failed');
        return false;
    }
}

export async function getAutoRestartState() {
    try {
        const response = await api.get('/api/actions/state');
        return response.data?.set_auto_restart_config || {enabled: false, itemIds: []};
    } catch (error) {
        showToast('error', `Failed to load auto-restart state: ${error.message}`);
        return {enabled: false, itemIds: []};
    }
}