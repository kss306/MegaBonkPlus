import {createElement, getElem, on} from '../../utils/dom.js';
import {getHotkeyConfig, updateHotkeyConfig} from '../../hooks/hotkeyHook.js';
import {ACTIONS_CONFIG} from '../../configs/actionConfig.js';

let hotkeyList = [];
let masterToggleEnabled = true;
let allItemsList = [];
let currentlyEditingHotkeyIndex = null;
let currentConfigString = '';
let isLocallyEditing = false;

let modalBackdrop, modalCloseBtn, modalSaveBtn, modalActionSelect, modalModifiersContainer;

export async function setupHotkeys(allItems) {
    allItemsList = allItems || [];

    modalBackdrop = getElem('hotkey-action-modal-backdrop');
    modalCloseBtn = getElem('hotkey-modal-close-btn');
    modalSaveBtn = getElem('hotkey-modal-save-btn');
    modalActionSelect = getElem('hotkey-modal-action-select');
    modalModifiersContainer = getElem('hotkey-modal-modifiers');

    const toggle = getElem('hotkey-master-toggle');
    if (toggle) {
        on(toggle, 'change', (e) => {
            masterToggleEnabled = e.target.checked;
            saveHotkeys();
        });
    }

    if (modalBackdrop) {
        on(modalCloseBtn, 'click', closeActionModal);
        on(modalSaveBtn, 'click', handleModalSave);
        on(modalActionSelect, 'change', () => {
            const selectedActionId = modalActionSelect.value;
            renderActionModifiers({ id: selectedActionId, payload: {} });
        });
    }

    await loadHotkeys();
}

async function loadHotkeys() {
    try {
        const config = await getHotkeyConfig();
        hotkeyList = config.hotkeys || [];
        masterToggleEnabled = config.enabled === undefined ? true : config.enabled;
        currentConfigString = JSON.stringify({ hotkeys: hotkeyList, enabled: masterToggleEnabled });
    } catch (e) {
        console.error("Error loading hotkeys:", e);
        hotkeyList = [];
        masterToggleEnabled = true;
        currentConfigString = JSON.stringify({ enabled: true, hotkeys: [] });
    }

    const toggle = getElem('hotkey-master-toggle');
    if (toggle) toggle.checked = masterToggleEnabled;
    renderUI();
}

export function syncHotkeysFromServer(config) {
    if (!config) return;

    const isModalOpen = modalBackdrop && !modalBackdrop.classList.contains('is-hidden');
    const isWaitingForKey = document.querySelector('.hotkey-input.is-waiting');
    
    if (isModalOpen || isWaitingForKey || isLocallyEditing) {
        return;
    }

    const newConfigString = JSON.stringify({
        hotkeys: config.hotkeys || [],
        enabled: config.enabled === undefined ? true : config.enabled
    });

    if (newConfigString !== currentConfigString) {
        hotkeyList = config.hotkeys || [];
        masterToggleEnabled = config.enabled === undefined ? true : config.enabled;
        currentConfigString = newConfigString;

        const toggle = getElem('hotkey-master-toggle');
        if (toggle) toggle.checked = masterToggleEnabled;

        renderUI();
    }
}

async function saveHotkeys() {
    const sanitizedHotkeys = hotkeyList
        .filter(h => h?.key && h?.action?.id)
        .map(h => ({
            key: h.key,
            action: {
                id: h.action.id,
                payload: h.action.payload ?? {}
            }
        }));

    const newConfig = {
        enabled: masterToggleEnabled,
        hotkeys: sanitizedHotkeys
    };

    const newConfigString = JSON.stringify(newConfig);
    if (newConfigString === currentConfigString) {
        return;
    }

    const wasSaved = await updateHotkeyConfig(newConfig);
    if (wasSaved) {
        currentConfigString = newConfigString;
        isLocallyEditing = false;
    }
}

function renderUI() {
    const listElement = getElem('hotkey-list');
    if (!listElement) return;
    listElement.innerHTML = '';
    hotkeyList.forEach((hotkey, index) => {
        listElement.appendChild(createHotkeyRow(hotkey, index));
    });
    listElement.appendChild(createEmptySlot());
}

