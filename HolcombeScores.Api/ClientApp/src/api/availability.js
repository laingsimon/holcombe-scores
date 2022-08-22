class Availability {
    constructor(http) {
        this.http = http;
    }

    get(teamId, gameId) {
        return this.http.get(`/api/Availability/${teamId}/${gameId}`);
    }

    update(teamId, gameId, playerId, available) {
        return this.http.post(`/api/Availability`, {
            teamId: teamId,
            gameId: gameId,
            playerId: playerId,
            available: available
        });
    }

    delete(teamId, gameId, playerId) {
        return this.http.delete(`/api/Availability`, {
            teamId: teamId,
            gameId: gameId,
            playerId: playerId
        });
    }
}

export { Availability };
