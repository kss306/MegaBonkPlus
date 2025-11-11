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
    modalBackdrop = document.getElementById('item-modal-backdrop');
    modal = document.getElementById('item-modal');
    modalCloseBtn = document.getElementById('modal-close-btn');
    modalItemList = document.getElementById('modal-item-list');
    modalSearchInput = document.getElementById('modal-search-input');

    if (!modalBackdrop || !modal || !modalCloseBtn || !modalItemList || !modalSearchInput) {
        console.error("ModalService konnte nicht alle DOM-Elemente finden.");
        return;
    }

    modalBackdrop.addEventListener('click', closeItemModal);
    modalCloseBtn.addEventListener('click', closeItemModal);
    modal.addEventListener('click', (e) => e.stopPropagation());

    modalSearchInput.addEventListener('input', (e) => {
        buildModalList(e.target.value);
    });
}
export function openItemModal(onSelect, getDisabledItems) {
    onSelectCallback = onSelect;
    getDisabledItemsCallback = getDisabledItems;

    buildModalList(modalSearchInput.value);

    modalBackdrop.classList.remove('is-hidden');
    modalSearchInput.value = '';
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
            const li = document.createElement('li');
            li.className = 'modal-item';
            li.dataset.itemId = item.id;

            if (disabledItems.includes(item.id)) {
                li.classList.add('is-disabled');
            }

            li.innerHTML = `
                <img src="images/items/${item.id}.png" alt="${item.name}" class="modal-item-icon">
                <div class="modal-item-info">
                    <span class="modal-item-name">${item.name}</span>
                    <span class="modal-item-desc">${item.description}</span>
                </div>
            `;

            li.addEventListener('click', () => {
                if (!li.classList.contains('is-disabled') && onSelectCallback) {
                    onSelectCallback(item.id);
                    closeItemModal();
                }
            });
            modalItemList.appendChild(li);
        }
    });
}