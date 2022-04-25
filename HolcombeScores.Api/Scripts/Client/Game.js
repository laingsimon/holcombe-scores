class Game {
    constructor(api) {
        this.api = api;
    }

    getAllGames() {
        return this.api.http.get(`/api/Games`);
    }

    getGame(id) {
        return this.api.http.get(`/api/Game/${id}`);
    }

    deleteGame(id) {
        return this.api.http.delete(`/api/Game/${id}`);
    }

    createGame(teamId, date, opponent, playingAtHome, playerNames) {
        let gameDetail = {
            teamId: teamId,
            date: date,
            opponent: opponent,
            playingAtHome: playingAtHome,
            players: playerNames
        };
        
        return this.api.http.post(`/api/Game`, gameDetail);
    }

    updateGame(id, teamId, date, opponent, playingAtHome, playerNames) {
        let gameDetail = {
            id: id,
            teamId: teamId,
            date: date,
            opponent: opponent,
            playingAtHome: playingAtHome,
            players: playerNames
        };

        return this.api.http.patch(`/api/Game`, gameDetail);
    }
    
    removePlayer(id, playerNumber) {
        return this.api.http.delete(`/api/Game/${id}/${playerNumber}`);
    }

    removeGoal(id, goalId) {
        return this.api.http.delete(`/api/Game/${id}/${goalId}`);
    }

    recordGoal(id, time, holcombeGoal, playerNumber) {
        let goalDetail = {
            time: time,
            holcombeGoal: holcombeGoal,
            player: holcombeGoal ? { number: playerNumber } : null,
            gameId: id
        };
        
        return this.api.http.post(`/api/Game/Goal`, goalDetail);
    }
}

registerHolcombeScoresApi(api => { api.game = new Game(api) });