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

    createGame(gameDetail) {
        return this.http.post(`/api/Game`, gameDetail);
    }

    updateGame(gameDetail) {
        return this.http.patch(`/api/Game`, gameDetail);
    }

    removePlayer(gameId, playerId) {
        return this.http.delete(`/api/Game/Player/${gameId}/${playerId}`);
    }

    removeGoal(gameId, goalId) {
        return this.http.delete(`/api/Game/Goal/${gameId}/${goalId}`);
    }

    recordGoal(gameId, time, holcombeGoal, playerId, token, assistedByPlayerId) {
        let goalDetail = {
            time: time,
            holcombeGoal: holcombeGoal,
            player: holcombeGoal ? { id: playerId } : null,
            gameId: gameId,
            recordGoalToken: token,
            assistedBy: assistedByPlayerId
                ? { id: assistedByPlayerId }
                : null
        };

        return this.http.post(`/api/Game/Goal`, goalDetail);
    }
}

export { Game };
