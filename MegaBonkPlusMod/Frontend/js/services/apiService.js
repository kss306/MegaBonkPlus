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

async function postData(endpoint, body) {
    try {
        const response = await fetch(endpoint, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(body)
        });
        if (!response.ok) {
            throw new Error(`Netzwerkfehler: ${response.statusText}`);
        }
        return await response.json();
    } catch (error) {
        console.error(`Fehler beim POST an ${endpoint}:`, error);
        return { status: "error" };
    }
}

export {fetchData, postData};