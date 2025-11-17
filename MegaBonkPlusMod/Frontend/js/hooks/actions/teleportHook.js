import {api} from '../../api/apiClient.js';
import {showToast} from '../../toast/toastService.js';

export async function teleportTo(instanceId) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'teleport',
            instanceId
        });
        showToast('success', response.message);
        return true;
    } catch (error) {
        showToast('error', error.message || 'Teleport failed');
        return false;
    }
}

export async function teleportToNearest(objectType) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'teleport_to_nearest',
            object: objectType
        });
        showToast('success', response.message);
        return true;
    } catch (error) {
        showToast('error', error.message || 'Teleport failed');
        return false;
    }
}