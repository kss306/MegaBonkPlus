import { getElem } from '../../utils/dom.js';
import { formatTime, parseStatPair, calculatePercentage } from '../../utils/format.js';
import { isDataEmpty } from '../../utils/data.js';

export function renderPlayerStats(data) {
    if (isDataEmpty(data)) {
        renderNoPlayer();
        return;
    }

    const player = data.items[0];
    const props = player.customProperties;

    renderPlayerInfo(props);
    renderPlayerBars(props.stats);
    renderPlayerTimes(data.items);
    renderPlayerStatsList(props.stats);
}

export function clearPlayerStats() {
    renderNoPlayer();
}

function renderNoPlayer() {
    const nameElem = getElem('char-name');
    const statsListElem = getElem('char-stats');
    const avatarElem = getElem('char-avatar');
    const levelElem = getElem('char-level');
    const timesElem = getElem('char-times');

    if (nameElem) nameElem.textContent = "No Player";
    if (statsListElem) statsListElem.innerHTML = '';
    if (avatarElem) avatarElem.src = '/images/characters/placeholder.png';
    if (levelElem) levelElem.textContent = 'Level ?';
    if (timesElem) timesElem.textContent = '0:00 / 0:00';

    clearPlayerBars();
}

function renderPlayerInfo(props) {
    const nameElem = getElem('char-name');
    const avatarElem = getElem('char-avatar');
    const levelElem = getElem('char-level');

    const charName = props.character ?? "Unknown";

    if (nameElem) nameElem.textContent = charName;
    if (avatarElem) avatarElem.src = `/images/characters/${charName.toLowerCase()}.png`;
    if (levelElem) levelElem.textContent = `Level ${props.level ?? 0}`;
}

function renderPlayerBars(stats) {
    if (!stats) {
        clearPlayerBars();
        return;
    }

    const hpFill = getElem('bar-hp-fill');
    const hpLabel = getElem('bar-hp-label');
    const shieldFill = getElem('bar-shield-fill');
    const shieldLabel = getElem('bar-shield-label');

    const hp = parseStatPair(stats['HP']);
    const shield = parseStatPair(stats['Shield']);

    const hpPct = calculatePercentage(hp.current, hp.max);
    const shieldPct = calculatePercentage(shield.current, shield.max);

    if (hpFill) hpFill.style.width = `${hpPct}%`;
    if (shieldFill) shieldFill.style.width = `${shieldPct}%`;
    if (hpLabel) hpLabel.textContent = `${hp.current} / ${hp.max}`;
    if (shieldLabel) shieldLabel.textContent = `${shield.current} / ${shield.max}`;
}

function clearPlayerBars() {
    const hpFill = getElem('bar-hp-fill');
    const hpLabel = getElem('bar-hp-label');
    const shieldFill = getElem('bar-shield-fill');
    const shieldLabel = getElem('bar-shield-label');

    if (hpFill) hpFill.style.width = '0%';
    if (shieldFill) shieldFill.style.width = '0%';
    if (hpLabel) hpLabel.textContent = '';
    if (shieldLabel) shieldLabel.textContent = '';
}

function renderPlayerTimes(items) {
    const timesElem = getElem('char-times');
    if (!timesElem) return;

    let stageTime = null, timeAlive = null;

    for (const item of items) {
        const props = item.customProperties || {};
        if (props.stageTime != null || props.timeAlive != null) {
            stageTime = props.stageTime ?? stageTime;
            timeAlive = props.timeAlive ?? timeAlive;
        }
    }

    timesElem.textContent = `${formatTime(timeAlive)} / ${formatTime(stageTime)}`;
}

function renderPlayerStatsList(stats) {
    const statsListElem = getElem('char-stats');
    if (!statsListElem) return;

    if (!stats) {
        statsListElem.innerHTML = '<li><span>No Stats found.</span><strong>-</strong></li>';
        return;
    }

    const hiddenKeys = new Set(['HP', 'Shield']);
    let statsHtml = '';

    for (const [key, value] of Object.entries(stats)) {
        if (hiddenKeys.has(key)) continue;
        statsHtml += `
            <li>
                <span>${key}</span>
                <strong>${value}</strong>
            </li>
        `;
    }

    statsListElem.innerHTML = statsHtml || '<li><span>No Stats found.</span><strong>-</strong></li>';
}