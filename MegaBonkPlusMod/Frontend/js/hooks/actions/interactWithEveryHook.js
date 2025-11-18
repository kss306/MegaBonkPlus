import {api} from "../../api/apiClient.js";
import {showToast} from "../../toast/toastService.js";

export async function interactWithEvery(payload) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'interact_with_every',
            ...payload
        });
        showToast('success', response.message);
        return true;
    } catch (error) {
        showToast('error', error.message || 'Failed to interact');
        return false;
    }
}