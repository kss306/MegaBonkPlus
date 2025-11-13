import { MAP_SCALE, WORLD_OFFSET_X, WORLD_OFFSET_Z } from '../../configs/mapConfig.js';

export function worldToCanvasPercentages(worldPos, canvasWidth, canvasHeight) {
    const canvasCenterX = canvasWidth / 2;
    const canvasCenterY = canvasHeight / 2;

    const relativeX = worldPos.x - WORLD_OFFSET_X;
    const relativeZ = worldPos.z - WORLD_OFFSET_Z;

    const u = (relativeX / MAP_SCALE) + canvasCenterX;
    const v = (relativeZ / MAP_SCALE) + canvasCenterY;

    let leftPercent = (u / canvasWidth) * 100;
    let topPercent = (v / canvasHeight) * 100;

    leftPercent = Math.max(0, Math.min(100, leftPercent));
    topPercent = Math.max(0, Math.min(100, topPercent));

    return { leftPercent, topPercent };
}