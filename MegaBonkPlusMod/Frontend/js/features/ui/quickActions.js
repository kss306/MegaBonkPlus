import {QUICK_ACTIONS_CONFIG} from '../../configs/quickActionConfig.js';
import {executeAction} from '../../configs/actionHooksConfig.js';
import {getKillEnemiesState} from '../../hooks/actions/enemyHook.js';
import {getPickUpAllXpState} from '../../hooks/actions/pickUpAllXpHook.js';
import {createElement, getElem, on} from '../../utils/dom.js';

export function setupQuickActions() {
    const container = getElem('quick-actions-container');
    if (!container) return;

    container.innerHTML = '';
    renderQuickActions(container);
    setupDropdownHandlers();
}

function renderQuickActions(container) {
    QUICK_ACTIONS_CONFIG.forEach(config => {
        const element = config.type === 'simple-button'
            ? createSimpleButton(config)
            : createSplitButton(config);

        if (element) {
            container.appendChild(element);
        }
    });
}

function createSimpleButton(config) {
    const button = createElement('button', {
        id: config.id,
        class: 'quick-action-button'
    }, config.label);

    on(button, 'click', () => {
        handleAction(config.actionId, config.payload || {}, button);
    });

    return button;
}

function createSplitButton(config) {
    const container = createElement('div', { class: 'split-button-container' });

    const actionButton = createElement('button', {
        id: `${config.id}-action`,
        class: 'quick-action-button split-left'
    }, config.label);

    const menuButton = createElement('button', {
        id: `${config.id}-menu`,
        class: 'quick-action-button split-right'
    }, '⋮');

    const dropdownMenu = createElement('div', {
        id: `${config.id}-dropdown`,
        class: 'dropdown-menu'
    });

    renderMenuItems(dropdownMenu, config);

    on(actionButton, 'click', () => {
        const payload = buildPayloadFromMenu(dropdownMenu);
        handleAction(config.actionId, payload, actionButton);
    });

    on(menuButton, 'click', (e) => {
        e.stopPropagation();
        toggleDropdown(dropdownMenu);
    });

    container.appendChild(actionButton);
    container.appendChild(menuButton);
    container.appendChild(dropdownMenu);

    return container;
}

function renderMenuItems(menu, config) {
    config.menuItems?.forEach(item => {
        if (item.type === 'checkbox') {
            const label = createElement('label', { class: 'dropdown-item' });

            const checkbox = createElement('input', {
                type: 'checkbox',
                id: item.id,
                data: { payloadKey: item.payloadKey }
            });

            on(checkbox, 'change', () => {
                const payload = buildPayloadFromMenu(menu);
                handleAction(config.actionId, payload);
            });

            label.appendChild(checkbox);
            label.appendChild(document.createTextNode(` ${item.label}`));
            menu.appendChild(label);
        }
    });
}

function buildPayloadFromMenu(menu) {
    const payload = {};
    const inputs = menu.querySelectorAll('input[type="checkbox"]');

    inputs.forEach(input => {
        payload[input.dataset.payloadKey] = input.checked;
    });

    return payload;
}

async function handleAction(actionId, payload, button) {
    if (button) button.disabled = true;

    try {
        await executeAction(actionId, payload);

        await syncQuickActionStateFromServer(actionId);
    } finally {
        if (button) {
            setTimeout(() => {
                button.disabled = false;
            }, 1000);
        }
    }
}

async function syncQuickActionStateFromServer(actionId) {
    try {
        if (actionId === 'kill_all_enemies') {
            const state = await getKillEnemiesState();
            const checkbox = getElem('kill_all_looping_checkbox');
            if (checkbox) {
                checkbox.checked = !!state.looping;
            }
        }

        if (actionId === 'pick_up_all_xp') {
            const state = await getPickUpAllXpState();
            const checkbox = getElem('pick_up_all_xp_looping_checkbox');
            if (checkbox) {
                checkbox.checked = !!state.looping;
            }
        }
    } catch (e) {
        console.error('Failed to sync quick action state:', e);
    }
}

function toggleDropdown(menu) {
    closeAllDropdowns(menu.id);
    menu.classList.toggle('active');
}

function closeAllDropdowns(exceptId = null) {
    document.querySelectorAll('.dropdown-menu').forEach(menu => {
        if (menu.id !== exceptId) {
            menu.classList.remove('active');
        }
    });
}

function setupDropdownHandlers() {
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.split-right') && !e.target.closest('.dropdown-menu')) {
            closeAllDropdowns();
        }
    });
}