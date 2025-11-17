import {api} from '../../api/apiClient.js';
import {showToast} from '../../toast/toastService.js';

export async function sendTomeAction(payload) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'tome',
            ...payload
        });
        showToast('success', response.message);
        return {ok: true, response};
    } catch (error) {
        showToast('error', error.message || 'Tome action failed');
        return {ok: false, error};
    }
}