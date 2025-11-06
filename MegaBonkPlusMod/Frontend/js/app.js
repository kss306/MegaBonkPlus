import {fetchData} from './services/apiService.js';

import {
    // text-painter
    renderPlayer,
    renderShadyGuys,
    renderChests,
    renderBossSpawner,
    renderChargeShrines,
    renderGreedShrines,
    renderSimpleShrine,

    // map-painter
    renderMinimap,
    renderTrackerDots
} from './services/renderService.js';


const ENDPOINTS_TO_TRACK = {

    minimap: {
        endpoint: '/api/stream/minimap',
        mapRenderer: renderMinimap
    },

    player: {
        endpoint: '/api/tracker/player',
        textRenderer: renderPlayer,
        dotColor: 'rgb(33,0,255)'
    },
    chests: {
        endpoint: '/api/tracker/chests',
        textRenderer: renderChests,
        dotColor: 'rgba(255,255,0,0)'
    },
    shadyGuys: {
        endpoint: '/api/tracker/shadyguys',
        textRenderer: renderShadyGuys,
        dotColor: 'rgba(255,255,0,0)'
    },
    bossSpawner: {
        endpoint: '/api/tracker/bossspawner',
        textRenderer: renderBossSpawner,
        dotColor: 'rgb(147,5,0)'
    },
    chargeShrines: {
        endpoint: '/api/tracker/shrines/charge',
        textRenderer: renderChargeShrines,
        dotColor: 'rgba(255,255,0,0)'
    },
    greedShrines: {
        endpoint: '/api/tracker/shrines/greed',
        textRenderer: renderGreedShrines,
        dotColor: 'rgba(255,255,0,0)'
    },
    cursedShrines: {
        endpoint: '/api/tracker/shrines/cursed',
        textRenderer: (data) => renderSimpleShrine('cursed-shrines-data', 'Cursed-Schreine', data),
        dotColor: 'rgba(255,255,0,0)'
    },
    magnetShrines: {
        endpoint: '/api/tracker/shrines/magnet',
        textRenderer: (data) => renderSimpleShrine('magnet-shrines-data', 'Magnet-Schreine', data),
        dotColor: 'rgba(255,255,0,0)'
    },
    maoiShrines: {
        endpoint: '/api/tracker/shrines/maoi',
        textRenderer: (data) => renderSimpleShrine('maoi-shrines-data', 'Maoi-Schreine', data),
        dotColor: 'rgba(255,255,0,0)'
    }
};


async function updateDashboard() {

    const allConfigs = Object.values(ENDPOINTS_TO_TRACK);

    const allData = await Promise.all(
        allConfigs.map(config => fetchData(config.endpoint))
    );

    const mapConfigIndex = allConfigs.findIndex(c => c.mapRenderer);
    let ctx = null;

    if (mapConfigIndex > -1) {
        const mapData = allData[mapConfigIndex];
        const mapConfig = allConfigs[mapConfigIndex];

        ctx = mapConfig.mapRenderer(mapData);
    }

    allData.forEach((data, index) => {
        const config = allConfigs[index];

        if (config.textRenderer) {
            config.textRenderer(data);
        }

        if (ctx && config.dotColor) {
            renderTrackerDots(ctx, data, config.dotColor);
        }
    });
}

console.log("Dashboard gestartet...");
setInterval(updateDashboard, 1000);
updateDashboard();