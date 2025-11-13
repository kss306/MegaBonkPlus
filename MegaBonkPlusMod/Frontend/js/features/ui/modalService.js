import { getElem, createElement, on } from '../../utils/dom.js';

let modalBackdrop = null;
let modal = null;
let modalCloseBtn = null;
let modalItemList = null;
let modalSearchInput = null;

let allItemsCache = [];
let onSelectCallback = null;
let getDisabledItemsCallback = null;

export function setupModal(allItems) {
    allItemsCache = allItems;
    modalBackdrop = getElem('item-modal-backdrop');
    modal = getElem('item-modal');
    modalCloseBtn = getElem('modal-close-btn');
    modalItemList = getElem('modal-item-list');
    modalSearchInput = getElem('modal-search-input');

    if (!modalBackdrop || !modal || !modalCloseBtn || !modalItemList || !modalSearchInput) {
        console.error("ModalService: Missing DOM elements");
        return;
    }

    on(modalBackdrop, 'click', closeItemModal);
    on(modalCloseBtn, 'click', closeItemModal);
    on(modal, 'click', (e) => e.stopPropagation());
    on(modalSearchInput, 'input', (e) => buildModalList(e.target.value));
}

export function openItemModal(onSelect, getDisabledItems) {
    onSelectCallback = onSelect;
    getDisabledItemsCallback = getDisabledItems;
    modalSearchInput.value = '';
    buildModalList();
    modalBackdrop.classList.remove('is-hidden');
    modalSearchInput.focus();
}

function closeItemModal() {
    modalBackdrop.classList.add('is-hidden');
    onSelectCallback = null;
    getDisabledItemsCallback = null;
}

function buildModalList(searchTerm = '') {
    if (!modalItemList) return;

    modalItemList.innerHTML = '';
    const lowerSearchTerm = searchTerm.toLowerCase();
    const disabledItems = getDisabledItemsCallback ? getDisabledItemsCallback() : [];

    allItemsCache.forEach(item => {
        if (item.name.toLowerCase().includes(lowerSearchTerm)) {
            const li = createElement('li', {
                class: 'modal-item' + (disabledItems.includes(item.id) ? ' is-disabled' : ''),
                data: { itemId: item.id }
            });

            li.innerHTML = `
                <img src="images/items/${item.id}.png" alt="${item.name}" class="modal-item-icon">
                <div class="modal-item-info">
                    <span class="modal-item-name">${item.name}</span>
                    <span class="modal-item-desc">${item.description}</span>
                </div>
            `;

            on(li, 'click', () => {
                if (!li.classList.contains('is-disabled') && onSelectCallback) {
                    onSelectCallback(item.id);
                    closeItemModal();
                }
            });
            modalItemList.appendChild(li);
        }
    });
}