import {api} from "../../api/apiClient.js";
import {showToast} from "../../toast/toastService.js";

export async function pickUpAllXp(payload = {}) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'pick_up_all_xp',
            ...payload
        });
        showToast('success', response.message);
        return true;
    } catch (error) {
        showToast('error', error.message || 'Pick up XP failed');
        return false;
    }
}

export async function getPickUpAllXpState() {
    try {
        const response = await api.get('/api/actions/state');
        return response.data?.pick_up_all_xp || {looping: false};
    } catch (error) {
        showToast('error', `Failed to load enemy state: ${error.message}`);
        return {looping: false};
    }
}