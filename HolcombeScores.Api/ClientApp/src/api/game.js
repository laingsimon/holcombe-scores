class Game {
    constructor(http) {
        this.http = http;
    }

    getAllGames() {
        return this.http.get(`/api/Games`);
    }

    getGame(id) {
        return this.http.get(`/api/Game/${id}`);
    }

    deleteGame(id) {
        return this.http.delete(`/api/Game/${id}`);
    }

    createGame(teamId, date, opponent, playingAtHome, playerNames) {
        let gameDetail = {
            teamId: teamId,
            date: date,
            opponent: opponent,
            playingAtHome: playingAtHome,
            players: playerNames
        };

        return this.http.post(`/api/Game`, gameDetail);
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

        return this.http.patch(`/api/Game`, gameDetail);
    }

    removePlayer(id, playerNumber) {
        return this.http.delete(`/api/Game/${id}/${playerNumber}`);
    }

    removeGoal(id, goalId) {
        return this.http.delete(`/api/Game/${id}/${goalId}`);
    }

    recordGoal(gameId, time, holcombeGoal, playerNumber) {
        let goalDetail = {
            time: time,
            holcombeGoal: holcombeGoal,
            player: holcombeGoal ? { number: playerNumber } : null,
            gameId: gameId
        };

        return this.http.post(`/api/Game/Goal`, goalDetail);
    }
}

export { Game };