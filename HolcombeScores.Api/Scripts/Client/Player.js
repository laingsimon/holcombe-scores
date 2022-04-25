class Player {
    constructor(api) {
        this.api = api;
    }

    getAllPlayers() {
        return this.api.http.get(`/api/Players`);
    }

    updatePlayer(teamId, number, name) {
        let playerDetail = {
            name: name,
            number: number,
            teamId: teamId
        };
        
        return this.api.http.put(`/api/Player`, playerDetail);
    }

    deletePlayer(teamId, number, name) {
        let playerDetail = {
            name: name,
            number: number,
            teamId: teamId
        };

        return this.api.http.delete(`/api/Player`, playerDetail);
    }

    transferPlayer(currentTeamId, currentNumber, newTeamId, optionalNewNumber) {
        let transferDetail = {
            currentNumber: currentNumber,
            currentTeamId: currentTeamId,
            newNumber: optionalNewNumber,
            newTeamId: newTeamId
        };
        
        return this.api.http.post(`/api/Player/Transfer`, transferDetail);
    }
}

registerHolcombeScoresApi(api => { api.player = new Player(api) });