export function formatTime(seconds) {
    if (seconds == null || isNaN(seconds)) return '0:00';
    const s = Math.max(0, Math.floor(seconds));
    const m = Math.floor(s / 60);
    const r = s % 60;
    return `${m}:${r.toString().padStart(2, '0')}`;
}

export function formatNumber(num) {
    return num.toLocaleString('en-US');
}

export function parseStatPair(value) {
    if (value == null) return {current: 0, max: 0};

    if (typeof value === 'string') {
        const match = value.match(/([\d.]+)\s*\/\s*([\d.]+)/);
        if (match) return {current: parseFloat(match[1]), max: parseFloat(match[2])};
        const num = parseFloat(value);
        return {current: num, max: num};
    }

    if (typeof value === 'number') {
        return {current: value, max: value};
    }

    if (typeof value === 'object' && 'current' in value && 'max' in value) {
        return {current: Number(value.current) || 0, max: Number(value.max) || 0};
    }

    return {current: 0, max: 0};
}

export function calculatePercentage(value, max) {
    if (max === 0) return 0;
    return Math.max(0, Math.min(100, (value / max) * 100));
}

export function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}