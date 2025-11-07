import { getElem } from './utils.js';


export function renderPlayer(data) {
    const nameElem = getElem('char-name');
    const statsListElem = getElem('char-stats');
    const avatarElem = getElem('char-avatar');

    const hpFill = getElem('bar-hp-fill');
    const hpLabel = getElem('bar-hp-label');
    const shieldFill = getElem('bar-shield-fill');
    const shieldLabel = getElem('bar-shield-label');

    const levelElem = getElem('char-level');
    const timesElem = getElem('char-times');

    if (!nameElem || !statsListElem) return;

    if (data.count === 0) {
        nameElem.textContent = "No Player";
        statsListElem.innerHTML = '';
        if (hpFill) hpFill.style.width = '0%';
        if (shieldFill) shieldFill.style.width = '0%';
        if (hpLabel) hpLabel.textContent = '';
        if (shieldLabel) shieldLabel.textContent = '';
        if (levelElem) levelElem.textContent = 'Level ?';
        if (timesElem) timesElem.textContent = '0:00 / 0:00';
        return;
    }

    const player = data.items[0];
    const props = player.customProperties;
    
    const charName = props.character ?? "Unknown";
    nameElem.textContent = charName;

    if (avatarElem && props.character) {
        avatarElem.src = `/images/characters/${charName.toLowerCase()}.png`;
    }

    if (levelElem) {
        const lvl = props.level ?? 0;
        levelElem.textContent = `Level ${lvl}`;
    }

    let stageTime = null, timeAlive = null;
    for (const it of data.items) {
        const p = it.customProperties || {};
        if (p.stageTime != null || p.timeAlive != null) {
            stageTime = p.stageTime ?? stageTime;
            timeAlive = p.timeAlive ?? timeAlive;
        }
    }
    const fmtTime = (sec) => {
        if (sec == null || isNaN(sec)) return '0:00';
        const s = Math.max(0, Math.floor(sec));
        const m = Math.floor(s / 60);
        const r = s % 60;
        return `${m}:${r.toString().padStart(2,'0')}`;
    };
    if (timesElem) {
        timesElem.textContent = `${fmtTime(timeAlive)} / ${fmtTime(stageTime)}`;
    }
    
    const stats = props.stats;

    const parsePair = (v) => {
        if (v == null) return { curr: 0, max: 0 };
        if (typeof v === 'string') {
            const m = v.match(/([\d.]+)\s*\/\s*([\d.]+)/);
            if (m) return { curr: parseFloat(m[1]), max: parseFloat(m[2]) };
            const n = parseFloat(v);
            return { curr: n, max: n };
        }
        if (typeof v === 'number') return { curr: v, max: v };
        if (typeof v === 'object' && 'current' in v && 'max' in v) {
            return { curr: Number(v.current) || 0, max: Number(v.max) || 0 };
        }
        return { curr: 0, max: 0 };
    };

    if (stats && hpFill && shieldFill) {
        const hp = parsePair(stats['HP']);
        const sh = parsePair(stats['Shield']);

        const hpPct = hp.max > 0 ? Math.max(0, Math.min(100, (hp.curr / hp.max) * 100)) : 0;
        const shPct = sh.max > 0 ? Math.max(0, Math.min(100, (sh.curr / sh.max) * 100)) : 0;

        hpFill.style.width = `${hpPct}%`;
        shieldFill.style.width = `${shPct}%`;

        if (hpLabel) hpLabel.textContent = `${hp.curr} / ${hp.max}`;
        if (shieldLabel) shieldLabel.textContent = `${sh.curr} / ${sh.max}`;
    }

    let statsHtml = '';
    if (stats) {
        const hiddenKeys = new Set(['HP', 'Shield']);

        for (const key of Object.keys(stats)) {
            if (hiddenKeys.has(key)) continue;

            const value = stats[key];
            statsHtml += `
                    <li>
                        <span>${key}</span>
                        <strong>${value}</strong>
                    </li>
                `;
        }
    } else {
        statsHtml = '<li><span>No Stats found.</span><strong>-</strong></li>';
    }

    statsListElem.innerHTML = statsHtml;
}