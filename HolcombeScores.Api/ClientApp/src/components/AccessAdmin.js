import React, { Component } from 'react';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Access} from "../api/access";
import {Team} from "../api/team";
import {Alert} from "./Alert";

export class AccessAdmin extends Component {
    constructor (props) {
        super(props);
        const http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.teamApi = new Team(http);
        this.state = {
            loading: true,
            error: null,
            mode: props.match.params.mode || 'requests',
            myAccess: null,
            requests: null,
            allAccess: null,
            processing: []
        };
        this.changeMode = this.changeMode.bind(this);
        this.respondRequest = this.respondRequest.bind(this);
        this.cancelRequest = this.cancelRequest.bind(this);
        this.cancelAccess = this.cancelAccess.bind(this);
    }

    //event handlers
    changeMode(event) {
        event.preventDefault();
        const url = event.target.getAttribute('href');
        const segments = url.split('/')
        const mode = segments[segments.length - 1];
        this.setState({
            mode: mode,
        });
    }

    async cancelAccess(event) {
        const userId = event.target.getAttribute('data-user-id');

        if (userId === this.state.myAccess.userId) {
            alert('You cannot cancel your own access');
            return;
        }

        if (!window.confirm('Are you sure you want to CANCEL this access')) {
            return;
        }

        this.setState({
            processing: this.union(this.state.processing, userId)
        });

        const result = await this.accessApi.deleteAccess(userId);
        if (result.success) {
            const allAccess = await this.getAllAccess();
            this.setState({
                allAccess: allAccess,
                processing: this.except(this.state.processing, userId)
            });
        }
    }

    async cancelRequest(event) {
        const userId = event.target.getAttribute('data-user-id');

        if (!window.confirm('Are you sure you want to DELETE this request')) {
            return;
        }

        this.setState({
            processing: this.union(this.state.processing, userId)
        });

        const result = await this.accessApi.deleteAccessRequest(userId);
        if (result.success) {
            const requests = await this.getAccessRequests();
            this.setState({
               requests: requests,
               processing: this.except(this.state.processing, userId)
            });
        }
    }

    async respondRequest(event) {
        const userId = event.target.getAttribute('data-user-id');

        const allow = window.confirm('Should this user be permitted?');
        const reason = allow ? null : window.prompt('Enter reason for rejection');
        await this.respondToRequest(userId, null, reason, allow);
    }

    componentDidMount() {
        // noinspection JSIgnoredPromiseFromCall
        this.requestData();
    }

    // renderers
    renderNav() {
        return (<ul className="nav nav-pills">
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'requests' ? ' active' : ''}`} href={`/admin/requests`} onClick={this.changeMode}>Requests</a>
            </li>
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'access' ? ' active' : ''}`} href={`/admin/access`} onClick={this.changeMode}>Access</a>
            </li>
        </ul>);
    }

    render() {
        if (this.state.loading) {
            return (<div className="d-flex justify-content-center">
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>);
        }

        if (this.state.error) {
            return (<div>
                {this.renderNav()}
                <hr />
                <Alert errors={[ this.state.error ]} />
            </div>);
        }

        if (!this.state.myAccess.admin) {
            return (<Alert warnings={[ 'This service is only available for administrators.' ]} />);
        }

        if (this.state.mode === 'requests') {
            return this.renderRequests();
        }
        if (this.state.mode === 'access') {
            return this.renderAccess();
        }

        return (<div>Unknown mode: {this.state.mode}</div>);
    }

    renderAccess() {
        return (<div>
            {this.renderNav()}
            <hr />
            <div className="list-group">
                {this.state.allAccess.map(access => this.renderCurrentAccess(access))}
            </div>
        </div>);
    }

    renderCurrentAccess(access) {
        const team = this.state.teams[access.teamId];
        const processing = Object.keys(this.state.processing).includes(access.userId);

        return (<div key={access.userId} className="list-group-item list-group-item-action flex-column align-items-start">
            <span>Name: <strong>{access.name}</strong>, Team: {team.name}{access.admin ? ', Admin' : ''}</span>
            <span className="float-end">
                <button type="button" className={`btn ${processing || access.userId === this.state.myAccess.userId ? 'btn-light' : 'btn-danger'}`} data-user-id={access.userId} onClick={this.cancelAccess}>&times;</button>
            </span>
        </div>);
    }

    renderRequests() {
        return (<div>
            {this.renderNav()}
            <hr />
            <h5>Pending: {this.state.requests.filter(r => !r.rejected).length}</h5>
            <div className="list-group">
                {this.state.requests.filter(r => !r.rejected).map(request => this.renderRequest(request))}
            </div>
            <h5>Rejected: {this.state.requests.filter(r => r.rejected).length}</h5>
            <div className="list-group">
                {this.state.requests.filter(r => r.rejected).map(request => this.renderRequest(request))}
            </div>
        </div>);
    }

    renderRequest(request) {
        const requestedDate = new Date(request.requested);
        const team = this.state.teams[request.teamId];
        const processing = Object.keys(this.state.processing).includes(request.userId);

        const respondRequestButton = (<button type="button" className={`btn ${processing ? 'btn-light' : 'btn-success'}`} data-user-id={request.userId} onClick={this.respondRequest}>&#10003;</button>);

        return (<div key={request.userId} className="list-group-item list-group-item-action flex-column align-items-start">
            <span>Name: <strong>{request.name}</strong>, Team: {team.name}, Requested: {requestedDate.toLocaleString()}</span>
            <span className="float-end">
                <button type="button" className={`btn ${processing ? 'btn-light' : 'btn-danger'}`} data-user-id={request.userId} onClick={this.cancelRequest}>&times;</button>
                &nbsp;
                {request.rejected ? null : respondRequestButton}
            </span>
        </div>);
    }

    //api
    async getAccessRequests() {
        const requests = await this.accessApi.getAllAccessRequests();
        requests.sort((a, b) => Date.parse(b.requested) - Date.parse(a.requested));

        return requests;
    }

    async getAllAccess() {
        const allAccess = await this.accessApi.getAllAccess();
        allAccess.sort((a, b) => a.name - b.name);

        return allAccess;
    }

    async requestData() {
        try {
            const myAccess = await this.accessApi.getMyAccess();
            const teams = await this.teamApi.getAllTeams();

            const teamsMap = {};
            teams.forEach(team => {
                teamsMap[team.id] = team;
            });

            this.setState({
                loading: false,
                myAccess: myAccess.access,
                requests: await this.getAccessRequests(),
                allAccess: await this.getAllAccess(),
                teams: teamsMap
            });
        } catch (e) {
            this.setState({
                error: e,
                loading: false
            });
        }
    }

    async respondToRequest(userId, teamIdOverride, reason, allow) {
        const request = this.state.requests.filter(r => r.userId === userId)[0];
        const message = allow
            ? `Confirm approval of ${request.name} access to the requested team?`
            : `Confirm rejection of ${request.name} because ${reason}?`;
        if (!window.confirm(message)) {
            return;
        }

        this.setState({
            processing: this.union(this.state.processing, userId)
        });

        const result = await this.accessApi.respondToAccessRequest(userId, teamIdOverride || request.teamId, reason, allow);
        if (result.success) {
            this.setState({
                requests: await this.getAccessRequests(),
                allAccess: await this.getAllAccess(),
                processing: this.except(this.state.processing, userId)
            });
        }
    }

    // utilities
    except(selected, remove) {
        let copy = Object.assign({}, selected);
        delete copy[remove];
        return copy;
    }

    union(selected, add) {
        let copy = Object.assign({}, selected);
        copy[add] = true;
        return copy;
    }
}