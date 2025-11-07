import {renderMinimap} from './services/mapService.js';

export const MAP_SCALE = 2.3;
export const WORLD_OFFSET_X = 15;
export const WORLD_OFFSET_Z = 0.0;

export const ENDPOINTS_TO_TRACK = {
    minimap: {
        endpoint: '/api/stream/minimap',
        mapRenderer: renderMinimap
    },

    greedShrines: {
        endpoint: '/api/tracker/shrines/greed',
        renderConfigSelector: (item, isHovered) => {
            if (item.customProperties.done) {
                return {type: 'none'};
            }
            return {
                type: 'dot',
                color: '#e96228',
                size: isHovered ? 4 : 2
            };
        },
        getTooltipHtml: (item) => {
            return `
                <h4>Greed Shrine</h4>
                <img src="/images/map_objects/greed.png" alt="Greed Shrine">
                <p>Increases the Difficulty by 5%</p>
            `;
        }
    },

    chests: {
        endpoint: '/api/tracker/chests',
        renderConfigSelector: (item, isHovered) => {
            const type = item.customProperties.type?.toLowerCase() ?? 'normal';
            return {
                type: 'image',
                path: `/images/map_objects/chest_${type}.png`,
                size: isHovered ? 10 : 8
            };
        },
        getTooltipHtml: (item) => {
            const type = item.customProperties.type ?? 'Normal';
            return `
                <h4>${type ?? ""} Chest</h4>
                <img src="/images/map_objects/chest_${type.toLowerCase()}.png" alt="Chest">
                <p>Buy to recieve a random item</p>
            `;
        }
    },

    cursedShrines: {
        endpoint: '/api/tracker/shrines/cursed',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/cursed.png',
            size: isHovered ? 16 : 12
        }),
        getTooltipHtml: (item) => {
            return `
                <h4>Cursed Shrine</h4>
                <img src="/images/map_objects/cursed.png" alt="Cursed Shrine">
                <p>Interact with this Shrine to Spawn +1 Boss</p>
            `;
        }
    },

    challengeShrines: {
        endpoint: '/api/tracker/shrines/challenge',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/challenge.png',
            size: isHovered ? 20 : 16
        }),
        getTooltipHtml: (item) => {
            return `
                <h4>Challenge Shrine</h4>
                <img src="/images/map_objects/challenge.png" alt="Challenge Shrine">
                <p>Spawns elite enemies that drop a free chest on death</p>
            `;
        }
    },

    magnetShrines: {
        endpoint: '/api/tracker/shrines/magnet',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/magnet.png',
            size: isHovered ? 20 : 16
        }),
        getTooltipHtml: (item) => {
            return `
                <h4>Magnet Shrine</h4>
                <img src="/images/map_objects/magnet.png" alt="Magnet Shrine">
                <p>Interact to pull all dropped XP across the map towards you</p>
            `;
        }
    },

    microwaves: {
        endpoint: '/api/tracker/microwave',
        renderConfigSelector: (item, isHovered) => {
            const rarity = item.customProperties.rarity?.toLowerCase() ?? 'common';
            return {
                type: 'image',
                path: `/images/map_objects/microwave_${rarity}.png`,
                size: isHovered ? 20 : 16
            };
        },
        getTooltipHtml: (item) => {
            const type = item.customProperties.type ?? 'Common';
            return `
                <h4>${type ?? ""} Microwave</h4>
                <img src="/images/map_objects/microwave_${type.toLowerCase()}.png" alt="Microwave">
                <p>Clone one ${type ?? ""} Item by sacrificing another. You need at least three of the same rarity</p>
            `;
        }
    },

    moaiShrines: {
        endpoint: '/api/tracker/shrines/maoi',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/maoi.png',
            size: isHovered ? 20 : 16
        }),
        getTooltipHtml: (item) => {
            return `
                <h4>Moai Shrine</h4>
                <img src="/images/map_objects/maoi.png" alt="Moai Shrine">
                <p>Interact select a random Item from the Shrine for free</p>
            `;
        }
    },

    chargeShrines: {
        endpoint: '/api/tracker/shrines/charge',
        renderConfigSelector: (item, isHovered) => {
            if (item.customProperties.completed) {
                return {type: 'none'};
            }
            const baseName = item.customProperties.isGolden ? 'charge_gold' : 'charge_normal';
            return {
                type: 'image',
                path: `/images/map_objects/${baseName}.png`,
                size: isHovered ? 26 : 22
            };
        },
        getTooltipHtml: (item) => {
            const imgName = item.customProperties.isGolden ? 'charge_gold' : 'charge_normal';
            const baseName = item.customProperties.isGolden ? 'Golden Charge Shrine' : 'Charge Shrine';
            return `
                <h4>${baseName ?? "Charge Shrine"}</h4>
                <img src="/images/map_objects/${imgName}.png" alt="Charge Shrine">
                <p>Stand in the range of shrine to recieve a Stat Boost</p>
            `;
        }
    },

    shadyGuys: {
        endpoint: '/api/tracker/shadyguys',
        renderConfigSelector: (item, isHovered) => {
            const rarity = item.customProperties.rarity?.toLowerCase() ?? 'common';
            return {
                type: 'image',
                path: `/images/map_objects/shady_${rarity}.png`,
                size: isHovered ? 28 : 24
            };
        },
        getTooltipHtml: (item) => {
            const rarity = item.customProperties.rarity ?? 'Common';
            return `
                <h4>${rarity ?? ""} Shady Guy</h4>
                <img src="/images/map_objects/shady_${rarity.toLowerCase()}.png" alt="Shady Guy">
                <p>Buy one ${rarity ?? ""} Item from this shady vendor</p>
            `;
        }
    },

    bossSpawner: {
        endpoint: '/api/tracker/bossspawner',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/bossspawner.png',
            size: isHovered ? 20 : 16
        }),
        getTooltipHtml: (item) => {
            return `
                <h4>Boss Spawner</h4>
                <img src="/images/map_objects/bossspawner.png" alt="Boss Spawner">
                <p>Interact to spawn the final boss of the Stage and trigger the Final Wave</p>
            `;
        }
    },

    player: {
        endpoint: '/api/tracker/player',
        textRenderer: 'renderPlayer',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: `/images/characters/${item.customProperties.character.toString().toLowerCase()}.png`,
            size: isHovered ? 22 : 18
        }),
        getTooltipHtml: (item) => {
            const props = item.customProperties;
            const pos = item.position;
            return `
                <h4>${props.character} (Du)</h4>
                <img src="/images/characters/${props.character.toString().toLowerCase()}.png" alt="${props.character}">
                <p>Position: ${Math.round(pos.x)}, ${Math.round(pos.y)}, ${Math.round(pos.z)}</p>
            `;
        }
    }
};