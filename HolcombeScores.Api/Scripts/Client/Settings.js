class Settings {
    constructor() {
    }
    
    get apiHost() {
        if (!this._apiHost) {
            this._apiHost = '%API_HOST%';
        }
        
        return this._apiHost;
    }
}