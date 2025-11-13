import { api } from '../../api/apiClient.js';
import { showToast } from '../../toast/toastService.js';

export async function interact(instanceId) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'interact',
            instanceId
        });
        showToast('success', response.message);
        return true;
    } catch (error) {
        showToast('error', error.message || 'Interaction failed');
        return false;
    }
}