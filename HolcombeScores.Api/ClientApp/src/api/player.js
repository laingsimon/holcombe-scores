class Player {
    constructor(http) {
        this.http = http;
    }

    getAllPlayers() {
        return this.http.get(`/api/Players`);
    }

    getPlayers(teamId) {
        return this.http.get(`/api/Players/${teamId}`);
    }

    updatePlayer(playerId, teamId, number, name) {
        let playerDetail = {
            id: playerId,
            name: name,
            number: number,
            teamId: teamId
        };

        return this.http.put(`/api/Player`, playerDetail);
    }

    deletePlayer(playerId) {
        return this.http.delete(`/api/Player/${playerId}`);
    }

    transferPlayer(playerId, newTeamId, optionalNewNumber) {
        let transferDetail = {
            playerId: playerId,
            newNumber: optionalNewNumber,
            newTeamId: newTeamId
        };

        return this.http.post(`/api/Player/Transfer`, transferDetail);
    }
}

export { Player };