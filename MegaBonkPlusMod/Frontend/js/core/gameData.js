import {getAllItems} from '../hooks/actions/itemHook.js';
import {getAllWeapons} from '../hooks/actions/weaponDataHook.js';
import {getAllTomes} from '../hooks/actions/tomeDataHook.js';

class GameDataService {
    constructor() {
        this.items = [];
        this.weapons = [];
        this.tomes = [];
        this.isReady = false;
    }

    async initialize() {
        if (this.isReady) return;

        let attempts = 0;
        console.log('GameDataService: Waiting for static data...');

        while (!this.isReady) {
            const [itemsData, weaponsData, tomesData] = await Promise.all([
                getAllItems(true),
                getAllWeapons(true),
                getAllTomes(true)
            ]);

            if (itemsData?.length > 0 && weaponsData?.length > 0 && tomesData?.length > 0) {
                this.items = itemsData;
                this.weapons = weaponsData;
                this.tomes = tomesData;
                this.isReady = true;
                console.log(`GameDataService: Loaded ${this.items.length} Items, ${this.weapons.length} Weapons, ${this.tomes.length} Tomes.`);
            } else {
                attempts++;
                if (attempts % 5 === 0) {
                    console.log('GameDataService: Not ready yet, retrying...');
                }
                await new Promise(resolve => setTimeout(resolve, 1000));
            }
        }
    }

    getItems() {
        return this.items;
    }

    getWeapons() {
        return this.weapons;
    }

    getTomes() {
        return this.tomes;
    }
}

export const gameData = new GameDataService();