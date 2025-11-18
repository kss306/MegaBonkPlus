import {createElement, getElem, on} from '../../utils/dom.js';
import {fetchWeaponUpgradeOptions, sendWeaponAction} from '../../hooks/actions/weaponHook.js';
import {getAllWeapons, getWeaponInventory} from '../../hooks/actions/weaponDataHook.js';
import {getAllTomes, getTomeInventory} from '../../hooks/actions/tomeDataHook.js';
import {sendTomeAction} from '../../hooks/actions/tomeHook.js';
import {openWeaponModal} from '../ui/modalService.js';
import {openWeaponUpgradeModal} from './weaponUpgradeModal.js';
import {openConfirmModal} from '../ui/confirmModal.js';

let allWeapons = [];
let allTomes = [];
let equippedWeapons = [];
let equippedTomes = [];

let weaponSlotContainer = null;
let tomeSlotContainer = null;
let raritySelect = null;
let weaponMaxLevelWarningShown = false;

let lastWeaponInventorySerialized = null;
let lastTomeInventorySerialized = null;

export async function setupWeaponActions() {
    const card = document.querySelector('.inventory-card');
    if (!card) {
        return;
    }

    allWeapons = await getAllWeapons();
    allTomes = await getAllTomes();

    equippedWeapons = [];
    equippedTomes = [];

    card.innerHTML = '';
    const wrapper = createElement('div', {class: 'weapon-module'});

    wrapper.innerHTML = `
        <div class="weapon-module-header">
            <div class="weapon-title-group">
                <h3>Inventory</h3>
                <div class="info-tooltip"
                     data-tooltip="Manage your weapons and tomes: add, upgrade, downgrade">
                    ?
                </div>
            </div>
            <div class="weapon-header-controls">
                <label class="weapon-rarity-label">
                    Rarity
                    <select id="weapon-rarity-select" class="weapon-rarity-select">
                        <option value="Common">Common</option>
                        <option value="Uncommon">Uncommon</option>
                        <option value="Rare">Rare</option>
                        <option value="Epic">Epic</option>
                        <option value="Legendary">Legendary</option>
                    </select>
                </label>
                <button id="weapon-add-btn" class="quick-action-button" type="button">
                    + Add Weapon
                </button>
                <button id="tome-add-btn" class="quick-action-button" type="button">
                    + Add Tome
                </button>
            </div>
        </div>

        <div class="weapon-tome-container">
            <div class="weapon-column">
                <div class="weapon-slot-list-container">
                    <div id="weapon-slot-list" class="weapon-slot-list"></div>
                </div>
            </div>
            <div class="tome-column">
                <div class="tome-slot-list-container">
                    <div id="tome-slot-list" class="weapon-slot-list"></div>
                </div>
            </div>
        </div>
    `;

    card.appendChild(wrapper);

    weaponSlotContainer = getElem('weapon-slot-list');
    tomeSlotContainer = getElem('tome-slot-list');
    const addBtn = getElem('weapon-add-btn');
    const addTomeBtn = getElem('tome-add-btn');
    raritySelect = getElem('weapon-rarity-select');

    if (!weaponSlotContainer || !tomeSlotContainer || !addBtn || !raritySelect) {
        console.error('WeaponActions: Missing DOM elements in module');
        return;
    }

    on(addBtn, 'click', openWeaponSelectModal);
    on(addTomeBtn, 'click', openTomeSelectModal);

    renderWeaponSlots();
    renderTomeSlots();
}

export function applyWeaponInventory(inventory) {
    const normalized = inventory || [];

    const serialized = JSON.stringify(normalized);
    if (serialized === lastWeaponInventorySerialized) {
        return;
    }
    lastWeaponInventorySerialized = serialized;

    equippedWeapons = normalized;
    renderWeaponSlots();
}

export function applyTomeInventory(inventory) {
    const normalized = inventory || [];

    const serialized = JSON.stringify(normalized);
    if (serialized === lastTomeInventorySerialized) {
        return;
    }
    lastTomeInventorySerialized = serialized;

    equippedTomes = normalized;
    renderTomeSlots();
}

