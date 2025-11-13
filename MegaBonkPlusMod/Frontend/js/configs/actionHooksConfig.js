import { interact } from '../hooks/actions/interactHook.js';
import { teleportTo, teleportToNearest } from '../hooks/actions/teleportHook.js';
import { killAllEnemies } from '../hooks/actions/enemyHook.js';
import { addLevels } from '../hooks/actions/levelHook.js';
import { setGold, addGold } from '../hooks/actions/goldHook.js';
import { spawnItems } from '../hooks/actions/itemHook.js';

export const ACTION_HOOKS = {
    'interact': (payload) => interact(payload.instanceId),
    'teleport': (payload) => teleportTo(payload.instanceId),
    'teleport_to_nearest': (payload) => teleportToNearest(payload.object),
    'kill_all_enemies': (payload) => killAllEnemies(payload.looping || false),
    'add_levels': (payload) => addLevels(payload.amount),
    'edit_gold': (payload) => {
        if (payload.changeMode === 'set') {
            return setGold(payload.amount);
        } else if (payload.changeMode === 'add') {
            return addGold(payload.amount);
        }
    },
    'spawn_items': (payload) => spawnItems(payload.items)
};

export async function executeAction(actionName, payload) {
    const actionHook = ACTION_HOOKS[actionName];

    if (!actionHook) {
        console.warn(`Unknown action: ${actionName}`);
        return false;
    }

    try {
        await actionHook(payload);
        return true;
    } catch (error) {
        console.error(`Error executing action '${actionName}':`, error);
        return false;
    }
}