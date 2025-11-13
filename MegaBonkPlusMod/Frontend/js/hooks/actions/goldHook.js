import { api } from '../../api/apiClient.js';
import { showToast } from '../../toast/toastService.js';

export async function setGold(amount) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'edit_gold',
            changeMode: 'set',
            amount
        });
        showToast('success', response.message);
        return true;
    } catch (error) {
        showToast('error', error.message || 'Set gold failed');
        return false;
    }
}

export async function addGold(amount) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'edit_gold',
            changeMode: 'add',
            amount
        });
        showToast('success', response.message);
        return true;
    } catch (error) {
        showToast('error', error.message || 'Add gold failed');
        return false;
    }
}