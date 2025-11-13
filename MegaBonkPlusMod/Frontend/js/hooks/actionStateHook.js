import { api } from '../api/apiClient.js';
import { showToast } from '../toast/toastService.js';;

export async function getActionStates() {
    try {
        const response = await api.get('/api/actions/state');
        return response.data;
    } catch (error) {
        showToast('error', `Failed to load action states: ${error.message}`);
        return {};
    }
}