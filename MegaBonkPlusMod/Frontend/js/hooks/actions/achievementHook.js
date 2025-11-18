import {api} from "../../api/apiClient.js";
import {showToast} from "../../toast/toastService.js";

export async function unlockAll(payload = {}) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'unlock_all',
            payload
        });
        showToast('success', response.message);
        return true;
    } catch (error) {
        showToast('error', error.message || 'Failed to unlock achievements');
        return false;
    }
}