import { getElem } from '../../utils/dom.js';
import { executeAction } from '../../configs/actionHooksConfig.js';

export function setupTooltipListeners() {
    const iconContainer = getElem('minimap-icons');
    const tooltip = getElem('map-tooltip');

    if (!iconContainer || !tooltip) return;

    const tooltipState = createTooltipState(tooltip);

    setupIconHoverListeners(iconContainer, tooltipState);
    setupTooltipHoverListeners(tooltip, tooltipState);
    setupTooltipClickListener(tooltip, tooltipState);
}

function createTooltipState(tooltip) {
    return {
        pinnedIcon: null,
        hideTimeout: null,
        lockSwitch: false,
        tooltip
    };
}

function setupIconHoverListeners(iconContainer, state) {
    let enterTimer = null;

    iconContainer.addEventListener('mouseover', (e) => {
        const icon = e.target.closest('.map-icon');
        if (!icon) return;
        const from = e.relatedTarget;
        if (from && icon.contains(from)) return;

        clearTimeout(enterTimer);
        if (state.lockSwitch || state.pinnedIcon === icon) return;

        enterTimer = setTimeout(() => {
            state.pinnedIcon = icon;
            showTooltipForIcon(icon, state);
        }, 40);
    });

    iconContainer.addEventListener('mouseout', (e) => {
        const icon = e.target.closest('.map-icon');
        if (!icon) return;
        const to = e.relatedTarget;
        if (to && icon.contains(to)) return;
        if (icon === state.pinnedIcon) {
            scheduleHide(state);
        }
    });
}

function setupTooltipHoverListeners(tooltip, state) {
    tooltip.addEventListener('mouseenter', () => {
        clearTimeout(state.hideTimeout);
        state.lockSwitch = true;
    });

    tooltip.addEventListener('mouseleave', () => {
        state.lockSwitch = false;
        scheduleHide(state);
    });
}

function setupTooltipClickListener(tooltip, state) {
    tooltip.addEventListener('click', (e) => {
        const button = e.target.closest('button[data-action]');
        if (!button || !state.pinnedIcon) return;

        const instanceId = state.pinnedIcon.dataset.instanceId;
        const action = button.dataset.action;

        if (instanceId && action) {
            executeAction(action, { instanceId: parseInt(instanceId) });
            hideTooltip(state);
        }
    });
}

function showTooltipForIcon(iconEl, state) {
    const html = iconEl.dataset.tooltipHtml;
    if (!html) return;

    const isWide = html.includes('data-tooltip-wide="1"');
    state.tooltip.classList.toggle('tooltip--wide', isWide);

    const iconRect = iconEl.getBoundingClientRect();
    const gap = 4;

    state.tooltip.innerHTML = html;
    state.tooltip.style.display = 'block';
    state.tooltip.style.visibility = 'hidden';
    state.tooltip.style.left = '0px';
    state.tooltip.style.top = '0px';

    const ttWidth = state.tooltip.offsetWidth || (isWide ? 480 : 380);
    const ttHeight = state.tooltip.offsetHeight || 200;

    let left = iconRect.right + gap;
    let top = iconRect.top;

    if (left + ttWidth > window.innerWidth - 6) {
        left = Math.max(6, iconRect.left - gap - ttWidth);
    }
    top = Math.min(
        Math.max(6, iconRect.top + (iconRect.height / 2) - (ttHeight / 2)),
        window.innerHeight - 6 - ttHeight
    );

    state.tooltip.style.left = `${left}px`;
    state.tooltip.style.top = `${top}px`;
    state.tooltip.style.visibility = 'visible';
    document.body.classList.add('body-tooltip-open');
}

function scheduleHide(state, immediate = false) {
    clearTimeout(state.hideTimeout);
    state.hideTimeout = setTimeout(() => {
        if (!state.pinnedIcon) return;

        const overIcon = state.pinnedIcon.matches(':hover');
        const overTooltip = state.tooltip.matches(':hover');

        if (!overIcon && !overTooltip) {
            hideTooltip(state);
        }
    }, immediate ? 0 : 100);
}

function hideTooltip(state) {
    state.tooltip.style.display = 'none';
    state.pinnedIcon = null;
    document.body.classList.remove('body-tooltip-open');
}