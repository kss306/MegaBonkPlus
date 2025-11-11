import { postData } from './apiService.js';
import {ACTIONS_CONFIG} from "../configs/actionConfig.js";

const LOCAL_STORAGE_KEY = 'megaBonkPlusHotkeys';

let hotkeyList = [];
let masterToggleEnabled = true;
let allItemsList = [];
let currentlyEditingHotkeyIndex = null;

let modalBackdrop;
let modalCloseBtn;
let modalSaveBtn;
let modalActionSelect;
let modalModifiersContainer;

export function setupHotkeys(allItems) {
    allItemsList = allItems || [];

    modalBackdrop = document.getElementById('hotkey-action-modal-backdrop');
    modalCloseBtn = document.getElementById('hotkey-modal-close-btn');
    modalSaveBtn = document.getElementById('hotkey-modal-save-btn');
    modalActionSelect = document.getElementById('hotkey-modal-action-select');
    modalModifiersContainer = document.getElementById('hotkey-modal-modifiers');

    const toggle = document.getElementById('hotkey-master-toggle');
    if (toggle) {
        toggle.addEventListener('change', (e) => {
            masterToggleEnabled = e.target.checked;
            saveHotkeys();
        });
    }

    if (modalBackdrop) {
        modalCloseBtn.addEventListener('click', closeActionModal);
        modalSaveBtn.addEventListener('click', handleModalSave);
        modalActionSelect.addEventListener('change', () => {
            const selectedActionId = modalActionSelect.value;
            renderActionModifiers({ id: selectedActionId, payload: {} });
        });
    }
    loadHotkeys();
}

function loadHotkeys() {
    const storedData = localStorage.getItem(LOCAL_STORAGE_KEY);
    if (storedData) {
        try {
            const data = JSON.parse(storedData);
            hotkeyList = data.hotkeys || [];
            masterToggleEnabled = data.enabled === undefined ? true : data.enabled;

            const toggle = document.getElementById('hotkey-master-toggle');
            if (toggle) toggle.checked = masterToggleEnabled;
        } catch (e) {
            hotkeyList = [];
        }
    }
    renderUI();
    syncHotkeysToBackend();
}

function saveHotkeys() {
    const dataToStore = {
        enabled: masterToggleEnabled,
        hotkeys: hotkeyList
    };
    localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(dataToStore));
    syncHotkeysToBackend();
}

function syncHotkeysToBackend() {
    const config = {
        enabled: masterToggleEnabled,
        hotkeys: hotkeyList
    };

    try {
        postData('/api/action', {
            action: 'set_hotkey_config',
            ...config
        });
    } catch (err) {
        console.error("Error syncing Hotkeys:", err);
    }
}

function renderUI() {
    const listElement = document.getElementById('hotkey-list');
    if (!listElement) return;
    listElement.innerHTML = '';
    hotkeyList.forEach((hotkey, index) => {
        listElement.appendChild(createHotkeyRow(hotkey, index));
    });
    listElement.appendChild(createEmptySlot());
}

function createHotkeyRow(hotkey, index) {
    const li = document.createElement('li');
    li.className = 'hotkey-slot hotkey-slot-configured';

    const keyInput = document.createElement('button');
    keyInput.className = 'hotkey-input';
    keyInput.textContent = hotkey.key || 'Press Key...';

    keyInput.addEventListener('click', () => {
        keyInput.textContent = 'Waiting...';
        keyInput.classList.add('is-waiting');
    });

    keyInput.addEventListener('blur', () => {
        keyInput.classList.remove('is-waiting');
        keyInput.textContent = hotkey.key || 'Press Key...';
    });

    keyInput.addEventListener('keydown', (e) => {
        e.preventDefault();
        
        const keyCode = e.code;
        if (['Shift', 'Control', 'Alt', 'Meta'].includes(e.key)) return;

        hotkey.key = keyCode;
        hotkeyList[index].key = keyCode;
        keyInput.textContent = keyCode;
        keyInput.classList.remove('is-waiting');
        keyInput.blur();
        saveHotkeys();
    });

    const configButton = document.createElement('button');
    configButton.className = 'hotkey-select';
    configButton.textContent = formatActionForButton(hotkey.action);
    configButton.addEventListener('click', () => {
        openActionModal(index);
    });

    const deleteBtn = document.createElement('button');
    deleteBtn.className = 'hotkey-delete-btn';
    deleteBtn.innerHTML = '&times;';
    deleteBtn.addEventListener('click', () => {
        hotkeyList.splice(index, 1);
        saveHotkeys();
        renderUI();
    });

    li.appendChild(keyInput);
    li.appendChild(configButton);
    li.appendChild(deleteBtn);

    return li;
}

