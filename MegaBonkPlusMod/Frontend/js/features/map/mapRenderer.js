import {getElem} from '../../utils/dom.js';

export function renderMinimap(data, currentMap) {
    const canvas = getElem('minimap-canvas');
    const wrapper = getElem('minimap-wrapper');

    if (!canvas || !wrapper) return;

    if (data.count === 0 || !data.items[0] || !data.items[0].customProperties.rawPixelData) {
        wrapper.classList.add('is-loading');
        return;
    }

    wrapper.classList.remove('is-loading');

    const props = data.items[0].customProperties;
    const width = props.width;
    const height = props.height;

    if (canvas.width !== width) canvas.width = width;
    if (canvas.height !== height) canvas.height = height;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    try {
        const binaryString = atob(props.rawPixelData);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        const imageData = new ImageData(new Uint8ClampedArray(bytes.buffer), width, height);
        ctx.putImageData(imageData, 0, 0);
    } catch (e) {
        console.error("Error rendering minimap:", e);
    }
}