function renderWeaponSlots() {
    if (!weaponSlotContainer) return;

    const totalWeapons = equippedWeapons.length;
    const minSlots = 4;
    const totalSlots = Math.max(minSlots, totalWeapons);

    let html = '';

    for (let i = 0; i < totalSlots; i++) {
        const w = equippedWeapons[i];
        if (w) {
            html += renderWeaponSlotHtml(w);
        } else {
            html += `
                <div class="weapon-slot restarter-item-slot item-slot-empty">
                    <span class="weapon-slot-plus">+</span>
                </div>
            `;
        }
    }

    weaponSlotContainer.innerHTML = html;

    weaponSlotContainer.querySelectorAll('.item-slot-empty').forEach(slot => {
        on(slot, 'click', openWeaponSelectModal);
    });

    weaponSlotContainer.querySelectorAll('.weapon-slot-filled').forEach(slot => {
        const weaponId = slot.dataset.weaponId;

        const quickRegion = slot.querySelector('.weapon-slot-region-top');
        const customRegion = slot.querySelector('.weapon-slot-region-middle');
        const downgradeRegion = slot.querySelector('.weapon-slot-region-bottom');
        const removeBtn = slot.querySelector('.weapon-slot-remove-btn');

        if (quickRegion) {
            on(quickRegion, 'click', async (e) => {
                e.stopPropagation();
                const weapon = equippedWeapons.find(w => w.id === weaponId);
                if (!weaponMaxLevelWarningShown && weapon?.level >= 40) {

                    const result = await openConfirmModal({
                        title: 'Quick Upgrade Warning',
                        message: `
                            <p>Weapons above level 40 may flag your run as cheater in the leaderboard.</p>
                            <p>Do you want to continue with a quick random upgrade?</p>
                        `,
                        buttons: [
                            {id: 'cancel', label: 'Cancel', variant: 'default'},
                            {id: 'continue', label: 'Continue', variant: 'primary'}
                        ]
                    });

                    if (result !== 'continue') {
                        return;
                    }

                    weaponMaxLevelWarningShown = true;
                }

                const rarity = raritySelect?.value || 'New';
                await sendWeaponAction({
                    mode: 'add',
                    weapon: weaponId,
                    upgrade: {
                        mode: 'random',
                        rarity
                    }
                });
                const inv = await getWeaponInventory();
                applyWeaponInventory(inv);
            });
        }

        if (customRegion) {
            on(customRegion, 'click', async (e) => {
                e.stopPropagation();
                const weapon = equippedWeapons.find(w => w.id === weaponId);
                const rarity = raritySelect?.value || 'New';

                let allowedStats = null;
                const optionsResult = await fetchWeaponUpgradeOptions(weaponId);
                if (optionsResult.ok && optionsResult.data && Array.isArray(optionsResult.data.allowedStats)) {
                    allowedStats = optionsResult.data.allowedStats;
                }

                openWeaponUpgradeModal({
                    weaponId,
                    weaponName: weapon?.name || weaponId,
                    rarity,
                    currentLevel: weapon?.level ?? 1,
                    allowedStats
                });
            });
        }

        if (downgradeRegion) {
            on(downgradeRegion, 'click', async (e) => {
                e.stopPropagation();
                await sendWeaponAction({
                    mode: 'downgrade',
                    weapon: weaponId
                });
                const inv = await getWeaponInventory();
                applyWeaponInventory(inv);
            });
        }

        if (removeBtn) {
            on(removeBtn, 'click', async (e) => {
                e.stopPropagation();
                await sendWeaponAction({
                    mode: 'remove',
                    weapon: weaponId
                });
                const inv = await getWeaponInventory();
                applyWeaponInventory(inv);
            });
        }
    });
}

function renderTomeSlots() {
    if (!tomeSlotContainer) return;

    const totalTomes = equippedTomes.length;
    const minSlots = 4;
    const totalSlots = Math.max(minSlots, totalTomes);

    let html = '';

    for (let i = 0; i < totalSlots; i++) {
        const t = equippedTomes[i];
        if (t) {
            html += renderTomeSlotHtml(t);
        } else {
            html += `
                <div class="weapon-slot restarter-item-slot item-slot-empty">
                    <span class="weapon-slot-plus">+</span>
                </div>
            `;
        }
    }

    tomeSlotContainer.innerHTML = html;

    tomeSlotContainer.querySelectorAll('.item-slot-empty').forEach(slot => {
        on(slot, 'click', openTomeSelectModal);
    });

    tomeSlotContainer.querySelectorAll('.tome-slot-filled').forEach(slot => {
        const tomeId = slot.dataset.tomeId;

        const quickRegion = slot.querySelector('.weapon-slot-region-top');
        const removeBtn = slot.querySelector('.weapon-slot-remove-btn');

        if (quickRegion) {
            on(quickRegion, 'click', async (e) => {
                e.stopPropagation();
                const rarity = raritySelect?.value || 'New';

                await sendTomeAction({
                    mode: 'add',
                    tome: tomeId,
                    upgrade: {
                        mode: 'random',
                        rarity
                    }
                });

                const inv = await getTomeInventory();
                applyTomeInventory(inv);
            });
        }

        if (removeBtn) {
            on(removeBtn, 'click', async (e) => {
                e.stopPropagation();
                await sendTomeAction({
                    mode: 'remove',
                    tome: tomeId
                });
                const inv = await getTomeInventory();
                applyTomeInventory(inv);
            });
        }
    });
}

