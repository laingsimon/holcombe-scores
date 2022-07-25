class Settings {
    get apiHost() {
        if (!this._apiHost) {
            this._apiHost = document.location.hostname === 'localhost'
                ? 'https://localhost:5001'
                : document.location.origin + '/data';
        }

        return this._apiHost;
    }
}

export { Settings };