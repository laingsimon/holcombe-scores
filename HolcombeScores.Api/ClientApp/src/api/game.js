class Game {
    constructor(http) {
        this.http = http;
    }

    getAllGames() {
        return this.http.get(`/api/Games`);
    }

    getGames(teamId) {
        return this.http.get(`/api/Games/${teamId}`);
    }

    getGame(id) {
        return this.http.get(`/api/Game/${id}`);
    }

    deleteGame(id) {
        return this.http.delete(`/api/Game/${id}`);
    }

    createGame(teamId, date, opponent, playingAtHome, playerIds, training, address, friendly) {
        let gameDetail = {
            teamId: teamId,
            date: date,
            opponent: opponent,
            playingAtHome: playingAtHome,
            playerIds: playerIds,
            training: training,
            address: address,
            friendly: friendly
        };

        return this.http.post(`/api/Game`, gameDetail);
    }

    updateGame(id, teamId, date, opponent, playingAtHome, playerIds, training, address, postponed, friendly) {
        let gameDetail = {
            id: id,
            teamId: teamId,
            date: date,
            opponent: opponent,
            playingAtHome: playingAtHome,
            playerIds: playerIds,
            training: training,
            address: address,
            postponed: postponed,
            friendly: friendly
        };

        return this.http.patch(`/api/Game`, gameDetail);
    }

    removePlayer(gameId, playerId) {
        return this.http.delete(`/api/Game/Player/${gameId}/${playerId}`);
    }

    removeGoal(gameId, goalId) {
        return this.http.delete(`/api/Game/Goal/${gameId}/${goalId}`);
    }

    recordGoal(gameId, time, holcombeGoal, playerId, token) {
        let goalDetail = {
            time: time,
            holcombeGoal: holcombeGoal,
            player: holcombeGoal ? { id: playerId } : null,
            gameId: gameId,
            recordGoalToken: token
        };

        return this.http.post(`/api/Game/Goal`, goalDetail);
    }
}

export { Game };
