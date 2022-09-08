import React, { Component } from 'react';
import {Http} from '../../api/http';
import {Settings} from '../../api/settings';
import {Access} from '../../api/access';
import {Team} from '../../api/team';
import {Alert} from '../Alert';
import {AccessOverview} from './AccessOverview';
import {RequestOverview} from '../access/RequestOverview';

/*
* Props:
* - access
* - teams
*
* Events:
* -none-
*/
export class Admin extends Component {
    constructor (props) {
        super(props);
        const http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.teamApi = new Team(http);
        this.state = {
            loading: true,
            error: null,
            mode: props.match.params.mode || 'requests',
            teams: {},
            requests: null,
            allAccess: null,
            processing: [],
            unimpersonating: false
        };
        this.changeMode = this.changeMode.bind(this);
        this.accessChanged = this.accessChanged.bind(this);
        this.requestChanged = this.requestChanged.bind(this);
        this.accessImpersonated = this.accessImpersonated.bind(this);
        this.unimpersonate = this.unimpersonate.bind(this);
    }

    //event handlers
    async accessImpersonated() {
        this.setState({
            loading: true
        });

        await this.props.reloadAll();

        this.setState({
            allAccess: await this.getAllAccess(true),
            loading: false
        });
    }

    async accessChanged() {
        this.setState({
            allAccess: await this.getAllAccess(true),
        });
    }

    async requestChanged() {
        this.setState({
            requests: await this.getAccessRequests(),
            allAccess: await this.getAllAccess(),
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

    async unimpersonate() {
        if (!window.confirm('Are you sure you want to unimpersonate?')) {
            return;
        }

        this.setState({
            unimpersonating: true
        });

        try {
            await this.accessApi.unimpersonate();

            this.setState({
                loading: true
            });

            await this.props.reloadAll();

            this.setState({
                unimpersonating: false,
                loading: false,
                allAccess: await this.getAllAccess(true),
            });
        } catch (e) {
            this.setState({
                unimpersonating: false
            });

            alert(e);
        }
    }

    async componentDidMount() {
        await this.requestData();
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
        </ul>);
    }

    render() {
        if (this.state.loading) {
            return (<div className="d-flex justify-content-center">
                <div className="spinner-border spinner-football" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>);
        }

        if (this.state.error) {
            return (<div>
                {this.renderNav()}
                <br />
                <Alert errors={[ this.state.error ]} />
            </div>);
        }

        if (!this.props.access.admin && !this.props.access.manager) {
            return (<Alert warnings={[ 'This service is only available for managers and administrators.' ]} />);
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
            <br />
            <h5>Active</h5>
            <div className="list-group">
                {this.state.allAccess.filter(a => !a.revoked).map(access => this.renderAccessOverview(access))}
            </div>
            <h5>Canceled</h5>
            <div className="list-group">
                {this.state.allAccess.filter(a => a.revoked).map(access => this.renderAccessOverview(access))}
            </div>
            {this.props.isImpersonated ? (<button className="btn btn-secondary" onClick={this.unimpersonate}>
                {this.state.unimpersonating ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                Unimpersonate
            </button>) : null}
        </div>);
    }

    renderAccessOverview(access) {
        return (<AccessOverview key={access.userId} onAccessChanged={this.accessChanged} onAccessRevoked={this.accessChanged} access={access} teams={this.state.teams}
                        myAccess={this.props.access} onAccessImpersonated={this.accessImpersonated} />);
    }

    renderRequests() {
        return (<div>
            {this.renderNav()}
            <br />
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
        return (<RequestOverview key={request.userId + ':' + request.teamId} request={request} teams={this.state.teams} onRequestChanged={this.requestChanged} onRequestDeleted={this.requestChanged} />);
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
            const teamsMap = {};
            this.props.teams.forEach(team => {
                teamsMap[team.id] = team;
            });

            this.setState({
                loading: false,
                requests: await this.getAccessRequests(),
                allAccess: await this.getAllAccess(),
                teams: teamsMap
            });
        } catch (e) {
            console.error(e);
            this.setState({
                error: e,
                loading: false
            });
        }
    }
}
