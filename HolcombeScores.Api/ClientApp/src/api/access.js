class Access {
    constructor(http) {
        this.http = http;
    }

    logout() {
       return this.http.post(`/api/Access/Logout`, {});
    }

    impersonate(userId, adminPassCode) {
       return this.http.post(`/api/Access/Impersonate`, {
         userId: userId,
         adminPassCode: adminPassCode
       });
    }

    unimpersonate() {
       return this.http.post(`/api/Access/Unimpersonate`, {});
    }

    getAllAccess() {
        return this.http.get(`/api/Access`);
    }

    deleteAccess(userId, teamId) {
        if (teamId) {
            return this.http.delete(`/api/Access/${userId}/${teamId}`);
        } else {
            return this.http.delete(`/api/Access/${userId}`);
        }
    }

    updateAccess(teamId, userId, name, admin, manager) {
        let access = {
            teamId: teamId,
            userId: userId,
            admin: admin,
            manager: manager,
            name: name,
        };

        return this.http.patch(`/api/Access`, access);
    }

    getMyAccess() {
        return this.http.get(`/api/My/Access`);
    }

    getAccessForRecovery() {
        return this.http.get(`/api/Access/Recover`);
    }

    recoverAccess(recoveryId, adminPassCode) {
        let accessToRecover = {
            recoveryId: recoveryId,
            adminPassCode: adminPassCode
        };

        return this.http.post(`/api/Access/Recover`, accessToRecover);
    }

    createAccessRequest(name, teamId) {
        let access = {
            teamId: teamId,
            name: name
        };

        return this.http.post(`/api/Access/Request`, access);
    }

    deleteAccessRequest(userId, teamId) {
        if (userId) {
            return this.http.delete(`/api/Access/Request/${userId}/${teamId}`);
        } else {
            return this.http.delete(`/api/Access/Request/${teamId}`);
        }
    }

    getAllAccessRequests() {
        return this.http.get(`/api/Access/Request`);
    }

    respondToAccessRequest(userId, teamId, reason, allow) {
        let response = {
            userId: userId,
            allow: allow,
            reason: reason,
            teamId: teamId
        };

        return this.http.post(`/api/Access/Respond`, response);
    }

    revokeAccess(userId, teamId, reason) {
        let accessToRevoke = {
            userId: userId,
            reason: reason,
            teamId: teamId
        };

        return this.http.post(`/api/Access/Revoke`, accessToRevoke);
    }
}

export { Access };
