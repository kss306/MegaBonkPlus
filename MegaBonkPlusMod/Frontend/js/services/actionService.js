import { postData } from './apiService.js';

export function applyActionStates(states) {
    const killAllLoopingCheckbox = document.getElementById('kill-all-looping-checkbox');
    if (!killAllLoopingCheckbox) return;

    if (states && states.kill_all_enemies) {
        killAllLoopingCheckbox.checked = states.kill_all_enemies.looping;
    } else {
        killAllLoopingCheckbox.checked = false;
    }
}

export function handleQuickAction(actionName, payload, buttonElement) {
    console.log(`Action: ${actionName} with payload:`, payload);
    if (buttonElement) buttonElement.disabled = true;

    try {
        postData('/api/action', {
            action: actionName,
            ...payload
        });
    } catch (err) {
        console.error(`Fehler bei Aktion '${actionName}':`, err);
    } finally {
        if (buttonElement) {
            setTimeout(() => {
                buttonElement.disabled = false;
            }, 1000);
        }
    }
}

export function setupQuickActions() {
    const killAllButton = document.getElementById('action-kill-all');
    const killAllMenuButton = document.getElementById('action-kill-all-menu');
    const killAllDropdown = document.getElementById('kill-all-dropdown');
    const killAllLoopingCheckbox = document.getElementById('kill-all-looping-checkbox');
    

    if (killAllLoopingCheckbox) {
        killAllLoopingCheckbox.addEventListener('change', (event) => {
            const isLooping = event.target.checked;
            handleQuickAction("kill_all_enemies", { looping: isLooping }, null);
        });
    }

    if (killAllButton) {
        killAllButton.addEventListener('click', () => {
            const isLooping = killAllLoopingCheckbox.checked;
            handleQuickAction("kill_all_enemies", { looping: isLooping }, killAllButton);
        });
    }

    if (killAllMenuButton) {
        killAllMenuButton.addEventListener('click', (event) => {
            event.stopPropagation();
            killAllDropdown.classList.toggle('active');
        });
    }

    document.addEventListener('click', (event) => {
        if (killAllDropdown && !killAllDropdown.contains(event.target) && !killAllMenuButton.contains(event.target)) {
            killAllDropdown.classList.remove('active');
        }
    });
}