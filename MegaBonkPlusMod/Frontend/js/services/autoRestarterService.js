import {postData} from './apiService.js';
import {openItemModal} from "./modalService.js";


let minEmptySlots = 0;
let selectedItems = [];
let masterToggleState = false;

let itemListContainer = null;
let masterToggle = null;

let allItems = [];


export async function setupAutoRestarter(items) {
    allItems = items;

    itemListContainer = document.getElementById('restarter-item-list');
    masterToggle = document.getElementById('restarter-master-toggle');


    if (!itemListContainer || !masterToggle) {
        console.error("AutoRestarter konnte nicht alle DOM-Elemente finden.");
        return;
    }

    try {
        const containerWidth = itemListContainer.clientWidth;
        const slotWidth = 70;
        const slotGap = 16;
        minEmptySlots = Math.floor((containerWidth + 1 + slotGap) / (slotWidth + slotGap));
        if (minEmptySlots < 1) minEmptySlots = 1;
    } catch (e) {
        console.warn("Konnte Mindest-Slots nicht berechnen, setze auf 5:", e);
        minEmptySlots = 5;
    }

    itemListContainer.addEventListener('click', (event) => {
        const slot = event.target.closest('.restarter-item-slot');
        if (!slot) return;

        if (slot.classList.contains('item-slot-empty')) {
            openItemModal(
                (itemId) => selectItem(itemId),
                () => selectedItems
            );
        } else if (event.target.classList.contains('remove-item-btn')) {
            const index = parseInt(event.target.dataset.index, 10);
            removeItem(index);
        }
    });

    masterToggle.addEventListener('change', () => {
        masterToggleState = masterToggle.checked;
        sendStateToBackend();
    });

    renderItemSlots();
}

function renderItemSlots() {
    if (!itemListContainer) return;

    const totalFilled = selectedItems.length;
    const numEmptySlots = Math.max(1, minEmptySlots - totalFilled);

    let html = '';

    selectedItems.forEach((itemId, index) => {
        const item = allItems.find(a => a.id === itemId);
        const itemName = item ? item.name : 'Unbekanntes Item';

        html += `
            <div class="restarter-item-slot item-slot-filled" data-tooltip="${itemName}">
                <img src="images/items/${item.id}.png" alt="${itemName}">
                <button class="remove-item-btn" data-index="${index}">&times;</button>
            </div>
        `;
    });

    for (let i = 0; i < numEmptySlots; i++) {
        html += `
            <div class="restarter-item-slot item-slot-empty">
                <span>+</span>
            </div>
        `;
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

function sendStateToBackend() {
    postData('/api/action', {
        action: 'set_auto_restart_config',
        enabled: masterToggleState,
        itemIds: selectedItems
    });
}

export function applyBackendState(state) {
    if (!state || !masterToggle) {
        return;
    }

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