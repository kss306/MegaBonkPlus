import {getElem, on} from '../../utils/dom.js';
import {setAutoRestart} from '../../hooks/actions/autoRestartHook.js';
import {openItemModal} from '../ui/modalService.js';

let minEmptySlots = 0;
let selectedItems = [];
let masterToggleState = false;
let itemListContainer = null;
let masterToggle = null;
let allItems = [];

export async function setupAutoRestarter(items) {
    allItems = items;
    itemListContainer = getElem('restarter-item-list');
    masterToggle = getElem('restarter-master-toggle');

    if (!itemListContainer || !masterToggle) {
        console.error("AutoRestarter: Missing DOM elements");
        return;
    }

    calculateMinEmptySlots();

    on(itemListContainer, 'click', '.item-slot-empty', () => {
        openItemModal(
            (itemId) => selectItem(itemId),
            () => selectedItems
        );
    });

    on(itemListContainer, 'click', '.remove-item-btn', (e) => {
        const index = parseInt(e.target.dataset.index, 10);
        removeItem(index);
    });

    on(masterToggle, 'change', () => {
        masterToggleState = masterToggle.checked;
        sendStateToBackend();
    });

    renderItemSlots();
}

function calculateMinEmptySlots() {
    try {
        const containerWidth = itemListContainer.clientWidth;
        const slotWidth = 70;
        const slotGap = 16;
        minEmptySlots = Math.floor((containerWidth + 1 + slotGap) / (slotWidth + slotGap));
        if (minEmptySlots < 1) minEmptySlots = 1;
    } catch (e) {
        console.warn("Could not calculate slots, using default:", e);
        minEmptySlots = 5;
    }
}

function renderItemSlots() {
    if (!itemListContainer) return;

    const totalFilled = selectedItems.length;
    const numEmptySlots = Math.max(1, minEmptySlots - totalFilled);

    let html = '';

    selectedItems.forEach((itemId, index) => {
        const item = allItems.find(a => a.id === itemId);
        const itemName = item ? item.name : 'Unknown Item';

        html += `
            <div class="restarter-item-slot item-slot-filled" data-tooltip="${itemName}">
                <img src="images/items/${item.id}.png" alt="${itemName}">
                <button class="remove-item-btn" data-index="${index}">&times;</button>
            </div>
        `;
    });

    for (let i = 0; i < numEmptySlots; i++) {
        html += '<div class="restarter-item-slot item-slot-empty"><span>+</span></div>';
    }

    itemListContainer.innerHTML = html;
}

function selectItem(itemId) {
    if (!selectedItems.includes(itemId)) {
        selectedItems.push(itemId);
        renderItemSlots();
        sendStateToBackend();
    }
}

function removeItem(index) {
    if (index >= 0 && index < selectedItems.length) {
        selectedItems.splice(index, 1);
        renderItemSlots();
        sendStateToBackend();
    }
}

async function sendStateToBackend() {
    await setAutoRestart(masterToggleState, selectedItems);
}

export function applyBackendState(state) {
    if (!state || !masterToggle) return;

    const newToggleState = state.enabled === true;
    if (masterToggleState !== newToggleState) {
        masterToggleState = newToggleState;
        masterToggle.checked = newToggleState;
    }

    const newItemList = state.itemIds || [];
    if (JSON.stringify(selectedItems) !== JSON.stringify(newItemList)) {
        selectedItems = newItemList;
        renderItemSlots();
    }
}