import {getElem} from './utils.js';
import {MAP_SCALE, WORLD_OFFSET_X, WORLD_OFFSET_Z} from '../config.js';
import {postData} from './apiService.js';

const imageCache = {};

export function renderMinimap(data) {
    const canvas = getElem('minimap-canvas');
    const wrapper = getElem('minimap-wrapper');

    if (!canvas || !wrapper) return;

    if (data.count === 0 || !data.items[0] || !data.items[0].customProperties.rawPixelData) {
        wrapper.classList.add('is-loading');
        return;
    }

    wrapper.classList.remove('is-loading');

    const props = data.items[0].customProperties;
    const width = props.width;
    const height = props.height;

    if (canvas.width !== width) canvas.width = width;
    if (canvas.height !== height) canvas.height = height;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    try {
        const binaryString = atob(props.rawPixelData);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        const imageData = new ImageData(new Uint8ClampedArray(bytes.buffer), width, height);
        ctx.putImageData(imageData, 0, 0);
    } catch (e) {
        console.error("Fehler beim Malen der Minimap auf das Canvas:", e);
    }
}

function worldToCanvasPercentages(worldPos, canvasWidth, canvasHeight) {
    const canvasCenterX = canvasWidth / 2;
    const canvasCenterY = canvasHeight / 2;

    const relativeX = worldPos.x - WORLD_OFFSET_X;
    const relativeZ = worldPos.z - WORLD_OFFSET_Z;

    const u = (relativeX / MAP_SCALE) + canvasCenterX;
    const v = (relativeZ / MAP_SCALE) + canvasCenterY;

    let leftPercent = (u / canvasWidth) * 100;
    let topPercent = (v / canvasHeight) * 100;

    leftPercent = Math.max(0, Math.min(100, leftPercent));
    topPercent = Math.max(0, Math.min(100, topPercent));

    return {leftPercent, topPercent};
}

