import {getElem, on} from '../../utils/dom.js';
import {openItemModal} from '../ui/modalService.js';
import {spawnItems} from '../../hooks/actions/itemHook.js';

let selectedItems = [];
let itemListContainer = null;
let spawnButton = null;
let allItems = [];

export function setupItemSpawner(items) {
    allItems = items;
    itemListContainer = getElem('item-spawner-list');
    spawnButton = getElem('spawn-items-btn');

    if (!itemListContainer || !spawnButton) {
        console.error("ItemSpawner: Missing DOM elements");
        return;
    }

    on(itemListContainer, 'click', '.restarter-item-slot.item-slot-empty', () => {
        openItemModal(
            (itemId) => selectItem(itemId),
            () => selectedItems.map(i => i.id)
        );
    });

    on(itemListContainer, 'click', '.remove-item-btn', (e) => {
        const itemId = e.target.dataset.id;
        removeItem(itemId);
    });

    on(itemListContainer, 'input', '.quantity-input', (e) => {
        const itemId = e.target.dataset.id;
        const input = e.target;
        let value = parseInt(input.value.replace(/[^0-9]/g, ''), 10);
        value = Math.min(99, Math.max(1, value || 1));
        input.value = value;
        updateQuantity(itemId, value);
    });

    on(spawnButton, 'click', handleSpawn);
    renderItemSlots();
}

function renderItemSlots() {
    if (!itemListContainer) return;

    let html = '';

    selectedItems.forEach((itemEntry) => {
        const item = allItems.find(a => a.id === itemEntry.id);
        html += `
            <div class="restarter-item-slot item-slot-filled" data-tooltip="${item.name}">
                <img src="images/items/${item.id}.png" alt="${item.name}">
                <div class="quantity-selector">
                    <input type="number" class="quantity-input" value="${itemEntry.quantity}" 
                           min="1" max="99" data-id="${itemEntry.id}">
                </div>
                <button class="remove-item-btn" data-id="${itemEntry.id}">&times;</button>
            </div>
        `;
    });

    html += '<div class="restarter-item-slot item-slot-empty"><span>+</span></div>';
    itemListContainer.innerHTML = html;
}

function selectItem(itemId) {
    if (!selectedItems.find(i => i.id === itemId)) {
        selectedItems.push({ id: itemId, quantity: 1 });
        renderItemSlots();
    }
}

function removeItem(itemId) {
    selectedItems = selectedItems.filter(i => i.id !== itemId);
    renderItemSlots();
}

function updateQuantity(itemId, quantity) {
    const item = selectedItems.find(i => i.id === itemId);
    if (item) item.quantity = quantity;
}

async function handleSpawn() {
    if (selectedItems.length === 0) return;
    await spawnItems(selectedItems);
    selectedItems = [];
    renderItemSlots();
}