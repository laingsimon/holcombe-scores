class HolcombeScores {
    constructor() {
        this.settings = new Settings();
        this.http = new Http(this.settings);
        this.api = new Api(this.settings, this.http);
    }
}

export { HolcombeScores };