import {api} from '../../api/apiClient.js';
import {showToast} from '../../toast/toastService.js';

export async function sendWeaponAction(payload) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'weapon',
            ...payload
        });
        showToast('success', response.message);
        return {ok: true, response};
    } catch (error) {
        showToast('error', error.message || 'Weapon action failed');
        return {ok: false, error};
    }
}

export async function fetchWeaponUpgradeOptions(weaponId) {
    try {
        const response = await api.post('/api/actions/execute', {
            action: 'weapon',
            mode: 'options',
            weapon: weaponId
        });

        let data;
        try {
            data = JSON.parse(response.message);
        } catch {
            data = {weapon: weaponId, allowedStats: [], error: 'Invalid options payload'};
        }

        if (data.error) {
            showToast('error', data.error);
        }

        return {ok: true, data};
    } catch (error) {
        showToast('error', error.message || 'Failed to fetch weapon upgrade options');
        return {ok: false, error};
    }
}