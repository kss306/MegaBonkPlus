import {getElem} from '../../utils/dom.js';
import {worldToCanvasPercentages} from './mapCoordinates.js';

let lastIconsHtml = null;

export function updateMapIcons(dataMap, configMap, filterStates) {
    const iconContainer = getElem('minimap-icons');
    const canvas = getElem('minimap-canvas');

    if (!iconContainer || !canvas || canvas.width === 0) {
        return;
    }

    const icons = buildIconsHTML(dataMap, configMap, filterStates, canvas.width, canvas.height);

    if (icons === lastIconsHtml) {
        return;
    }
    lastIconsHtml = icons;

    iconContainer.innerHTML = icons;
}


function buildIconsHTML(dataMap, configMap, filterStates, canvasWidth, canvasHeight) {
    let allIconsHtml = '';

    for (const key of Object.keys(dataMap)) {
        if (key === 'minimap' || !configMap[key]) continue;

        const config = configMap[key];
        if (!config.renderConfigSelector) continue;

        const data = dataMap[key];
        const filter = filterStates[key];

        if (!filter || !filter.main || !data || data.count === 0) continue;

        const hasRarities = Object.keys(filter).some(k => k !== 'main');

        data.items.forEach(item => {
            if (hasRarities && shouldFilterByRarity(item, filter)) {
                return;
            }

            const iconHTML = buildIconHTML(item, config, canvasWidth, canvasHeight);
            if (iconHTML) {
                allIconsHtml += iconHTML;
            }
        });
    }

    return allIconsHtml;
}

function shouldFilterByRarity(item, filter) {
    const itemRarity = (item.customProperties?.rarity || 'common').toLowerCase();
    return filter[itemRarity] === false;
}

function buildIconHTML(item, config, canvasWidth, canvasHeight) {
    const renderInfo = config.renderConfigSelector(item);
    if (renderInfo.type === 'none') return null;

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
        return `<img class="map-icon" 
                     src="${renderInfo.path}" 
                     style="${style}"
                     data-tooltip-html="${tooltipHtml}"
                     data-instance-id="${item.instanceId}"
                />`;
    } else if (renderInfo.type === 'dot') {
        style += ` background-color: ${renderInfo.color};`;
        return `<div class="map-icon" 
                     style="${style}"
                     data-tooltip-html="${tooltipHtml}"
                     data-instance-id="${item.instanceId}"
                ></div>`;
    }

    return null;
}