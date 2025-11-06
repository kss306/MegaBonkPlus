const MAP_SCALE = 2.32;

const imageCache = {};

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

function worldToCanvas(worldPos, canvas) {
    const canvasCenterX = canvas.width / 2;
    const canvasCenterY = canvas.height / 2;
    
    const u = (worldPos.x / MAP_SCALE) + canvasCenterX;
    
    const v = (worldPos.z / MAP_SCALE) + canvasCenterY;

    return { u, v };
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


export function renderMinimap(data) {
    const canvas = getElem('minimap-canvas');
    if (!canvas) return null;

    if (data.count === 0 || !data.items[0] || !data.items[0].customProperties.rawPixelData) {
        canvas.style.display = 'none';
        return null;
    }

    canvas.style.display = 'block';
    const props = data.items[0].customProperties;
    const width = props.width;
    const height = props.height;

    if (canvas.width !== width) canvas.width = width;
    if (canvas.height !== height) canvas.height = height;

    const ctx = canvas.getContext('2d');
    if (!ctx) return null;

    try {
        const binaryString = atob(props.rawPixelData);
        const len = binaryString.length;
        const bytes = new Uint8Array(len);
        for (let i = 0; i < len; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        const imageData = new ImageData(new Uint8ClampedArray(bytes.buffer), width, height);
        ctx.putImageData(imageData, 0, 0);

        return ctx;

    } catch (e) {
        console.error("Fehler beim Malen der Minimap auf das Canvas:", e);
        return null;
    }
}

export function renderTrackers(ctx, data, renderConfigSelector, hoveredItem) {
    if (data.count === 0) return;

    data.items.forEach(item => {
        const config = renderConfigSelector(item, item === hoveredItem);

        if (config.type === 'none') {
            return;
        }

        const pos = worldToCanvas(item.position, ctx.canvas);
        const size = config.size ?? 16;
        
        switch (config.type) {
            case 'dot':
                ctx.fillStyle = config.color ?? '#FF00FF';
                ctx.beginPath();
                ctx.rect(pos.u - (size / 2), pos.v - (size / 2), size, size);
                ctx.fill();
                break;

            case 'image':
                renderImage(ctx, pos, config.path, size);
                break;
        }
    });
}

function renderImage(ctx, pos, imagePath, size) {
    if (!imagePath) return;

    if (imageCache[imagePath]) {
        const img = imageCache[imagePath];
        try {
            ctx.save();
            ctx.translate(pos.u, pos.v);
            ctx.scale(1, -1);
            ctx.drawImage(img, -(size / 2), -(size / 2), size, size);
            ctx.restore();
        } catch (e) { ctx.restore(); }
    } else if (imageCache[imagePath] !== null) {
        imageCache[imagePath] = null;
        const img = new Image();
        img.src = imagePath;
        img.onload = () => { imageCache[imagePath] = img; };
        img.onerror = () => { console.error(`Fehler beim Laden des Bildes: ${imagePath}.`); };
    }
}

export function findItemAtPosition(mousePos, dataMap, configMap, canvas) {
    let foundItem = null;
    const keys = Object.keys(configMap).reverse();

    for (const key of keys) {
        if (key === 'minimap' || !dataMap[key]) continue;

        const data = dataMap[key];
        const config = configMap[key];

        if (data.count === 0) continue;

        for (const item of data.items) {
            const renderInfo = config.renderConfigSelector(item, false);

            if (renderInfo.type === 'none') continue;

            const size = renderInfo.size ?? 16;
            const pos = worldToCanvas(item.position, canvas);

            if (Math.abs(mousePos.x - pos.u) < size / 2 &&
                Math.abs(mousePos.y - pos.v) < size / 2)
            {
                foundItem = item;
                break;
            }
        }

        if (foundItem) {
            break;
        }
    }

    return foundItem;
}

export function getMousePos(canvas, evt) {
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
    return {
        x: (evt.clientX - rect.left) * scaleX,
        y: (evt.clientY - rect.top) * scaleY
    };
}