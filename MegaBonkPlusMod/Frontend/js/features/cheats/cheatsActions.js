import {CHEATS_CONFIG} from '../../configs/cheatsConfig.js';
import {createElement, getElem, on} from '../../utils/dom.js';
import {openConfirmModal} from '../ui/confirmModal.js';
import {executeAction} from '../../configs/actionHooksConfig.js';

export function setupCheatActions() {
    const card = getElem('cheats-card');
    if (!card) {
        console.warn('CheatActions: cheats-card element not found');
        return;
    }

    card.innerHTML = '';

    const wrapper = createElement('div', {class: 'cheats-module'});

    wrapper.innerHTML = `
        <div class="cheats-header">
            <div class="cheats-title-group">
                <h3>Cheats</h3>
                <div class="info-tooltip"
                     data-tooltip="Use at your own risk.">
                    ?
                </div>
            </div>
        </div>
        <div class="cheats-body">
            <div class="cheats-list" id="cheats-list"></div>
        </div>
    `;

    card.appendChild(wrapper);

    const list = getElem('cheats-list');
    if (!list) {
        console.error('CheatActions: cheats-list element missing');
        return;
    }

    renderCheatItems(list, CHEATS_CONFIG);
}

function renderCheatItems(container, config) {
    const itemsHtml = config.map(renderCheatItemHtml).join('');
    container.innerHTML = itemsHtml;

    container.querySelectorAll('[data-cheat-id]').forEach(elem => {
        const cheatId = elem.getAttribute('data-cheat-id');
        const cheat = CHEATS_CONFIG.find(c => c.id === cheatId);
        if (!cheat) return;

        on(elem, 'click', () => handleCheatClick(cheat));
    });
}

function renderCheatItemHtml(cheat) {
    const idAttr = cheat.id;
    
    if (cheat.type === 'button') {
        return `
            <div class="cheat-row">
                <div class="cheat-info">
                    <span class="cheat-label">${cheat.label}</span>
                </div>
                <div class="cheat-control">
                    <button
                        type="button"
                        class="quick-action-button cheat-button"
                        data-cheat-id="${idAttr}">
                        ${cheat.buttonLabel}
                    </button>
                </div>
            </div>
        `;
    }

    return `
        <div class="cheat-row">
            <div class="cheat-info">
                <span class="cheat-label">${cheat.label}</span>
            </div>
            <div class="cheat-control">
                <span>Unsupported type: ${cheat.type}</span>
            </div>
        </div>
    `;
}

async function handleCheatClick(cheat) {
    const payload = cheat.defaultPayload || {};

    if (cheat.confirm) {
        const result = await openConfirmModal({
            title: cheat.confirm.title,
            message: cheat.confirm.message,
            buttons: [
                {
                    id: 'cancel',
                    label: cheat.confirm.cancelLabel || 'Cancel',
                    variant: 'default'
                },
                {
                    id: 'confirm',
                    label: cheat.confirm.confirmLabel || 'Confirm',
                    variant: cheat.confirm.variant === 'danger' ? 'danger' : 'primary'
                }
            ]
        });

        if (result !== 'confirm') {
            return;
        }
    }

    if (!cheat.actionId) {
        console.warn(`Cheat '${cheat.id}' has no actionId configured`);
        return;
    }

    await executeAction(cheat.actionId, payload);
}