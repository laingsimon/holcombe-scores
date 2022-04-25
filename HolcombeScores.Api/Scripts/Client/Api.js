class Api {
    constructor(settings, http) {
        this.settings = settings;
        this.http = http;
        
        if (window.holcombeScoresApis) {
            for (var i = 0; i < window.holcombeScoresApis.length; i++) {
                let factory = window.holcombeScoresApis[i];
                factory(this);
            }
            
            console.log(`Registered ${window.holcombeScoresApis.length} HolcombeScores API/s`);
        } else {
            console.error("HolcombeScores APIs register does not exist");
        }
    }
}