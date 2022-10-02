class Testing {
    constructor(http) {
        this.http = http;
    }

    async getTestingId() {
        const contextObject = await this.http.get(`/api/Testing`);
        return contextObject.contextId;
    }

    startTesting(request) {
        return this.http.post(`/api/Testing`, request);
    }

    stopTesting(endEntireStack) {
        return this.http.delete(`/api/Testing`, {
            endEntireStack: endEntireStack
        });
    }

    getAllTestingContexts() {
        return this.http.get(`/api/Testing/All`);
    }

    endAllTestingContexts() {
        return this.http.delete(`/api/Testing/All`);
    }
}

export { Testing };