import {getMapData} from '../../configs/mapConfig.js';

export function worldToCanvasPercentages(worldPos, canvasWidth, canvasHeight, currentMap) {
    const canvasCenterX = canvasWidth / 2;
    const canvasCenterY = canvasHeight / 2;

    const mapData = getMapData(currentMap);
    
    const relativeX = worldPos.x - mapData.worldOffsetX;
    const relativeZ = worldPos.z - mapData.worldOffsetZ;

    const u = (relativeX / mapData.mapScale) + canvasCenterX;
    const v = (relativeZ / mapData.mapScale) + canvasCenterY;

    let leftPercent = (u / canvasWidth) * 100;
    let topPercent = (v / canvasHeight) * 100;

    leftPercent = Math.max(0, Math.min(100, leftPercent));
    topPercent = Math.max(0, Math.min(100, topPercent));

    return {leftPercent, topPercent};
}