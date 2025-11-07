import { getElem } from './utils.js';
import { MAP_SCALE, WORLD_OFFSET_X, WORLD_OFFSET_Z } from '../config.js';

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

    return { leftPercent, topPercent };
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

        const filterId = config.filterToggleId;
        if (filterId && filterStates[filterId] === false) {
            continue;
        }

        if (data.count === 0) continue;

        data.items.forEach(item => {
            const renderInfo = config.renderConfigSelector(item);
            if (renderInfo.type === 'none') return;

            const tooltipHtml = config.getTooltipHtml(item).replace(/"/g, '&quot;');
            const { leftPercent, topPercent } = worldToCanvasPercentages(item.position, canvasWidth, canvasHeight);

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
                    />`;
            } else if (renderInfo.type === 'dot') {
                style += ` background-color: ${renderInfo.color};`;
                allIconsHtml +=
                    `<div class="map-icon" 
                         style="${style}"
                         data-tooltip-html="${tooltipHtml}"
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

    iconContainer.addEventListener('mousemove', (e) => {
        const targetIcon = e.target.closest('.map-icon');

        if (targetIcon) {
            const html = targetIcon.dataset.tooltipHtml;
            if (html) {
                tooltip.innerHTML = html;
                tooltip.style.display = 'block';
                tooltip.style.left = (e.clientX + 15) + 'px';
                tooltip.style.top = (e.clientY + 15) + 'px';
            }
        } else {
            tooltip.style.display = 'none';
        }
    });

    iconContainer.addEventListener('mouseleave', () => {
        tooltip.style.display = 'none';
    });
}