function createHotkeyRow(hotkey, index) {
    const li = createElement('li', { class: 'hotkey-slot hotkey-slot-configured' });

    const keyLabel = hotkey.key ? formatKeyLabel(hotkey.key) : 'Press Key...';
    const keyInput = createElement('button', { class: 'hotkey-input' }, keyLabel);

    on(keyInput, 'click', () => {
        keyInput.textContent = 'Waiting...';
        keyInput.classList.add('is-waiting');
        isLocallyEditing = true;
    });

    on(keyInput, 'blur', () => {
        keyInput.classList.remove('is-waiting');
        keyInput.textContent = hotkey.key ? formatKeyLabel(hotkey.key) : 'Press Key...';

        if (hotkey.key && hotkey.action && hotkey.action.id) {
            isLocallyEditing = false;
        }
    });

    on(keyInput, 'keydown', (e) => {
        e.preventDefault();
        if (['Shift', 'Control', 'Alt', 'Meta'].includes(e.key)) return;

        hotkey.key = e.code;
        hotkeyList[index].key = e.code;
        keyInput.textContent = formatKeyLabel(e.code);
        keyInput.classList.remove('is-waiting');

        keyInput.blur();

        if (hotkey.action && hotkey.action.id) {
            saveHotkeys();
        }
    });

    const configButton = createElement('button', { class: 'hotkey-select' }, formatActionForButton(hotkey.action));
    on(configButton, 'click', () => openActionModal(index));

    const deleteBtn = createElement('button', { class: 'hotkey-delete-btn' }, '×');
    on(deleteBtn, 'click', () => {
        hotkeyList.splice(index, 1);
        saveHotkeys();
        renderUI();
    });

    li.appendChild(keyInput);
    li.appendChild(configButton);
    li.appendChild(deleteBtn);

    return li;
}


function formatKeyLabel(code) {
    if (!code) return '';

    if (code.startsWith('Key') && code.length > 3) {
        return code.substring(3);
    }

    if (code.startsWith('Digit') && code.length > 5) {
        return code.substring(5);
    }

    if (code.startsWith('Numpad') && code.length > 6) {
        return 'Num' + code.substring(6);
    }

    if (code === 'ArrowUp') return '↑';
    if (code === 'ArrowDown') return '↓';
    if (code === 'ArrowLeft') return '←';
    if (code === 'ArrowRight') return '→';

    if (code === 'Space') return 'Space';

    if (/^F\d+$/.test(code)) return code;

    return code;
}

function createEmptySlot() {
    const li = createElement('li', { class: 'hotkey-slot hotkey-slot-empty' });
    const span = createElement('span', {}, '+ New Hotkey');
    li.appendChild(span);
    on(li, 'click', () => {
        isLocallyEditing = true;
        hotkeyList.push({ key: null, action: null });
        renderUI();
    });
    return li;
}

function formatActionForButton(action) {
    if (!action || !action.id) return 'Set Action...';

    const config = ACTIONS_CONFIG.find(a => a.id === action.id);
    if (!config) return 'Set Action...';

    const payload = action.payload || {};
    let label = config.name;
    let details = '';

    if ((action.id === 'kill_all_enemies' || action.id === 'pick_up_all_xp') && payload.mode === 'toggle') {
        label = `${config.name} | Toggle`;
    }

    if (action.id === 'spawn_items' && payload.itemId) {
        const item = allItemsList.find(i => i.id === payload.itemId);
        const itemName = item ? item.name : payload.itemId;
        const qty = payload.quantity || 1;
        details = `${itemName} x${qty}`;
        label = `${config.name}: ${details}`;
        return label;
    }

    if (action.id === 'teleport_to_nearest' && payload.object) {
        const mod = config.modifiers.find(m => m.payloadKey === 'object');
        const option = mod?.options?.find(o => o.value === payload.object);
        const targetName = option ? option.name : payload.object;
        label = `${config.name} ${targetName}`;
        return label;
    }
    
    if (action.id === 'add_levels' && typeof payload.amount === 'number') {
        label = `${config.name}: x${payload.amount}`;
        return label;
    }

    if (action.id === 'edit_gold' && typeof payload.amount === 'number') {
        const mode = payload.changeMode === 'set' ? 'Set' : 'Add';
        label = `${config.name}: ${mode} ${payload.amount}`;
        return label;
    }
    
    return label;
}

