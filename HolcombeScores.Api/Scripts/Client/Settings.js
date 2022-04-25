class Settings {
    constructor() {
    }
    
    get apiHost() {
        if (!this._apiHost) {
            this._apiHost = document.location.origin;
        }
        
        return this._apiHost;
    }
}