import {postData} from './apiService.js';
import {openItemModal} from "./modalService.js";

let selectedItems = [];

let itemListContainer = null;
let spawnButton = null;
let allItems = [];


export async function setupItemSpawner(items) {
    allItems = items;

    itemListContainer = document.getElementById('item-spawner-list');
    spawnButton = document.getElementById('spawn-items-btn');

    if (!itemListContainer || !spawnButton) {
        console.error("ItemSpawner konnte nicht alle DOM-Elemente finden.");
        return;
    }

    itemListContainer.addEventListener('click', (event) => {
        const slot = event.target.closest('.restarter-item-slot');
        if (!slot) return;

        if (slot.classList.contains('item-slot-empty')) {
            openItemModal(
                (itemId) => selectItem(itemId),
                () => selectedItems.map(i => i.id)
            );
        } else if (event.target.classList.contains('remove-item-btn')) {
            const itemId = event.target.dataset.id;
            removeItem(itemId);
        }
    });

    itemListContainer.addEventListener('input', (event) => {
        if (event.target.classList.contains('quantity-input')) {
            const itemId = event.target.dataset.id;
            let newQuantity = parseInt(event.target.value, 10);
            if (newQuantity < 1) newQuantity = 1;
            if (newQuantity > 999) newQuantity = 999;
            updateQuantity(itemId, newQuantity);
        }
    });

    spawnButton.addEventListener('click', () => {
        sendStateToBackend();
    });

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
                    <input type="number" 
                           class="quantity-input" 
                           value="${itemEntry.quantity}" 
                           min="1" 
                           max="999" 
                           data-id="${itemEntry.id}">
                </div>
                
                <button class="remove-item-btn" data-id="${itemEntry.id}">&times;</button>
            </div>
        `;
    });

    html += `
        <div class="restarter-item-slot item-slot-empty">
            <span>+</span>
        </div>
    `;

    itemListContainer.innerHTML = html;
}

function selectItem(itemId) {
    if (!selectedItems.find(i => i.id === itemId)) {
        selectedItems.push({id: itemId, quantity: 1});
        renderItemSlots();
    }
}

function removeItem(itemId) {
    selectedItems = selectedItems.filter(i => i.id !== itemId);
    renderItemSlots();
}

function updateQuantity(itemId, quantity) {
    const item = selectedItems.find(i => i.id === itemId);
    if (item) {
        item.quantity = quantity;
    }
}

function sendStateToBackend() {
    if (selectedItems.length === 0) return;

    postData('/api/action', {
        action: 'spawn_items',
        items: selectedItems
    });

    selectedItems = [];
    renderItemSlots();
}