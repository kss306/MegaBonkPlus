import {api} from '../../api/apiClient.js';
import {showToast} from '../../toast/toastService.js';

export async function getAllWeapons() {
    try {
        const response = await api.get('/api/weapons/all');
        return response.data || [];
    } catch (error) {
        showToast('error', `Failed to load weapons: ${error.message}`);
        return [];
    }
}

export async function getWeaponInventory() {
    try {
        const response = await api.get('/api/weapons/inventory');
        return response.data?.weapons || [];
    } catch (error) {
        showToast('error', `Failed to load weapon inventory: ${error.message}`);
        return [];
    }
}