class Player {
    constructor(http) {
        this.http = http;
    }

    getAllPlayers() {
        return this.http.get(`/api/Players`);
    }

    updatePlayer(teamId, number, name) {
        let playerDetail = {
            name: name,
            number: number,
            teamId: teamId
        };

        return this.http.put(`/api/Player`, playerDetail);
    }

    deletePlayer(teamId, number) {
        return this.http.delete(`/api/Player/${teamId}/${number}`);
    }

    transferPlayer(currentTeamId, currentNumber, newTeamId, optionalNewNumber) {
        let transferDetail = {
            currentNumber: currentNumber,
            currentTeamId: currentTeamId,
            newNumber: optionalNewNumber,
            newTeamId: newTeamId
        };

        return this.http.post(`/api/Player/Transfer`, transferDetail);
    }
}

export { Player };