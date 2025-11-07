import { getElem } from './utils.js';


export function renderPlayer(data) {
    const nameElem = getElem('char-name');
    const statsListElem = getElem('char-stats');
    const avatarElem = getElem('char-avatar');

    if (!nameElem || !statsListElem) return;

    if (data.count === 0) {
        nameElem.textContent = "No Player";
        statsListElem.innerHTML = '';
        return;
    }

    const player = data.items[0];
    const props = player.customProperties;
    
    const charName = props.character ?? "Unkown";
    nameElem.textContent = charName;

    if (avatarElem && props.character) {
        avatarElem.src = `/images/characters/${props.character.toString().toLowerCase()}.png`;
    }
    
    const stats = props.stats;

    let statsHtml = '';

    if (stats) {
        for (const key of Object.keys(stats)) {
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