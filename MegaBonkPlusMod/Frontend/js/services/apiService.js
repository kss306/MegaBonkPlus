async function fetchData(endpoint) {
    try {
        const response = await fetch(endpoint);
        if (!response.ok) {
            throw new Error(`Netzwerkfehler: ${response.statusText}`);
        }
        const data = await response.json();
        return data;
    } catch (error) {
        console.error(`Fehler beim Abrufen von ${endpoint}:`, error);
        return {count: 0, items: []};
    }
}

export {fetchData};