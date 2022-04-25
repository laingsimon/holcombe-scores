class Team {
    constructor(api) {
        this.api = api;
    }

    getAllTeams() {
        return this.api.http.get(`/api/Teams`);
    }

    createTeam(id, name, coach) {
        let teamDetail = {
          name: name,
          coach: coach,
          id: id
        };
        
        return this.api.http.post(`/api/Team`, teamDetail);
    }

    updateTeam(id, name, coach) {
        let teamDetail = {
            name: name,
            coach: coach,
            id: id
        };

        return this.api.http.patch(`/api/Team`, teamDetail);
    }

    deleteTeam(teamId) {
        return this.api.http.delete(`/api/Team/${teamId}`);
    }
}

registerHolcombeScoresApi(api => { api.team = new Team(api) });