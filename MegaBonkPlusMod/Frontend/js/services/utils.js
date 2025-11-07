export function getElem(id) {
    const elem = document.getElementById(id);
    if (!elem) console.error(`Render-Fehler: Element mit ID '${id}' nicht gefunden.`);
    return elem;
}