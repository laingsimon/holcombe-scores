class Team {
    constructor(http) {
        this.http = http;
    }

    getAllTeams() {
        return this.http.get(`/api/Teams`);
    }

    async getTeam(id) {
        const teams = await this.getAllTeams();
        const matchingTeams = teams.filter(t => t.id === id);
        return matchingTeams.length > 0
            ? matchingTeams[0]
            : null;
    }

    createTeam(name, coach) {
        let teamDetail = {
            name: name,
            coach: coach
        };

        return this.http.post(`/api/Team`, teamDetail);
    }

    updateTeam(id, name, coach) {
        let teamDetail = {
            name: name,
            coach: coach,
            id: id
        };

        return this.http.patch(`/api/Team`, teamDetail);
    }

    deleteTeam(teamId) {
        return this.http.delete(`/api/Team/${teamId}`);
    }
}

export { Team };