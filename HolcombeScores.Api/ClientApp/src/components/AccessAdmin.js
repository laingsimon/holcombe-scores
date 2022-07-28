import React, { Component } from 'react';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Access} from "../api/access";
import {Team} from "../api/team";
import {Alert} from "./Alert";
import {AccessOverview} from "./AccessOverview";
import {Functions} from '../functions'

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
            processing: [],
            cache: null,
            cacheAt: null,
        };
        this.changeMode = this.changeMode.bind(this);
        this.respondRequest = this.respondRequest.bind(this);
        this.deleteRequest = this.deleteRequest.bind(this);
        this.getCache = this.getCache.bind(this);
        this.accessChanged = this.accessChanged.bind(this);
    }

    //event handlers
    async accessChanged() {
        this.setState({
            allAccess: await this.getAllAccess(true),
        });
    }

    changeMode(event) {
        event.preventDefault();
        const url = event.target.getAttribute('href');
        const segments = url.split('/')
        const mode = segments[segments.length - 1];
        this.setState({
            mode: mode,
        });
        window.history.replaceState(null, event.target.textContent, url);
    }

    async deleteRequest(event) {
        const userId = event.target.getAttribute('data-user-id');

        if (!window.confirm('Are you sure you want to DELETE this request')) {
            return;
        }

        this.setState({
            processing: Functions.union(this.state.processing, userId)
        });

        const result = await this.accessApi.deleteAccessRequest(userId);
        if (result.success) {
            const requests = await this.getAccessRequests();
            this.setState({
               requests: requests,
               processing: Functions.except(this.state.processing, userId)
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
        this.intervalHandle = window.setInterval(this.getCache, 1000);
        this.getCache();
    }

    componentWillUnmount() {
        window.clearInterval(this.intervalHandle);
    }

    // renderers
    renderNav() {
        return (<ul className="nav nav-tabs">
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'requests' ? ' active' : ''}`} href={`/admin/requests`} onClick={this.changeMode}>Requests</a>
            </li>
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'access' ? ' active' : ''}`} href={`/admin/access`} onClick={this.changeMode}>Access</a>
            </li>
            {this.state.myAccess.admin ? (<li className="nav-item">
                <a className={`nav-link${this.state.mode === 'cache' ? ' active' : ''}`} href={`/admin/cache`} onClick={this.changeMode}>Cache</a>
            </li>) : null}
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

        if (!this.state.myAccess.admin && !this.state.myAccess.manager) {
            return (<Alert warnings={[ 'This service is only available for managers and administrators.' ]} />);
        }

        if (this.state.mode === 'requests') {
            return this.renderRequests();
        }
        if (this.state.mode === 'access') {
            return this.renderAccess();
        }
        if (this.state.mode === 'cache') {
            return this.renderCache();
        }

        return (<div>Unknown mode: {this.state.mode}</div>);
    }

    renderCache() {
        return (<div>
            {this.renderNav()}
            <hr />
            <h5>Cache as at: {this.state.cacheAt ? this.state.cacheAt.toLocaleTimeString() : 'not retrieved'}</h5>
            <div className="list-group">
                {this.state.cache ? this.state.cache.map(item => {
                    return (<div key={item.key} className="list-group-item list-group-item-action flex-column align-items-start">
                        {item.key}: Reads: {item.reads}
                    </div>)
                }) : (<div>No cache</div>)}
            </div>
        </div>);
    }

    renderAccess() {
        return (<div>
            {this.renderNav()}
            <hr />
            <div className="list-group">
                {this.state.allAccess.map(access => <AccessOverview key={access.userId} onAccessChanged={this.accessChanged} access={access} teams={this.state.teams} myAccess={this.state.myAccess} />)}
            </div>
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
                <button type="button" className={`btn ${processing ? 'btn-light' : 'btn-danger'}`} data-user-id={request.userId} onClick={this.deleteRequest}>🗑</button>
                &nbsp;
                {request.rejected ? null : respondRequestButton}
            </span>
        </div>);
    }

    //api
    getCache() {
        if (this.state.mode !== 'cache' && this.state.cache !== null) {
            return;
        }

        const cacheCopy = Object.assign({}, Http.cache);
        const orderedCache = [];
        Object.keys(cacheCopy).forEach(key => {
            orderedCache.push(Object.assign({key: key}, cacheCopy[key]));
        });
        orderedCache.sort((a, b) => a.reads - b.reads);

        this.setState({
            cache: orderedCache,
            cacheAt: new Date()
        });
    }

    async getAccessRequests() {
        const requests = await this.accessApi.getAllAccessRequests();
        requests.sort((a, b) => Date.parse(b.requested) - Date.parse(a.requested));

        return requests;
    }

    async getAllAccess(bypassCache) {
        const allAccess = await this.accessApi.getAllAccess(bypassCache);
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
            processing: Functions.union(this.state.processing, userId)
        });

        const result = await this.accessApi.respondToAccessRequest(userId, teamIdOverride || request.teamId, reason, allow);
        if (result.success) {
            this.setState({
                requests: await this.getAccessRequests(),
                allAccess: await this.getAllAccess(),
                processing: Functions.except(this.state.processing, userId)
            });
        }
    }
}
