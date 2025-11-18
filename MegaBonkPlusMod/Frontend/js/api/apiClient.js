class ApiClient {
    constructor(baseUrl = '') {
        this.baseUrl = baseUrl;
    }

    async request(endpoint, options = {}) {
        try {
            const response = await fetch(`${this.baseUrl}${endpoint}`, {
                headers: {
                    'Content-Type': 'application/json',
                    ...options.headers
                },
                ...options
            });

            const text = await response.text();

            let data;
            try {
                data = text ? JSON.parse(text) : {};
            } catch (parseError) {
                throw new ApiError('Invalid JSON response', parseError.message, response.status);
            }

            if (!response.ok) {
                const message = data?.message || response.statusText || 'Request failed';
                throw new ApiError(message, data?.error, response.status);
            }

            if (!data.success) {
                throw new ApiError(data.message, data.error, data.statusCode ?? response.status);
            }

            return data;
        } catch (error) {
            if (error instanceof ApiError) {
                throw error;
            }
            throw new ApiError('Network error', error.message, 0);
        }
    }

    get(endpoint) {
        return this.request(endpoint, {method: 'GET'});
    }

    post(endpoint, body) {
        return this.request(endpoint, {
            method: 'POST',
            body: JSON.stringify(body)
        });
    }
}

class ApiError extends Error {
    constructor(message, error, statusCode) {
        super(message);
        this.error = error;
        this.statusCode = statusCode;
    }
}

const api = new ApiClient();

export {api, ApiError};