import { FILTERS_CONFIG } from '../configs/filterConfig.js';

const filterState = {};

function createToggleHTML(id, isChecked = true) {
    return `
        <label class="toggle-switch">
            <input type="checkbox" id="${id}" ${isChecked ? 'checked' : ''}>
            <span class="slider"></span>
        </label>
    `;
}

function generateFilterList() {
    const container = document.getElementById('filter-list-container');
    if (!container) return;

    let html = '';
    for (const filter of FILTERS_CONFIG) {
        const parentId = `filter-${filter.id}`;
        const hasRarities = filter.rarities && filter.rarities.length > 0;

        filterState[filter.id] = { main: true };

        const iconHtml = filter.icon
            ? `<img src="${filter.icon}" alt="${filter.name}">`
            : '';

        html += `
            <li class="filter-item" 
                data-filter-id="${filter.id}" 
                ${hasRarities ? `data-target-list="rarity-list-${filter.id}"` : ''}>
                
                <span class="filter-icon">${iconHtml}</span>
                <span class="filter-name">${filter.name}</span>
                ${createToggleHTML(parentId, true)}
            </li>
        `;

        if (hasRarities) {
            html += `
                <li class="filter-rarity-container">
                    <ul class="filter-rarity-list" id="rarity-list-${filter.id}">
            `;
            for (const rarity of filter.rarities) {
                const childId = `filter-${filter.id}-${rarity}`;
                filterState[filter.id][rarity] = true;
                html += `
                    <li class="filter-rarity-item" data-rarity="${rarity}">
                        <span></span> <span class="rarity-name">${rarity.charAt(0).toUpperCase() + rarity.slice(1)}</span>
                        ${createToggleHTML(childId, true)}
                    </li>
                `;
            }
            html += `
                    </ul>
                </li>
            `;
        }
    }
    container.innerHTML = html;
}

function attachFilterListeners(onFilterChange) {
    const container = document.getElementById('filter-list-container');
    if (!container) return;
    
    container.addEventListener('click', (event) => {
        const item = event.target.closest('.filter-item');

        if (!item || event.target.closest('.toggle-switch') || item.classList.contains('is-disabled')) {
            return;
        }

        const targetListId = item.dataset.targetList;
        if (!targetListId) return;

        const list = document.getElementById(targetListId);

        if (list) {
            list.classList.toggle('is-open');
        }
    });
    
    container.addEventListener('change', (event) => {
        if (!event.target.matches('input[type="checkbox"]')) return;

        const checkbox = event.target;
        const li = checkbox.closest('li');

        if (li.classList.contains('filter-item')) {
            const filterId = li.dataset.filterId;
            const isChecked = checkbox.checked;

            filterState[filterId].main = isChecked;

            li.classList.toggle('is-disabled', !isChecked);

            if (!isChecked) {
                const targetListId = li.dataset.targetList;
                if (targetListId) {
                    const list = document.getElementById(targetListId);
                    if (list) {
                        list.classList.remove('is-open');
                    }
                }
            }

        } else if (li.classList.contains('filter-rarity-item')) {
            const rarity = li.dataset.rarity;
            const parentLi = li.closest('.filter-rarity-container').previousElementSibling;
            const filterId = parentLi.dataset.filterId;

            filterState[filterId][rarity] = checkbox.checked;
        }

        if (onFilterChange) onFilterChange();
    });
}

/**
 * Öffentliche Funktion, die von app.js aufgerufen wird.
 */
export function initializeFilters(onFilterChange) {
    generateFilterList();
    attachFilterListeners(onFilterChange);
}

/**
 * Öffentliche Funktion, die den aktuellen Zustand zurückgibt.
 */
export function getFilterState() {
    return filterState;
}