function openActionModal(index) {
    isLocallyEditing = true;
    currentlyEditingHotkeyIndex = index;
    const hotkey = hotkeyList[index];
    const currentAction = hotkey.action || { id: null, payload: {} };

    modalActionSelect.innerHTML = '<option value="">Choose action...</option>';
    ACTIONS_CONFIG.forEach(action => {
        const option = createElement('option', { value: action.id }, action.name);
        if (currentAction.id === action.id) option.selected = true;
        modalActionSelect.appendChild(option);
    });

    renderActionModifiers(currentAction);
    modalBackdrop.classList.remove('is-hidden');
}

function closeActionModal() {
    modalBackdrop.classList.add('is-hidden');
    currentlyEditingHotkeyIndex = null;
    modalModifiersContainer.innerHTML = '';
    
    const hasIncompleteHotkey = hotkeyList.some(h => !h.key || !(h.action && h.action.id));
    if (!hasIncompleteHotkey) {
        isLocallyEditing = false;
    }
}

function renderActionModifiers(action) {
    modalModifiersContainer.innerHTML = '';
    if (!action || !action.id) {
        modalModifiersContainer.textContent = 'Select an action to configure its modifiers';
        return;
    }

    const config = ACTIONS_CONFIG.find(a => a.id === action.id);
    if (!config) {
        modalModifiersContainer.textContent = 'Error: action not found';
        return;
    }

    if (config.modifiers.length === 0) {
        modalModifiersContainer.textContent = 'This action has no modifiers';
        return;
    }

    config.modifiers.forEach(mod => {
        const group = createElement('div', { class: 'hotkey-modal-group' });
        const label = createElement('label', { for: `modifier-input-${mod.id}` }, mod.name);

        const currentValue = (action.payload && action.payload[mod.payloadKey] !== undefined)
            ? action.payload[mod.payloadKey]
            : mod.defaultValue;

        let inputElement;

        if (mod.type === 'select') {
            inputElement = createElement('select', { class: 'modifier-input', id: `modifier-input-${mod.id}` });
            mod.options.forEach(opt => {
                const option = createElement('option', { value: opt.value }, opt.name);
                if (opt.value === currentValue) option.selected = true;
                inputElement.appendChild(option);
            });
        } else if (mod.type === 'item-select') {
            inputElement = createElement('select', { class: 'modifier-input', id: `modifier-input-${mod.id}` });
            const defaultOpt = createElement('option', { value: '' }, 'Select item...');
            inputElement.appendChild(defaultOpt);

            allItemsList.forEach(item => {
                const opt = createElement('option', { value: item.id }, item.name);
                if (item.id === currentValue) opt.selected = true;
                inputElement.appendChild(opt);
            });
        } else {
            inputElement = createElement('input', {
                class: 'modifier-input',
                type: mod.type || 'text',
                id: `modifier-input-${mod.id}`,
                value: currentValue || ''
            });

            if (mod.min !== undefined) inputElement.min = mod.min;
            if (mod.max !== undefined) inputElement.max = mod.max;
        }

        inputElement.dataset.payloadKey = mod.payloadKey;

        group.appendChild(label);
        group.appendChild(inputElement);
        modalModifiersContainer.appendChild(group);
    });
}

function handleModalSave() {
    if (currentlyEditingHotkeyIndex === null) return;

    const actionId = modalActionSelect.value;
    if (!actionId) {
        hotkeyList[currentlyEditingHotkeyIndex].action = null;
    } else {
        const config = ACTIONS_CONFIG.find(a => a.id === actionId);
        const newPayload = {};
        const inputs = modalModifiersContainer.querySelectorAll('.modifier-input');

        inputs.forEach(input => {
            const modKey = input.dataset.payloadKey;
            if (!modKey) return;

            const mod = config.modifiers.find(m => m.payloadKey === modKey);
            let value = input.value;

            if (input.type === 'number') {
                value = parseInt(value, 10) || 0;
                if (mod) {
                    if (mod.min !== undefined) value = Math.max(mod.min, value);
                    if (mod.max !== undefined) value = Math.min(mod.max, value);
                }
            }
            newPayload[modKey] = value;
        });

        hotkeyList[currentlyEditingHotkeyIndex].action = {
            id: actionId,
            payload: newPayload
        };
    }

    saveHotkeys();
    renderUI();
    closeActionModal();
}