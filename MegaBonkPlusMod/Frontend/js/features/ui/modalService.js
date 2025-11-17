import {createElement, getElem, on} from '../../utils/dom.js';

let modalBackdrop = null;
let modal = null;
let modalCloseBtn = null;
let modalItemList = null;
let modalSearchInput = null;
let modalTitleElem = null;
let modalWarningElem = null;

let itemDataCache = [];
let modalDataCache = [];
let onSelectCallback = null;
let getDisabledItemsCallback = null;
let currentImageBasePath = 'images/items';
let currentTitle = 'Select Item';
let currentWarningText = '';

export function setupModal(allItems) {
    itemDataCache = allItems || [];
    modalDataCache = itemDataCache;
    currentImageBasePath = 'images/items';
    currentTitle = 'Select Item';

    modalBackdrop = getElem('item-modal-backdrop');
    modal = getElem('item-modal');
    modalCloseBtn = getElem('modal-close-btn');
    modalItemList = getElem('modal-item-list');
    modalSearchInput = getElem('modal-search-input');
    modalTitleElem = modal?.querySelector('.modal-header h3');

    if (!modalBackdrop || !modal || !modalCloseBtn || !modalItemList || !modalSearchInput || !modalTitleElem) {
        console.error("ModalService: Missing DOM elements");
        return;
    }

    const searchContainer = modal.querySelector('.modal-search');
    modalWarningElem = createElement('div', {
        class: 'modal-warning',
        id: 'modal-warning'
    });
    modalWarningElem.style.display = 'none';
    searchContainer.insertAdjacentElement('afterend', modalWarningElem);

    on(modalBackdrop, 'click', closeItemModal);
    on(modalCloseBtn, 'click', closeItemModal);
    on(modal, 'click', (e) => e.stopPropagation());
    on(modalSearchInput, 'input', (e) => buildModalList(e.target.value));
}

export function setModalData(data, options = {}) {
    modalDataCache = data || [];
    if (options.title) currentTitle = options.title;
    if (options.imageBasePath) currentImageBasePath = options.imageBasePath;
    currentWarningText = options.warningText || '';
}

function applyModalWarning() {
    if (!modalWarningElem) return;

    if (currentWarningText) {
        modalWarningElem.style.display = 'block';
        modalWarningElem.textContent = currentWarningText;
    } else {
        modalWarningElem.style.display = 'none';
        modalWarningElem.textContent = '';
    }
}


export function openItemModal(onSelect, getDisabledItems) {
    onSelectCallback = onSelect;
    getDisabledItemsCallback = getDisabledItems;

    modalDataCache = itemDataCache;
    currentTitle = 'Select Item';
    currentImageBasePath = 'images/items';
    currentWarningText = '';

    if (modalTitleElem) modalTitleElem.textContent = currentTitle;
    applyModalWarning();

    modalSearchInput.value = '';
    buildModalList();
    modalBackdrop.classList.remove('is-hidden');
    modalSearchInput.focus();
}

export function openWeaponModal(allWeapons, onSelect, getDisabledWeapons, warningText = null, titleText = "Select Object") {
    onSelectCallback = onSelect;
    getDisabledItemsCallback = getDisabledWeapons;

    setModalData(allWeapons, {
        title: titleText,
        imageBasePath: 'images/inventory',
        warningText: warningText || ''
    });

    if (modalTitleElem) modalTitleElem.textContent = currentTitle;
    applyModalWarning();

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

    modalDataCache.forEach(item => {
        if (!item || !item.name) return;
        if (!item.name.toLowerCase().includes(lowerSearchTerm)) return;

        const li = createElement('li', {
            class: 'modal-item' + (disabledItems.includes(item.id) ? ' is-disabled' : ''),
            data: {itemId: item.id}
        });

        li.innerHTML = `
            <img src="${currentImageBasePath}/${item.id}.png" alt="${item.name}" class="modal-item-icon">
            <div class="modal-item-info">
                <span class="modal-item-name">${item.name}</span>
                <span class="modal-item-desc">${item.description ?? ''}</span>
            </div>
        `;

        on(li, 'click', () => {
            if (!li.classList.contains('is-disabled') && onSelectCallback) {
                onSelectCallback(item.id);
                closeItemModal();
            }
        });
        modalItemList.appendChild(li);
    });
}