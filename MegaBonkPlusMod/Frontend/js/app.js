import {fetchData} from './services/apiService.js';

import {
    renderBossSpawner,
    renderChargeShrines,
    renderChests,
    renderGreedShrines,
    renderMinimap,
    renderPlayer,
    renderShadyGuys,
    renderSimpleShrine
} from './services/renderService.js';


const ENDPOINTS_TO_TRACK = {
    player: {
        endpoint: '/api/tracker/player',
        renderer: renderPlayer
    },
    shadyGuys: {
        endpoint: '/api/tracker/shadyguys',
        renderer: renderShadyGuys
    },
    chests: {
        endpoint: '/api/tracker/chests',
        renderer: renderChests
    },
    bossSpawner: {
        endpoint: '/api/tracker/bossspawner',
        renderer: renderBossSpawner
    },
    chargeShrines: {
        endpoint: '/api/tracker/shrines/charge',
        renderer: renderChargeShrines
    },
    greedShrines: {
        endpoint: '/api/tracker/shrines/greed',
        renderer: renderGreedShrines
    },
    // Für die "einfachen" Schreine rufen wir den generischen Renderer auf:
    cursedShrines: {
        endpoint: '/api/tracker/shrines/cursed',
        renderer: (data) => renderSimpleShrine('cursed-shrines-data', 'Cursed-Schreine', data)
    },
    magnetShrines: {
        endpoint: '/api/tracker/shrines/magnet',
        renderer: (data) => renderSimpleShrine('magnet-shrines-data', 'Magnet-Schreine', data)
    },
    maoiShrines: {
        endpoint: '/api/tracker/shrines/maoi',
        renderer: (data) => renderSimpleShrine('maoi-shrines-data', 'Maoi-Schreine', data)
    },
    minimap: {
        endpoint: '/api/stream/minimap',
        renderer: renderMinimap
    }
};


async function updateDashboard() {
    const fetchPromises = Object.values(ENDPOINTS_TO_TRACK)
        .map(config => fetchData(config.endpoint));

    const allData = await Promise.all(fetchPromises);

    Object.keys(ENDPOINTS_TO_TRACK).forEach((key, index) => {
        const config = ENDPOINTS_TO_TRACK[key];
        const data = allData[index];
        config.renderer(data);
    });
}

console.log("Dashboard gestartet...");

setInterval(updateDashboard, 1000);

updateDashboard();