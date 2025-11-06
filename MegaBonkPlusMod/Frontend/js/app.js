import { fetchData } from './services/apiService.js';

import {
    renderPlayer,
    renderShadyGuys,
    renderChests,
    renderBossSpawner,
    renderChargeShrines,
    renderGreedShrines,

    renderMinimap,
    renderTrackers,
    
    findItemAtPosition,
    getMousePos
} from './services/renderService.js';


let lastDataMap = {};
let hoveredItem = null;
let mainCanvas = null;

const ENDPOINTS_TO_TRACK = {
    minimap: {
        endpoint: '/api/stream/minimap',
        mapRenderer: renderMinimap
    },

    greedShrines: {
        endpoint: '/api/tracker/shrines/greed',
        renderConfigSelector: (item, isHovered) => {
            if (item.customProperties.done) {
                return { type: 'none' };
            }
            return {
                type: 'dot',
                color: '#e96228',
                size: isHovered ? 4 : 2
            };
        }
    },
    
    chests: {
        endpoint: '/api/tracker/chests',
        renderConfigSelector: (item, isHovered) => {
            const type = item.customProperties.type?.toLowerCase() ?? 'normal';
            return {
                type: 'image',
                path: `/images/map_objects/chest_${type}.png`,
                size: isHovered ? 24 : 8
            };
        }
    },

    cursedShrines: {
        endpoint: '/api/tracker/shrines/cursed',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/cursed.png',
            size: isHovered ? 48 : 16
        })
    },

    challengeShrines: {
        endpoint: '/api/tracker/shrines/challenge',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/challenge.png',
            size: isHovered ? 48 : 16
        })
    },

    magnetShrines: {
        endpoint: '/api/tracker/shrines/magnet',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/magnet.png',
            size: isHovered ? 48 : 16
        })
    },

    microwaves: {
        endpoint: '/api/tracker/microwave',
        renderConfigSelector: (item, isHovered) => {
            const rarity = item.customProperties.rarity?.toLowerCase() ?? 'common';
            return {
                type: 'image',
                path: `/images/map_objects/microwave_${rarity}.png`,
                size: isHovered ? 30 : 24
            };
        }
    },

    moaiShrines: {
        endpoint: '/api/tracker/shrines/maoi',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/maoi.png',
            size: isHovered ? 48 : 16
        })
    },
    
    
    chargeShrines: {
        endpoint: '/api/tracker/shrines/charge',
        renderConfigSelector: (item, isHovered) => {
            if (item.customProperties.completed) {
                return { type: 'none' };
            }
            const baseName = item.customProperties.isGolden ? 'charge_gold' : 'charge_normal';
            return {
                type: 'image',
                path: `/images/map_objects/${baseName}.png`,
                size: isHovered ? 24 : 24
            };
        }
    },

    shadyGuys: {
        endpoint: '/api/tracker/shadyguys',
        textRenderer: renderShadyGuys,
        renderConfigSelector: (item, isHovered) => {
            const rarity = item.customProperties.rarity?.toLowerCase() ?? 'common';
            return {
                type: 'image',
                path: `/images/map_objects/shady_${rarity}.png`,
                size: isHovered ? 30 : 24
            };
        }
    },
    bossSpawner: {
        endpoint: '/api/tracker/bossspawner',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/bossspawner.png',
            size: isHovered ? 48 : 16
        })
    },
    player: {
        endpoint: '/api/tracker/player',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: `/images/characters/${item.customProperties.character.toString().toLowerCase()}.png`,
            size: isHovered ? 36 : 18
        })
    }
};


async function updateDashboard() {

    const allKeys = Object.keys(ENDPOINTS_TO_TRACK);

    const allData = await Promise.all(
        allKeys.map(key => fetchData(ENDPOINTS_TO_TRACK[key].endpoint))
    );

    const dataMap = {};
    allKeys.forEach((key, index) => { dataMap[key] = allData[index]; });
    lastDataMap = dataMap;

    let ctx = null;
    if (dataMap['minimap']) {
        ctx = renderMinimap(dataMap['minimap']);
        if (ctx && !mainCanvas) {
            mainCanvas = ctx.canvas;
            setupMouseListeners(mainCanvas);
        }
    }
    
    allKeys.forEach(key => {
        const config = ENDPOINTS_TO_TRACK[key];
        const data = dataMap[key];

        if (config.textRenderer) {
            config.textRenderer(data);
        }
        
        if (ctx && key !== 'minimap') {
            renderTrackers(ctx, data, config.renderConfigSelector, hoveredItem);
        }
    });
}

function setupMouseListeners(canvas) {
    canvas.addEventListener('mousemove', (evt) => {
        const pos = getMousePos(canvas, evt);
        hoveredItem = findItemAtPosition(pos, lastDataMap, ENDPOINTS_TO_TRACK, canvas);
    });

    canvas.addEventListener('mouseout', () => {
        hoveredItem = null;
    });
}


console.log("Dashboard gestartet...");
setInterval(updateDashboard, 1000);
updateDashboard();