function renderWeaponSlotHtml(weapon) {
    const displayName = weapon.name || weapon.id;
    const iconId = weapon.id;

    return `
        <div class="weapon-slot restarter-item-slot item-slot-filled weapon-slot-filled" data-weapon-id="${weapon.id}">
            <div class="weapon-slot-name" title="${displayName}">${displayName}</div>
            <div class="weapon-slot-level">Lv. ${weapon.level}</div>
            <div class="weapon-slot-icon-wrapper">
                <img src="images/inventory/${iconId}.png" alt="${displayName}" class="weapon-slot-icon">
                <div class="weapon-slot-overlay">
                    <div
                        class="weapon-slot-region weapon-slot-region-top"
                        title="Quick random upgrade for selected rarity">
                    </div>
                    <div
                        class="weapon-slot-region weapon-slot-region-middle"
                        title="Open custom stat upgrade modal">
                    </div>
                    <div
                        class="weapon-slot-region weapon-slot-region-bottom"
                        title="Downgrade this weapon by one level">
                    </div>
                </div>
            </div>
            <button
                class="weapon-slot-remove-btn"
                type="button"
                title="Remove this weapon from your inventory">
                Remove
            </button>
        </div>
    `;
}

function renderTomeSlotHtml(tome) {
    const displayName = tome.name || tome.id;
    const iconId = tome.id;

    return `
        <div class="weapon-slot restarter-item-slot item-slot-filled tome-slot-filled" data-tome-id="${tome.id}">
            <div class="weapon-slot-name" title="${displayName}">${displayName}</div>
            <div class="weapon-slot-level">Lv. ${tome.level}</div>
            <div class="weapon-slot-icon-wrapper">
                <img src="images/inventory/${iconId}.png" alt="${displayName}" class="weapon-slot-icon">
                <div class="weapon-slot-overlay">
                    <div
                        class="weapon-slot-region weapon-slot-region-top"
                        title="Quick random upgrade for this tome">
                    </div>
                </div>
            </div>
            <button
                class="weapon-slot-remove-btn"
                type="button"
                title="Remove this tome from your inventory">
                Remove
            </button>
        </div>
    `;
}

function openWeaponSelectModal() {
    const equippedIds = equippedWeapons.map(w => w.id);
    const hasTooManyWeapons = equippedWeapons.length >= 4;

    openWeaponModal(
        allWeapons,
        async (weaponId) => {
            await sendWeaponAction({
                mode: 'add',
                weapon: weaponId,
                upgrade: {
                    mode: 'random',
                    rarity: 'New'
                }
            });
            const inv = await getWeaponInventory();
            applyWeaponInventory(inv);
        },
        () => equippedIds,
        hasTooManyWeapons
            ? 'Warning: More than 4 weapons may flag your run as cheater in the leaderboard.'
            : null,
        'Select Weapon'
    );
}

function openTomeSelectModal() {
    const equippedIds = equippedTomes.map(t => t.id);
    const hasTooManyTomes = equippedTomes.length >= 4;

    let warningText = null;
    if (hasTooManyTomes) {
        warningText = 'Warning: More than 4 tomes may flag your run as cheater in the leaderboard.';
    }

    openWeaponModal(
        allTomes,
        async (tomeId) => {
            await sendTomeAction({
                mode: 'add',
                tome: tomeId,
                upgrade: {
                    mode: 'random',
                    rarity: 'New'
                }
            });
            const inv = await getTomeInventory();
            applyTomeInventory(inv);
        },
        () => equippedIds,
        warningText,
        'Select Tome'
    );
}