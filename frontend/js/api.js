/* api.js -- API Service Layer */

const CONFIG = {
    platformService: 'http://localhost:5000',
    commandService: 'http://localhost:8080'
};

async function apiCall(url, options = {}) {
    try {
        const fetchOptions = { ...options };
        if (fetchOptions.body) {
            fetchOptions.headers = { 'Content-Type': 'application/json', ...fetchOptions.headers };
        }
        const response = await fetch(url, fetchOptions);

        if (!response.ok) {
            const text = await response.text().catch(() => '');
            return { ok: false, status: response.status, error: text || `HTTP ${response.status}` };
        }

        const contentType = response.headers.get('content-type') || '';
        if (contentType.includes('application/json')) {
            const data = await response.json();
            return { ok: true, data };
        }
        const text = await response.text();
        return { ok: true, data: text };
    } catch (err) {
        console.error(`[API] ${url} failed:`, err);
        return { ok: false, error: err.message || 'Network error' };
    }
}

/* PlatformService API */
const PlatformAPI = {
    getAll() {
        return apiCall(`${CONFIG.platformService}/api/platform`);
    },
    getById(id) {
        return apiCall(`${CONFIG.platformService}/api/platform/${id}`);
    },
    create(data) {
        return apiCall(`${CONFIG.platformService}/api/platform`, {
            method: 'POST',
            body: JSON.stringify(data)
        });
    }
};

/* CommandService API */
const CommandAPI = {
    getAllPlatforms() {
        return apiCall(`${CONFIG.commandService}/api/cmd/platform`);
    },
    getCommands(platformId) {
        return apiCall(`${CONFIG.commandService}/api/cmd/platforms/${platformId}/commands`);
    },
    getCommand(platformId, commandId) {
        return apiCall(`${CONFIG.commandService}/api/cmd/platforms/${platformId}/commands/${commandId}`);
    },
    createCommand(platformId, data) {
        return apiCall(`${CONFIG.commandService}/api/cmd/platforms/${platformId}/commands`, {
            method: 'POST',
            body: JSON.stringify(data)
        });
    },
    testConnection() {
        return apiCall(`${CONFIG.commandService}/api/cmd/platform`);
    }
};
