import { postData } from './apiService.js';
import { applyBackendState } from "./autoRestarterService.js";
import { QUICK_ACTIONS_CONFIG } from '../configs/quickActionConfig.js';

export function applyActionStates(states) {
    if (!states) return;

    if (states.set_auto_restart_config) {
        applyBackendState(states.set_auto_restart_config);
    }

    QUICK_ACTIONS_CONFIG.forEach(qaConfig => {
        const actionState = states[qaConfig.actionId];
        if (!actionState || !qaConfig.menuItems) return;

        qaConfig.menuItems.forEach(menuItem => {
            const stateValue = actionState[menuItem.payloadKey];
            if (stateValue === undefined) return;

            const element = document.getElementById(menuItem.id);
            if (!element) return;

            if (menuItem.type === 'checkbox' && element.type === 'checkbox') {
                element.checked = stateValue;
            }
        });
    });
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
        console.error(`Error on action '${actionName}':`, err);
    } finally {
        if (buttonElement) {
            setTimeout(() => {
                buttonElement.disabled = false;
            }, 1000);
        }
    }
}

export function setupQuickActions() {
    const container = document.getElementById('quick-actions-container');
    if (!container) return;

    container.innerHTML = '';

    document.addEventListener('click', (event) => {
        handleGlobalClickToCloseDropdowns(event);
    });

    QUICK_ACTIONS_CONFIG.forEach(config => {
        let element;
        if (config.type === 'simple-button') {
            element = renderSimpleButton(config);
        } else if (config.type === 'split-button') {
            element = renderSplitButton(config);
        }

        if (element) {
            container.appendChild(element);
        }
    });
}

function renderSimpleButton(config) {
    const button = document.createElement('button');
    button.id = config.id;
    button.className = 'quick-action-button';
    button.textContent = config.label;

    button.addEventListener('click', () => {
        handleQuickAction(config.actionId, config.payload || {}, button);
    });

    return button;
}

function renderSplitButton(config) {
    const container = document.createElement('div');
    container.className = 'split-button-container';

    const actionButton = document.createElement('button');
    actionButton.id = config.id + '-action';
    actionButton.className = 'quick-action-button split-left';
    actionButton.textContent = config.label;

    const menuButton = document.createElement('button');
    menuButton.id = config.id + '-menu';
    menuButton.className = 'quick-action-button split-right';
    menuButton.innerHTML = '&#x22EE;';

    const dropdownMenu = document.createElement('div');
    dropdownMenu.id = config.id + '-dropdown';
    dropdownMenu.className = 'dropdown-menu';

    config.menuItems.forEach(itemConfig => {
        if (itemConfig.type === 'checkbox') {
            const label = document.createElement('label');
            label.className = 'dropdown-item';

            const checkbox = document.createElement('input');
            checkbox.type = 'checkbox';
            checkbox.id = itemConfig.id;
            checkbox.dataset.payloadKey = itemConfig.payloadKey;

            // Event-Listener für Status-Änderungen (z.B. Looping an/aus)
            checkbox.addEventListener('change', () => {
                const payload = buildPayloadFromControls(dropdownMenu);
                handleQuickAction(config.actionId, payload, null);
            });

            label.appendChild(checkbox);
            label.appendChild(document.createTextNode(` ${itemConfig.label}`));
            dropdownMenu.appendChild(label);
        }
    });

    actionButton.addEventListener('click', () => {
        const payload = buildPayloadFromControls(dropdownMenu);
        handleQuickAction(config.actionId, payload, actionButton);
    });

    menuButton.addEventListener('click', (event) => {
        event.stopPropagation();
        closeAllDropdowns(dropdownMenu.id);
        dropdownMenu.classList.toggle('active');
    });

    container.appendChild(actionButton);
    container.appendChild(menuButton);
    container.appendChild(dropdownMenu);

    return container;
}

function buildPayloadFromControls(dropdownMenu) {
    const payload = {};
    const inputs = dropdownMenu.querySelectorAll('input');

    inputs.forEach(input => {
        if (input.type === 'checkbox') {
            payload[input.dataset.payloadKey] = input.checked;
        }
    });

    return payload;
}

function closeAllDropdowns(exceptId = null) {
    const allDropdowns = document.querySelectorAll('.dropdown-menu');
    allDropdowns.forEach(menu => {
        if (menu.id !== exceptId) {
            menu.classList.remove('active');
        }
    });
}

function handleGlobalClickToCloseDropdowns(event) {
    const target = event.target;

    const isMenuButton = target.closest('.split-right');
    const isInsideDropdown = target.closest('.dropdown-menu');

    if (!isMenuButton && !isInsideDropdown) {
        closeAllDropdowns();
    }
}