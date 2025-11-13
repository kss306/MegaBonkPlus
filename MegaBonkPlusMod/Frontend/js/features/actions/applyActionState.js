import { executeAction } from '../../configs/actionHooksConfig.js';
import { applyBackendState } from '../items/autoRestarter.js';

export { executeAction };

export function applyActionStates(states) {
    if (!states) return;

    if (states.set_auto_restart_config) {
        applyBackendState(states.set_auto_restart_config);
    }
}