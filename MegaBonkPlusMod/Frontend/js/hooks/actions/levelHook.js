import {api} from '../../api/apiClient.js';
import {showToast} from '../../toast/toastService.js';

export async function addLevels(amount) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'add_levels',
            amount
        });
        showToast('success', response.message);
        return true;
    } catch (error) {
        showToast('error', error.message || 'Add levels failed');
        return false;
    }
}