import { postData } from './apiService.js';
import { ALL_ITEMS } from '../itemConfig.js';

const MAX_SLOTS = 10;
let selectedItems = [];
let masterToggleState = false;

let itemListContainer = null;
let modalBackdrop = null;
let modal = null;
let modalCloseBtn = null;
let modalItemList = null;
let modalSearchInput = null;
let masterToggle = null;


export function setupAutoRestarter() {
    itemListContainer = document.getElementById('restarter-item-list');
    modalBackdrop = document.getElementById('item-modal-backdrop');
    modal = document.getElementById('item-modal');
    modalCloseBtn = document.getElementById('modal-close-btn');
    modalItemList = document.getElementById('modal-item-list');
    modalSearchInput = document.getElementById('modal-search-input');
    masterToggle = document.getElementById('restarter-master-toggle');

    if (!itemListContainer || !modalBackdrop || !modal || !modalCloseBtn || !modalItemList || !modalSearchInput || !masterToggle) {
        console.error("AutoRestarter konnte nicht alle DOM-Elemente finden.");
        return;
    }

    // --- Event Listeners registrieren ---

    // Klicks auf die Item-Slots (Event Delegation)
    itemListContainer.addEventListener('click', (event) => {
        const slot = event.target.closest('.restarter-item-slot');
        if (!slot) return;

        if (slot.classList.contains('item-slot-empty')) {
            // Auf '+' geklickt -> Modal öffnen
            openItemModal();
        }

        const removeBtn = event.target.closest('.remove-item-btn');
        if (removeBtn) {
            // Auf 'x' geklickt -> Item entfernen
            const itemIndex = parseInt(removeBtn.dataset.index, 10);
            removeItem(itemIndex);
        }
    });

    // Master-Toggle
    masterToggle.addEventListener('change', (event) => {
        masterToggleState = event.target.checked;
        sendStateToBackend();
    });

    // Modal-Steuerung
    modalCloseBtn.addEventListener('click', closeItemModal);
    modalBackdrop.addEventListener('click', (event) => {
        if (event.target === modalBackdrop) {
            closeItemModal();
        }
    });

    // Modal-Suche
    modalSearchInput.addEventListener('input', filterModalList);

    // --- Initiales Rendern ---
    buildModalList();
    renderItemSlots();
}

// --- 3. Slot-Rendering ---

/**
 * Zeichnet die 10 Item-Slots (gefüllt + leer) neu.
 */
function renderItemSlots() {
    if (!itemListContainer) return;

    itemListContainer.innerHTML = ''; // Liste leeren

    // 1. Gefüllte Slots
    selectedItems.forEach((itemId, index) => {
        const item = ALL_ITEMS.find(i => i.id === itemId);
        if (item) {
            itemListContainer.innerHTML += `
                <div class="restarter-item-slot item-slot-filled" title="${item.name}\n${item.description}">
                    <img src="${item.icon}" alt="${item.name}">
                    <button class="remove-item-btn" data-index="${index}">&times;</button>
                </div>
            `;
        }
    });

    // 2. Leere Slots für den Rest (bis MAX_SLOTS) rendern
    const emptySlots = MAX_SLOTS - selectedItems.length;
    for (let i = 0; i < emptySlots; i++) {
        // Der erste leere Slot ist der "Hinzufügen"-Button
        if (i === 0) {
            itemListContainer.innerHTML += `
                <div class="restarter-item-slot item-slot-empty">
                    <span>+</span>
                </div>
            `;
        } else {
            // Die restlichen 9 (oder weniger) sind nur Platzhalter
            itemListContainer.innerHTML += `
                <div class="restarter-item-slot item-slot-empty" style="opacity: 0.3; cursor: default; user-select: none;">
                    <span>+</span>
                </div>
            `;
        }
    }
}

// --- 4. Modal-Logik ---

/**
 * Baut die Item-Liste im Modal einmalig auf.
 */
function buildModalList() {
    if (!modalItemList) return;

    modalItemList.innerHTML = '';
    ALL_ITEMS.forEach(item => {
        // Wir fügen jedem Listeneintrag eine data-id hinzu
        const li = document.createElement('li');
        li.className = 'modal-item';
        li.dataset.itemId = item.id;
        li.innerHTML = `
            <img src="${item.icon}" alt="${item.name}" class="modal-item-icon">
            <div class="modal-item-info">
                <span class="modal-item-name">${item.name}</span>
                <span class="modal-item-desc">${item.description}</span>
            </div>
        `;

        // Klick-Listener, um das Item auszuwählen
        li.addEventListener('click', () => {
            selectItem(item.id);
        });

        modalItemList.appendChild(li);
    });
}

/**
 * Filtert die Item-Liste im Modal basierend auf der Sucheingabe.
 */
function filterModalList() {
    const searchTerm = modalSearchInput.value.toLowerCase();
    const items = modalItemList.querySelectorAll('.modal-item');

    items.forEach(item => {
        const name = item.querySelector('.modal-item-name').textContent.toLowerCase();
        const desc = item.querySelector('.modal-item-desc').textContent.toLowerCase();

        if (name.includes(searchTerm) || desc.includes(searchTerm)) {
            item.classList.remove('is-filtered');
        } else {
            item.classList.add('is-filtered');
        }
    });
}

function openItemModal() {
    // Setzt die Suche zurück und entfernt alle Filter
    modalSearchInput.value = '';
    filterModalList();

    // Versteckt Items, die bereits ausgewählt sind
    const itemsInList = modalItemList.querySelectorAll('.modal-item');
    itemsInList.forEach(li => {
        if (selectedItems.includes(li.dataset.itemId)) {
            li.style.display = 'none';
        } else {
            li.style.display = 'flex';
        }
    });

    modalBackdrop.classList.remove('is-hidden');
}

function closeItemModal() {
    modalBackdrop.classList.add('is-hidden');
}

// --- 5. State-Änderungen (Items hinzufügen/entfernen) ---

/**
 * Fügt ein Item zur Liste hinzu und schließt das Modal.
 */
function selectItem(itemId) {
    if (selectedItems.length < MAX_SLOTS && !selectedItems.includes(itemId)) {
        selectedItems.push(itemId);
        renderItemSlots(); // Slots neu zeichnen
        sendStateToBackend(); // Backend aktualisieren
    }
    closeItemModal();
}

/**
 * Entfernt ein Item aus der Liste.
 */
function removeItem(index) {
    if (index >= 0 && index < selectedItems.length) {
        selectedItems.splice(index, 1);
        renderItemSlots(); // Slots neu zeichnen
        sendStateToBackend(); // Backend aktualisieren
    }
}

// --- 6. Backend-Kommunikation ---

/**
 * Sendet den aktuellen Status (Toggle + Item-Liste) an das Backend.
 */
function sendStateToBackend() {
    console.log("Sende Auto-Restarter-Status an Backend:", {
        enabled: masterToggleState,
        items: selectedItems
    });

    postData('/api/action', {
        action: 'set_auto_restart_config',
        enabled: masterToggleState,
        itemIds: selectedItems
    });
}