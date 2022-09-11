class Testing {
    constructor(http) {
        this.http = http;
    }

    getTestingId() {
        return this.http.get(`/api/Testing`);
    }

    startTesting(request) {
        return this.http.post(`/api/Testing`, request);
    }

    stopTesting() {
        return this.http.delete(`/api/Testing`);
    }
}

export { Testing };