export function updateMapIcons(dataMap, configMap, filterStates) {
    const iconContainer = getElem('minimap-icons');
    const canvas = getElem('minimap-canvas');
    if (!iconContainer || !canvas || canvas.width === 0) {
        return;
    }

    let allIconsHtml = '';
    const canvasWidth = canvas.width;
    const canvasHeight = canvas.height;

    for (const key of Object.keys(dataMap)) {
        if (key === 'minimap' || !configMap[key].renderConfigSelector) continue;

        const data = dataMap[key];
        const config = configMap[key];
        
        const filter = filterStates[key];
        
        if (!filter || !filter.main) {
            continue;
        }

        const hasRarities = Object.keys(filter).some(k => k !== 'main');

        if (data.count === 0) continue;

        data.items.forEach(item => {
            if (hasRarities) {
                const itemRarity = (item.customProperties?.rarity || 'common').toLowerCase();

                if (filter[itemRarity] === false) {
                    return;
                }
            }

            const renderInfo = config.renderConfigSelector(item);
            if (renderInfo.type === 'none') return;

            const tooltipHtml = config.getTooltipHtml(item).replace(/"/g, '&quot;');
            const {leftPercent, topPercent} = worldToCanvasPercentages(item.position, canvasWidth, canvasHeight);

            const sizeInPixels = renderInfo.size ?? 16;
            const widthPercent = (sizeInPixels / canvasWidth) * 100;
            const heightPercent = (sizeInPixels / canvasHeight) * 100;

            let style = `
                --icon-left: ${leftPercent}%; 
                --icon-top: ${topPercent}%; 
                --icon-width: ${widthPercent}%; 
                --icon-height: ${heightPercent}%;
            `;

            if (renderInfo.type === 'image') {
                allIconsHtml +=
                    `<img class="map-icon" 
                          src="${renderInfo.path}" 
                          style="${style}"
                          data-tooltip-html="${tooltipHtml}"
                          data-instance-id="${item.instanceId}"
                    />`;
            } else if (renderInfo.type === 'dot') {
                style += ` background-color: ${renderInfo.color};`;
                allIconsHtml +=
                    `<div class="map-icon" 
                         style="${style}"
                         data-tooltip-html="${tooltipHtml}"
                         data-instance-id="${item.instanceId}"
                    ></div>`;
            }
        });
    }

    iconContainer.innerHTML = allIconsHtml;
}

export function setupTooltipListeners() {
    const iconContainer = getElem('minimap-icons');
    const tooltip = getElem('map-tooltip');
    if (!iconContainer || !tooltip) return;

    let pinnedIcon = null;
    let hideTimeout = null;
    let lockSwitch = false;

    function setBodyFlag(open) {
        document.body.classList.toggle('body-tooltip-open', !!open);
    }

    function showTooltipForIcon(iconEl) {
        const html = iconEl.dataset.tooltipHtml;
        if (!html) return;

        const isWide = html.includes('data-tooltip-wide="1"');

        tooltip.classList.toggle('tooltip--wide', isWide);

        const iconRect = iconEl.getBoundingClientRect();
        const gap = 4;

        tooltip.innerHTML = html;
        tooltip.style.display = 'block';
        tooltip.style.visibility = 'hidden';
        tooltip.style.left = '0px';
        tooltip.style.top = '0px';

        const ttWidth = tooltip.offsetWidth || (isWide ? 480 : 380);
        const ttHeight = tooltip.offsetHeight || 200;

        let left = iconRect.right + gap;
        let top = iconRect.top;

        if (left + ttWidth > window.innerWidth - 6) {
            left = Math.max(6, iconRect.left - gap - ttWidth);
        }
        top = Math.min(
            Math.max(6, iconRect.top + (iconRect.height / 2) - (ttHeight / 2)),
            window.innerHeight - 6 - ttHeight
        );

        tooltip.style.left = `${left}px`;
        tooltip.style.top = `${top}px`;
        tooltip.style.visibility = 'visible';
        setBodyFlag(true);
    }

    function scheduleHide(immediate = false) {
        clearTimeout(hideTimeout);
        hideTimeout = setTimeout(() => {
            if (!pinnedIcon) return;

            const overIcon = pinnedIcon.matches(':hover');
            const overTooltip = tooltip.matches(':hover');

            if (!overIcon && !overTooltip) {
                tooltip.style.display = 'none';
                pinnedIcon = null;
                setBodyFlag(false);
            }
        }, immediate ? 0 : 100);
    }

    let enterTimer = null;

    function handleEnter(icon) {
        clearTimeout(enterTimer);
        if (lockSwitch) return;
        if (pinnedIcon === icon) return;

        enterTimer = setTimeout(() => {
            pinnedIcon = icon;
            showTooltipForIcon(icon);
        }, 40);
    }

    iconContainer.addEventListener('mouseover', (e) => {
        const icon = e.target.closest('.map-icon');
        if (!icon) return;
        const from = e.relatedTarget;
        if (from && icon.contains(from)) return;
        handleEnter(icon);
    });

    iconContainer.addEventListener('mouseout', (e) => {
        const icon = e.target.closest('.map-icon');
        if (!icon) return;
        const to = e.relatedTarget;
        if (to && icon.contains(to)) return;
        if (icon === pinnedIcon) {
            scheduleHide();
        }
    });

    tooltip.addEventListener('mouseenter', () => {
        clearTimeout(hideTimeout);
        lockSwitch = true;
    });
    tooltip.addEventListener('mouseleave', () => {
        lockSwitch = false;
        scheduleHide();
    });

    tooltip.addEventListener('click', (e) => {
        const button = e.target.closest('button[data-action]');
        if (!button) return;

        if (pinnedIcon) {
            const instanceId = pinnedIcon.dataset.instanceId;
            const action = button.dataset.action;

            if (instanceId && action) {
                console.log(`Sende Aktion: ${action}, ID: ${instanceId}`);

                postData('/api/action', {
                    action: action,
                    instanceId: parseInt(instanceId)
                });

                tooltip.style.display = 'none';
                pinnedIcon = null;
                setBodyFlag(false);
            }
        }
    });
}