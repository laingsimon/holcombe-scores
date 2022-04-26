class Access {
    constructor(api) {
        this.api = api;
    }

    getAllAccess() {
        return this.api.http.get(`/api/Access`);
    }

    deleteAccess(userId) {
        return this.api.http.delete(`/api/Access/${userId}`);
    }

    updateAccess(teamId, userId, name, admin) {
        let access = {
            teamId: teamId,
            userId: userId,
            admin: admin,
            name: name,
        };

        return this.api.http.patch(`/api/Access`, access);
    }

    getMyAccess() {
        return this.api.http.get(`/api/My/Access`);
    }

    getAccessForRecovery() {
        return this.api.http.get(`/api/Access/Recover`);
    }

    recoverAccess(recoveryId, adminPassCode) {
        let accessToRecover = {
            recoveryId: recoveryId,
            adminPassCode: adminPassCode
        };

        return this.api.http.post(`/api/Access/Recover`, accessToRecover);
    }

    createAccessRequest(userId, name, teamId) {
        let access = {
            userId: userId,
            teamId: teamId,
            name: name
        };

        return this.api.http.post(`/api/Access/Request`, access);
    }

    deleteAccessRequest(userId) {
        return this.api.http.delete(`/api/Access/Request/${userId}`);
    }

    getAllAccessRequests() {
        return this.api.http.get(`/api/Access/Request`);
    }

    respondToAccessRequest(userId, teamId, reason, allow) {
        let response = {
            userId: userId,
            allow: allow,
            reason: reason,
            teamId: teamId
        };

        return this.api.http.post(`/api/Access/Respond`, response);
    }

    revokeAccess(userId, teamId, reason) {
        let accessToRevoke = {
            userId: userId,
            reason: reason,
            teamId: teamId
        };

        return this.api.http.post(`/api/Access/Revoke`, accessToRevoke);
    }
}

registerHolcombeScoresApi(api => { api.access = new Access(api) });