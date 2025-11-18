import {renderMinimap} from '../features/map/mapRenderer.js';

export const MAP_SCALE = 2.35;
export const WORLD_OFFSET_X = 0;
export const WORLD_OFFSET_Z = 0;

export const MAP_DATA = {
    forest: {
        mapScale: 2.35,
        worldOffsetX: 0,
        worldOffsetZ: 0
    },
    desert: {
        mapScale: 3.8,
        worldOffsetX: 0,
        worldOffsetZ: 0
    }
}

export function getMapData(mapName) {
    const emptyData = { mapScale: MAP_SCALE, worldOffsetX: WORLD_OFFSET_X, worldOffsetZ: WORLD_OFFSET_Z};
    if (!mapName) {
        return emptyData;
    }

    const key = mapName.toLowerCase();
    const mapConfig = MAP_DATA[key];

    return mapConfig ?? emptyData;
}

export const ENDPOINTS_TO_TRACK = {
    minimap: {
        mapRenderer: renderMinimap
    },

    greedShrines: {
        renderConfigSelector: (item, isHovered) => {
            return {
                type: 'dot',
                color: '#e96228',
                size: isHovered ? 4 : 2
            };
        },
        getTooltipHtml: (item) => {
            return `
                <div class="tooltip-content">
                    <div class="tooltip-icon">
                        <img src="/images/map_objects/greed.png" alt="Greed Shrine">
                    </div>
                    <div class="tooltip-right">
                        <div class="tooltip-header">
                            <div class="tooltip-title">Greed Shrine</div>
                            <div class="tooltip-meta">Enemys</div>
                        </div>
                        <div class="tooltip-body">
                            Increases the Difficulty by 5%
                        </div>
                    </div>
                    <div class="tooltip-actions">
                        <button class="tooltip-button primary" data-action="teleport">Teleport</button>
                        <button class="tooltip-button" data-action="interact">Interact</button>
                    </div>
                </div>
            `;
        }
    },

    chests: {
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
                <div class="tooltip-content">
                    <div class="tooltip-icon">
                        <img src="/images/map_objects/chest_${type.toLowerCase()}.png" alt="Chest">
                    </div>
                    <div class="tooltip-right">
                        <div class="tooltip-header">
                            <div class="tooltip-title">${type ?? ""} Chest</div>
                            <div class="tooltip-meta">Items</div>
                        </div>
                        <div class="tooltip-body">
                            Buy to recieve a random item
                        </div>
                    </div>
                    <div class="tooltip-actions">
                        <button class="tooltip-button primary" data-action="teleport">Teleport</button>
                        <button class="tooltip-button" data-action="interact">Interact</button>
                    </div>
                </div>
            `;
        }
    },

    cursedShrines: {
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/cursed.png',
            size: isHovered ? 16 : 12
        }),
        getTooltipHtml: (item) => {
            return `
                <div class="tooltip-content">
                    <div class="tooltip-icon">
                        <img src="/images/map_objects/cursed.png" alt="Cursed Shrine">
                    </div>
                    <div class="tooltip-right">
                        <div class="tooltip-header">
                            <div class="tooltip-title">Cursed Shrine</div>
                            <div class="tooltip-meta">Enemies</div>
                        </div>
                        <div class="tooltip-body">
                            Interact with this Shrine to Spawn +1 Boss
                        </div>
                    </div>
                    <div class="tooltip-actions">
                        <button class="tooltip-button primary" data-action="teleport">Teleport</button>
                        <button class="tooltip-button" data-action="interact">Interact</button>
                    </div>
                </div>
            `;
        }
    },

    challengeShrines: {
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/challenge.png',
            size: isHovered ? 20 : 16
        }),
        getTooltipHtml: (item) => {
            return `
                <div class="tooltip-content">
                    <div class="tooltip-icon">
                        <img src="/images/map_objects/challenge.png" alt="Challenge Shrine">
                    </div>
                    <div class="tooltip-right">
                        <div class="tooltip-header">
                            <div class="tooltip-title">Challenge Shrine</div>
                            <div class="tooltip-meta">Enemies</div>
                        </div>
                        <div class="tooltip-body">
                            Spawns elite enemies that drop a free chest on death
                        </div>
                    </div>
                    <div class="tooltip-actions">
                        <button class="tooltip-button primary" data-action="teleport">Teleport</button>
                        <button class="tooltip-button" data-action="interact">Interact</button>
                    </div>
                </div>
            `;
        }
    },

    magnetShrines: {
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/magnet.png',
            size: isHovered ? 20 : 16
        }),
        getTooltipHtml: (item) => {
            return `
                <div class="tooltip-content">
                    <div class="tooltip-icon">
                        <img src="/images/map_objects/magnet.png" alt="Magnet Shrine">
                    </div>
                    <div class="tooltip-right">
                        <div class="tooltip-header">
                            <div class="tooltip-title">Magnet Shrine</div>
                            <div class="tooltip-meta">Level</div>
                        </div>
                        <div class="tooltip-body">
                            Interact to pull all dropped XP across the map towards you
                        </div>
                    </div>
                    <div class="tooltip-actions">
                        <button class="tooltip-button primary" data-action="teleport">Teleport</button>
                        <button class="tooltip-button" data-action="interact">Interact</button>
                    </div>
                </div>
            `;
        }
    },

    microwaves: {
        renderConfigSelector: (item, isHovered) => {
            const rarity = item.customProperties.rarity?.toLowerCase() ?? 'common';
            return {
                type: 'image',
                path: `/images/map_objects/microwave_${rarity}.png`,
                size: isHovered ? 20 : 16
            };
        },
        getTooltipHtml: (item) => {
            const type = item.customProperties.rarity ?? 'Common';
            return `
                <div class="tooltip-content">
                    <div class="tooltip-icon">
                        <img src="/images/map_objects/microwave_${type.toLowerCase()}.png" alt="Microwave">
                    </div>
                    <div class="tooltip-right">
                        <div class="tooltip-header">
                            <div class="tooltip-title">${type ?? ""} Microwave</div>
                            <div class="tooltip-meta">Items</div>
                        </div>
                        <div class="tooltip-body">
                            Clone one ${type ?? ""} Item by sacrificing another. You need at least three of the same rarity
                        </div>
                    </div>
                    <div class="tooltip-actions">
                        <button class="tooltip-button primary" data-action="teleport">Teleport</button>
                        <button class="tooltip-button" data-action="interact">Interact</button>
                    </div>
                </div>
            `;
        }
    },

    moaiShrines: {
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/maoi.png',
            size: isHovered ? 20 : 16
        }),
        getTooltipHtml: (item) => {
            return `
                <div class="tooltip-content">
                    <div class="tooltip-icon">
                        <img src="/images/map_objects/maoi.png" alt="Moai Shrine">
                    </div>
                    <div class="tooltip-right">
                        <div class="tooltip-header">
                            <div class="tooltip-title">Moai Shrine</div>
                            <div class="tooltip-meta">Items</div>
                        </div>
                        <div class="tooltip-body">
                            Interact select a random Item from the Shrine for free
                        </div>
                    </div>
                    <div class="tooltip-actions">
                        <button class="tooltip-button primary" data-action="teleport">Teleport</button>
                        <button class="tooltip-button" data-action="interact">Interact</button>
                    </div>
                </div>
            `;
        }
    },

    chargeShrines: {
        renderConfigSelector: (item, isHovered) => {
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
                <div class="tooltip-content">
                    <div class="tooltip-icon">
                        <img src="/images/map_objects/${imgName}.png" alt="Charge Shrine">
                    </div>
                    <div class="tooltip-right">
                        <div class="tooltip-header">
                            <div class="tooltip-title">${baseName}</div>
                            <div class="tooltip-meta">Stats</div>
                        </div>
                        <div class="tooltip-body">
                            Stand in the range of shrine to recieve a Stat Boost
                        </div>
                    </div>
                    <div class="tooltip-actions">
                        <button class="tooltip-button primary" data-action="teleport">Teleport</button>
                        <button class="tooltip-button" data-action="interact">Complete</button>
                    </div>
                </div>
            `;
        }
    },

    shadyGuys: {
        renderConfigSelector: (item, isHovered) => {
            const rarity = item.customProperties.rarity?.toLowerCase() ?? 'common';
            return {
                type: 'image',
                path: `/images/map_objects/shady_${rarity}.png`,
                size: isHovered ? 28 : 24
            };
        },
        getTooltipHtml: (item) => {
            const props = item.customProperties || {};
            const rarity = props.rarity ?? 'Common';
            const names = Array.isArray(props.itemNames) ? props.itemNames : [];
            const prices = Array.isArray(props.itemPrices) ? props.itemPrices : [];
            const ids = Array.isArray(props.itemIds) ? props.itemIds : [];

            const offersHtml = names.slice(0, 3).map((rawName, i) => {
                const id = ids[i] != null ? ids[i] : '-';
                const price = prices[i] != null ? prices[i] : '-';
                const displayName = rawName;
                const icon = `/images/items/${id}.png`;
                return `
                        <div class="offer-item">
                            <img src="${icon}" alt="${displayName}">
                            <div class="offer-name" title="${displayName}">${displayName}</div>
                            <div class="offer-price">${price}g</div>
                        </div>
                    `;
            }).join('') || `
                    <div class="offer-item">
                        <div class="offer-name" title="No offers">No offers</div>
                        <div class="offer-price">-</div>
                    </div>
                `;

            return `
                    <div class="tooltip-content" data-tooltip-wide="1">
                        <div class="tooltip-icon">
                            <img src="/images/map_objects/shady_${(rarity || 'Common').toLowerCase()}.png" alt="Shady Guy">
                        </div>

                        <div class="tooltip-right">
                            <div class="tooltip-header">
                                <div class="tooltip-title">${rarity} Shady Guy</div>
                                <div class="tooltip-meta">Vendor</div>
                            </div>

                            <div class="tooltip-body">
                                Buy one ${rarity} item from this shady vendor.
                            </div>

                            <div class="tooltip-offers">
                                <h5>Offers</h5>
                                <div class="offer-list">
                                    ${offersHtml}
                                </div>
                            </div>
                        </div>

                        <div class="tooltip-actions">
                            <button class="tooltip-button primary" data-action="teleport">Teleport</button>
                            <button class="tooltip-button" data-action="interact">Open Shop</button>
                        </div>
                    </div>
                `;
        }
    },

    bosses: {
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/characters/boss.png',
            size: isHovered ? 16 : 12
        }),
        getTooltipHtml: (item) => {
            return `
                <div class="tooltip-content">
                    <div class="tooltip-icon">
                        <img src="/images/characters/boss.png" alt="Boss">
                    </div>
                    <div class="tooltip-right">
                        <div class="tooltip-header">
                            <div class="tooltip-title">Boss</div>
                            <div class="tooltip-meta">Enemy</div>
                        </div>
                    </div>
                    <div class="tooltip-actions">
                        <button class="tooltip-button primary" data-action="teleport">Teleport</button>
                    </div>
                </div>
            `;
        }
    },

    bossSpawner: {
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: '/images/map_objects/bossspawner.png',
            size: isHovered ? 20 : 16
        }),
        getTooltipHtml: (item) => {
            return `
                <div class="tooltip-content">
                    <div class="tooltip-icon">
                        <img src="/images/map_objects/bossspawner.png" alt="Boss Spawner">
                    </div>
                    <div class="tooltip-right">
                        <div class="tooltip-header">
                            <div class="tooltip-title">Boss Spawner</div>
                            <div class="tooltip-meta">Final Wave</div>
                        </div>
                        <div class="tooltip-body">
                            Interact to spawn the final boss of the Stage and trigger the Final Wave
                        </div>
                    </div>
                    <div class="tooltip-actions">
                        <button class="tooltip-button primary" data-action="teleport">Teleport</button>
                        <button class="tooltip-button" data-action="interact">Interact</button>
                    </div>
                </div>
            `;
        }
    },

    player: {
        textRenderer: 'renderPlayer',
        renderConfigSelector: (item, isHovered) => ({
            type: 'image',
            path: `/images/characters/${item.customProperties.character.toString().toLowerCase()}.png`,
            size: isHovered ? 22 : 18
        }),
        getTooltipHtml: (item) => {
            const props = item.customProperties || {};
            const ch = props.character ?? 'Unknown';
            const slug = typeof ch === 'string' ? ch.toLowerCase() : 'placeholder';
            const lvl = props.level ?? 0;
            return `
                    <div class="tooltip-content">
                        <div class="tooltip-icon">
                            <img src="/images/characters/${slug}.png" alt="${ch}">
                        </div>
                        <div class="tooltip-right">
                            <div class="tooltip-header">
                                <div class="tooltip-title">${ch} (You)</div>
                                <div class="tooltip-meta">Level ${lvl}</div>
                            </div>

                        </div>
                    </div>
                `;
        }
    }
};