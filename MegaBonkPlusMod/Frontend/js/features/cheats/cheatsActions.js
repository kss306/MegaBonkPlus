import {CHEATS_CONFIG} from '../../configs/cheatsConfig.js';
import {createElement, getElem, on} from '../../utils/dom.js';
import {openConfirmModal} from '../ui/confirmModal.js';
import {executeAction} from '../../configs/actionHooksConfig.js';
import {getTimeScale} from "../../hooks/gameStateHook.js";

let _timeScaleSyncIntervalId = null;

export async function setupCheatActions() {
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
                <div class="info-tooltip" data-tooltip="Use at your own risk.">?</div>
            </div>
        </div>
        <div class="cheats-body">
            <div class="cheats-list" id="cheats-list"></div>
        </div>
    `;

    card.appendChild(wrapper);
    const list = getElem('cheats-list');
    renderCheatItems(list, CHEATS_CONFIG);

    await syncTimeScaleSlider(list);
    startTimeScaleSliderAutoSync(list);
}

async function syncTimeScaleSlider(container) {
    const sliderRow = container.querySelector('[data-cheat-id="game_time_scale"]');
    if (!sliderRow) return;

    const input = sliderRow.querySelector('input');
    const label = sliderRow.querySelector('.slider-value');

    const currentScale = await getTimeScale();

    if (input && label) {
        input.value = currentScale;
        const config = CHEATS_CONFIG.find(c => c.id === 'game_time_scale');
        const unit = config ? (config.unit || '') : '';
        label.textContent = `${currentScale.toFixed(2)}${unit}`;
    }
}

function startTimeScaleSliderAutoSync(container) {
    if (_timeScaleSyncIntervalId) {
        clearInterval(_timeScaleSyncIntervalId);
        _timeScaleSyncIntervalId = null;
    }

    const sliderRow = container.querySelector('[data-cheat-id="game_time_scale"]');
    if (!sliderRow) return;

    const input = sliderRow.querySelector('input');
    const label = sliderRow.querySelector('.slider-value');
    if (!input || !label) return;

    const config = CHEATS_CONFIG.find(c => c.id === 'game_time_scale');
    const unit = config ? (config.unit || '') : '';

    let isDragging = false;

    on(input, 'pointerdown', () => { isDragging = true; });
    on(input, 'pointerup', () => { isDragging = false; });
    on(input, 'pointercancel', () => { isDragging = false; });
    on(input, 'blur', () => { isDragging = false; });

    const updateUi = (scale) => {
        input.value = scale;
        label.textContent = `${scale.toFixed(2)}${unit}`;
    };

    _timeScaleSyncIntervalId = setInterval(async () => {
        if (isDragging) return;

        const scale = await getTimeScale();
        const uiValue = Number(input.value);

        if (!Number.isFinite(scale)) return;
        if (Number.isFinite(uiValue) && Math.abs(uiValue - scale) < 0.001) return;

        updateUi(scale);
    }, 500);
}

function renderCheatItems(container, config) {
    container.innerHTML = config.map(renderCheatItemHtml).join('');

    config.forEach(cheat => {
        const elem = container.querySelector(`[data-cheat-id="${cheat.id}"]`);
        if (!elem) return;

        if (cheat.type === 'button') {
            on(elem, 'click', () => handleCheatClick(cheat));
        }
        else if (cheat.type === 'slider') {
            const input = elem.querySelector('input');
            const valueLabel = elem.querySelector('.slider-value');
            const resetBtn = elem.querySelector('.slider-reset-btn');

            let sendTimer = null;
            let lastSentValue = null;

            const formatValue = (val) => `${val.toFixed(2)}${cheat.unit || ''}`;

            const queueSend = async (rawValue) => {
                const numeric = Number(rawValue);
                if (!Number.isFinite(numeric)) return;

                if (lastSentValue !== null && Math.abs(lastSentValue - numeric) < 0.0001) return;

                lastSentValue = numeric;
                await executeAction(cheat.actionId, { value: numeric });
            };

            const scheduleSend = (rawValue) => {
                if (sendTimer) clearTimeout(sendTimer);
                sendTimer = setTimeout(() => { queueSend(rawValue); }, 120);
            };

            on(input, 'input', (e) => {
                const val = Number(e.target.value);
                if (Number.isFinite(val)) {
                    valueLabel.textContent = formatValue(val);
                    scheduleSend(e.target.value);
                }
            });

            on(input, 'change', async (e) => {
                await queueSend(e.target.value);
            });

            if (resetBtn) {
                on(resetBtn, 'click', async (e) => {
                    e.stopPropagation();
                    const defaultVal = cheat.defaultValue || 1.0;

                    if (sendTimer) clearTimeout(sendTimer);
                    lastSentValue = null;

                    input.value = defaultVal;
                    valueLabel.textContent = formatValue(defaultVal);
                    await executeAction(cheat.actionId, { value: defaultVal });
                });
            }
        }
    });
}

function renderCheatItemHtml(cheat) {
    if (cheat.type === 'slider') {
        const resetHtml = cheat.showReset
            ? `<button class="quick-action-button slider-reset-btn" title="Reset to ${cheat.defaultValue}">↺</button>`
            : '';

        return `
            <div class="cheat-row" data-cheat-id="${cheat.id}">
                <div class="cheat-info">
                    <span class="cheat-label">${cheat.label}</span>
                </div>
                <div class="cheat-control slider-control-group">
                    <div class="slider-wrapper">
                        <input type="range" 
                               min="${cheat.min}" 
                               max="${cheat.max}" 
                               step="${cheat.step}" 
                               value="${cheat.defaultValue}"
                               class="cheat-slider">
                        <span class="slider-value">${cheat.defaultValue.toFixed(2)}${cheat.unit || ''}</span>
                    </div>
                    ${resetHtml}
                </div>
            </div>
        `;
    }
    
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
                        data-cheat-id="${cheat.id}">
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