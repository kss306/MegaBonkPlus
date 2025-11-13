export function createEmptyData() {
    return { count: 0, items: [] };
}

export function isDataEmpty(data) {
    return !data || !data.items || data.items.length === 0 || data.count === 0;
}

export function getProperty(obj, path, defaultValue = null) {
    const keys = path.split('.');
    let result = obj;

    for (const key of keys) {
        if (result == null) return defaultValue;
        result = result[key];
    }

    return result ?? defaultValue;
}

export function deepClone(obj) {
    return JSON.parse(JSON.stringify(obj));
}