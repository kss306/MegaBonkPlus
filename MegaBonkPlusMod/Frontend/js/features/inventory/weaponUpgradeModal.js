import {createElement, on} from '../../utils/dom.js';
import {sendWeaponAction} from '../../hooks/actions/weaponHook.js';
import {getWeaponInventory} from '../../hooks/actions/weaponDataHook.js';
import {applyWeaponInventory} from './weaponActions.js';
import {STAT_OPTIONS} from "../../enums/StatEnum.js";
import {MODIFY_TYPE_OPTIONS} from "../../enums/StatModifyTypeEnum.js";


let backdrop = null;
let modal = null;
let statsContainer = null;
let warningText = null;
let currentWeaponId = null;
let currentWeaponName = null;
let currentRarity = 'New';
let currentLevel = 1;
let currentAllowedStats = null;

export function openWeaponUpgradeModal({weaponId, weaponName, rarity, currentLevel: level, allowedStats}) {
    currentWeaponId = weaponId;
    currentWeaponName = weaponName;
    currentRarity = rarity || 'New';
    currentLevel = level || 1;
    currentAllowedStats = Array.isArray(allowedStats) && allowedStats.length > 0
        ? allowedStats
        : null;

    ensureModal();
    updateHeader();
    updateWarning();
    modal.classList.remove('is-hidden');
    backdrop.classList.remove('is-hidden');
}

function ensureModal() {
    if (backdrop) return;

    backdrop = createElement('div', {
        class: 'modal-backdrop',
        id: 'weapon-upgrade-modal-backdrop'
    });

    modal = createElement('div', {
        class: 'modal-content card weapon-upgrade-modal'
    });

    modal.innerHTML = `
        <div class="modal-header">
            <h3 id="weapon-upgrade-title">Upgrade Weapon</h3>
            <button id="weapon-upgrade-close-btn" class="modal-close-btn">&times;</button>
        </div>

        <div class="weapon-upgrade-body">
            <div id="weapon-upgrade-warning" class="weapon-upgrade-warning" style="display:none;"></div>

            <div class="weapon-upgrade-controls">
                <button id="weapon-add-stat-row-btn" class="quick-action-button" type="button">
                    + Add Modifier
                </button>
                <span class="weapon-upgrade-hint">
                    Up to 4 modifiers per upgrade.
                </span>
            </div>

            <div id="weapon-upgrade-stats-container" class="weapon-upgrade-stats-container"></div>
        </div>

        <div class="weapon-upgrade-footer">
            <button id="weapon-upgrade-apply-btn" class="quick-action-button" type="button">
                Apply Upgrade
            </button>
        </div>
    `;

    backdrop.appendChild(modal);
    document.body.appendChild(backdrop);

    const closeBtn = modal.querySelector('#weapon-upgrade-close-btn');
    const addRowBtn = modal.querySelector('#weapon-add-stat-row-btn');
    const applyBtn = modal.querySelector('#weapon-upgrade-apply-btn');

    statsContainer = modal.querySelector('#weapon-upgrade-stats-container');
    warningText = modal.querySelector('#weapon-upgrade-warning');

    on(backdrop, 'click', (e) => {
        if (e.target === backdrop) {
            hideModal();
        }
    });

    on(closeBtn, 'click', hideModal);
    on(addRowBtn, 'click', () => addStatRow());
    on(applyBtn, 'click', handleApplyUpgrade);
}

function hideModal() {
    if (!backdrop || !modal) return;
    modal.classList.add('is-hidden');
    backdrop.classList.add('is-hidden');
}

function updateHeader() {
    const title = modal.querySelector('#weapon-upgrade-title');
    if (title) {
        title.textContent = `Upgrade ${currentWeaponName} (${currentRarity})`;
    }
}

function updateWarning() {
    if (!warningText) return;

    if (currentLevel >= 40) {
        warningText.style.display = 'block';
        warningText.textContent =
            'Warning: Weapons above level 40 may flag your run as cheater in the leaderboard.';
    } else {
        warningText.style.display = 'none';
        warningText.textContent = '';
    }
}

function addStatRow() {
    if (!statsContainer) return;

    const existing = statsContainer.querySelectorAll('.weapon-upgrade-row').length;
    if (existing >= currentAllowedStats.length) return;

    const row = createElement('div', {class: 'weapon-upgrade-row'});

    const statSelect = createElement('select', {class: 'weapon-upgrade-select stat-select'});

    const statOptions = currentAllowedStats && currentAllowedStats.length > 0
        ? currentAllowedStats
        : STAT_OPTIONS;

    statOptions.forEach(s => {
        const opt = createElement('option', {value: s}, s);
        statSelect.appendChild(opt);
    });

    const opSelect = createElement('select', {class: 'weapon-upgrade-select op-select'});
    MODIFY_TYPE_OPTIONS.forEach(o => {
        const opt = createElement('option', {value: o}, o);
        opSelect.appendChild(opt);
    });

    const valueInput = createElement('input', {
        type: 'number',
        step: '0.01',
        class: 'weapon-upgrade-value',
        value: '0'
    });

    const removeBtn = createElement('button', {class: 'weapon-upgrade-remove-btn', type: 'button'}, '×');

    on(removeBtn, 'click', () => {
        statsContainer.removeChild(row);
    });

    row.appendChild(statSelect);
    row.appendChild(opSelect);
    row.appendChild(valueInput);
    row.appendChild(removeBtn);

    statsContainer.appendChild(row);
}

async function handleApplyUpgrade() {
    if (!statsContainer || !currentWeaponId) return;

    const rows = statsContainer.querySelectorAll('.weapon-upgrade-row');
    const stats = [];

    rows.forEach(row => {
        const stat = row.querySelector('.stat-select')?.value?.trim();
        const op = row.querySelector('.op-select')?.value?.trim();
        const valueStr = row.querySelector('.weapon-upgrade-value')?.value?.trim();
        const value = parseFloat(valueStr || '0');

        if (!stat || !op || !Number.isFinite(value)) return;

        stats.push({stat, operation: op, value});
    });

    if (stats.length === 0) {
        return;
    }

    await sendWeaponAction({
        mode: 'add',
        weapon: currentWeaponId,
        upgrade: {
            mode: 'custom',
            stats
        }
    });

    const inv = await getWeaponInventory();
    applyWeaponInventory(inv);

    const updated = inv.find(w => w.id === currentWeaponId);
    if (updated) {
        currentLevel = updated.level;
        updateWarning();
    }
}