function createEmptySlot() {
    const li = document.createElement('li');
    li.className = 'hotkey-slot hotkey-slot-empty';
    const span = document.createElement('span');
    span.textContent = '+ New Hotkey';
    li.appendChild(span);
    li.addEventListener('click', () => {
        hotkeyList.push({ key: null, action: null });
        renderUI();
    });
    return li;
}

function formatActionForButton(action) {
    if (!action || !action.id) {
        return 'Set Action...';
    }
    const config = ACTIONS_CONFIG.find(a => a.id === action.id);
    if (!config) {
        return 'Set Action...';
    }
    let details = '';
    if (action.id === 'give_gold' && action.payload?.amount) {
        details = `(${action.payload.amount})`;
    } else if (action.id === 'spawn_item' && action.payload?.itemId) {
        const item = allItemsList.find(i => i.id === action.payload.itemId);
        const itemName = item ? item.name : action.payload.itemId;
        const qty = action.payload.quantity || 1;
        details = `(${itemName} x${qty})`;
    }
    return `${config.name} ${details}`;
}

function openActionModal(index) {
    currentlyEditingHotkeyIndex = index;
    const hotkey = hotkeyList[index];
    const currentAction = hotkey.action || { id: null, payload: {} };

    modalActionSelect.innerHTML = '<option value="">Chose action...</option>';
    ACTIONS_CONFIG.forEach(action => {
        const option = document.createElement('option');
        option.value = action.id;
        option.textContent = action.name;
        if (currentAction.id === action.id) {
            option.selected = true;
        }
        modalActionSelect.appendChild(option);
    });

    renderActionModifiers(currentAction);
    modalBackdrop.classList.remove('is-hidden');
}

function closeActionModal() {
    modalBackdrop.classList.add('is-hidden');
    currentlyEditingHotkeyIndex = null;
    modalModifiersContainer.innerHTML = '';
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
        const group = document.createElement('div');
        group.className = 'hotkey-modal-group';

        const label = document.createElement('label');
        label.textContent = mod.name;
        label.htmlFor = `modifier-input-${mod.id}`;

        const currentValue = (action.payload && action.payload[mod.payloadKey] !== undefined)
            ? action.payload[mod.payloadKey]
            : mod.defaultValue;

        let inputElement;
        
        if (mod.type === 'select') {
            inputElement = document.createElement('select');
            inputElement.className = 'modifier-input';

            mod.options.forEach(opt => {
                const option = document.createElement('option');
                option.value = opt.value;
                option.textContent = opt.name;
                if (opt.value === currentValue) {
                    option.selected = true;
                }
                inputElement.appendChild(option);
            });
        }

        else if (mod.type === 'item-select') {
            inputElement = document.createElement('select');
            inputElement.className = 'modifier-input';
            const defaultOpt = document.createElement('option');
            defaultOpt.value = '';
            defaultOpt.textContent = 'Select item...';
            inputElement.appendChild(defaultOpt);

            allItemsList.forEach(item => {
                const opt = document.createElement('option');
                opt.value = item.id;
                opt.textContent = item.name;
                if (item.id === currentValue) {
                    opt.selected = true;
                }
                inputElement.appendChild(opt);
            });
        }
        else {
            inputElement = document.createElement('input');
            inputElement.className = 'modifier-input';
            inputElement.type = mod.type || 'text';
            inputElement.value = currentValue || '';

            if (mod.min !== undefined) {
                inputElement.min = mod.min;
            }
            if (mod.max !== undefined) {
                inputElement.max = mod.max;
            }
        }

        inputElement.id = `modifier-input-${mod.id}`;
        
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
                    if (mod.min !== undefined) {
                        value = Math.max(mod.min, value);
                    }
                    if (mod.max !== undefined) {
                        value = Math.min(mod.max, value);
                    }
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