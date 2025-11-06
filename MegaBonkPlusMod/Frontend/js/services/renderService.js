function getElem(id) {
    const elem = document.getElementById(id);
    if (!elem) {
        console.error(`Render-Fehler: Element mit ID '${id}' nicht gefunden.`);
    }
    return elem;
}

function createPositionString(pos) {
    if (!pos) return "Position: Unbekannt\n";
    return `Position: (X: ${pos.x}, Y: ${pos.y}, Z: ${pos.z})\n`;
}

export function renderPlayer(data) {
    const elem = getElem('player-data');
    if (!elem) return;
    if (data.count === 0) {
        elem.textContent = "Spieler nicht gefunden.";
        return;
    }
    const player = data.items[0];
    elem.textContent = createPositionString(player.position);
    elem.textContent += `Charakter: ${player.customProperties.character}\n`;
}

export function renderShadyGuys(data) {
    const elem = getElem('shadyguys-data');
    if (!elem) return;
    if (data.count === 0) {
        elem.textContent = "Keine Shady Guys gefunden.";
        return;
    }
    let html = `Anzahl gefunden: ${data.count}\n\n`;
    data.items.forEach((guy, index) => {
        html += `--- Guy #${index + 1} ---\n`;
        html += createPositionString(guy.position);
        html += `Rarität: ${guy.customProperties.rarity}\n`;
        html += `Items: ${guy.customProperties.itemNames.join(', ')}\n`;
        html += `Preise: ${guy.customProperties.itemPrices.join(', ')}\n`;
    });
    elem.textContent = html;
}

export function renderChests(data) {
    const elem = getElem('chests-data');
    if (!elem) return;
    if (data.count === 0) {
        elem.textContent = "Keine Kisten gefunden.";
        return;
    }
    let html = `Anzahl gefunden: ${data.count}\n\n`;
    data.items.forEach((chest) => {
        html += `${chest.customProperties.type} Kiste - ${createPositionString(chest.position)}`;
    });
    elem.textContent = html;
}

export function renderBossSpawner(data) {
    const elem = getElem('bossspawner-data');
    if (!elem) return;
    if (data.count === 0) {
        elem.textContent = "Kein Boss-Spawner gefunden.";
        return;
    }
    const spawner = data.items[0];
    elem.textContent = `Spawner gefunden!\n`;
    elem.textContent += createPositionString(spawner.position);
}

export function renderChargeShrines(data) {
    const elem = getElem('charge-shrines-data');
    if (!elem) return;
    if (data.count === 0) {
        elem.textContent = "Keine Charge-Schreine gefunden.";
        return;
    }
    let html = `Charge-Schreine: ${data.count}\n\n`;
    data.items.forEach((shrine) => {
        html += `Golden: ${shrine.customProperties.isGolden} | `;
        html += `Abgeschlossen: ${shrine.customProperties.completed}\n`;
        html += createPositionString(shrine.position);
    });
    elem.textContent = html;
}

export function renderGreedShrines(data) {
    const elem = getElem('greed-shrines-data');
    if (!elem) return;
    if (data.count === 0) {
        elem.textContent = "Keine Greed-Schreine gefunden.";
        return;
    }
    let html = `Greed-Schreine: ${data.count}\n\n`;
    data.items.forEach((shrine) => {
        html += `Abgeschlossen: ${shrine.customProperties.done}\n`;
        html += createPositionString(shrine.position);
    });
    elem.textContent = html;
}

export function renderSimpleShrine(elemId, name, data) {
    const elem = getElem(elemId);
    if (!elem) return;
    if (data.count === 0) {
        elem.textContent = `Keine ${name} gefunden.`;
        return;
    }
    let html = `${name}: ${data.count}\n\n`;
    data.items.forEach((shrine, index) => {
        html += `Schrein #${index + 1}\n`;
        html += createPositionString(shrine.position);
    });
    elem.textContent = html;
}

export function renderMinimap(data) {
    const canvas = getElem('minimap-canvas');
    if (!canvas) return;

    if (data.count === 0 || !data.items[0] || !data.items[0].customProperties.rawPixelData) {
        canvas.style.display = 'none';
        return;
    }

    canvas.style.display = 'block';

    const props = data.items[0].customProperties;
    const width = props.width;
    const height = props.height;

    if (canvas.width !== width) canvas.width = width;
    if (canvas.height !== height) canvas.height = height;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    try {
        const binaryString = atob(props.rawPixelData);
        const len = binaryString.length;
        const bytes = new Uint8Array(len);
        for (let i = 0; i < len; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        const imageData = new ImageData(new Uint8ClampedArray(bytes.buffer), width, height);
        ctx.putImageData(imageData, 0, 0);

    } catch (e) {
        console.error("Fehler beim Malen der Minimap auf das Canvas:", e);
    }
}