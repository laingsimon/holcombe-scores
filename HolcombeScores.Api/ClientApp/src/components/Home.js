import React, {Component} from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Team} from '../api/team';
import {Access} from '../api/access';
import {Alert} from "./Alert";
import {Functions} from "../functions";

// noinspection JSUnresolvedVariable
export class Home extends Component {
    constructor(props) {
        super(props);
        this.state = {
            loading: true,
            request: {name: ''},
            mode: props.match.params.mode || 'access',
            recovery: {adminPassCode: ''},
            proposedAccess: {}
        };
        this.requestAccess = this.requestAccess.bind(this);
        this.recoverAccess = this.recoverAccess.bind(this);
        this.requestChanged = this.requestChanged.bind(this);
        this.recoveryChanged = this.recoveryChanged.bind(this);
        this.removeError = this.removeError.bind(this);
        this.changeMode = this.changeMode.bind(this);
        this.updateAccess = this.updateAccess.bind(this);
        this.accessChanged = this.accessChanged.bind(this);
        this.removeAccess = this.removeAccess.bind(this);
        let http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.teamApi = new Team(http);
        this.history = props.history;
    }

    // hooks
    componentDidMount() {
        // noinspection JSIgnoredPromiseFromCall
        this.populateAccessForRecovery();
    }

    //event handlers
    async removeAccess() {
        if (!window.confirm('Are you sure you want to remove your access')) {
            return;
        }

        this.setState({loading: true});

        const result = await this.accessApi.deleteAccess(this.props.access.userId);

        if (result.success) {
            this.setState({mode: 'access'});
            if (this.props.updateAccess) {
                this.props.updateAccess();
            }
        } else {
            alert('Could not delete your details');
        }
    }

    async updateAccess() {
        if (!this.state.proposedAccess.name) {
            alert('You need to enter a name');
            return;
        }

        const currentAccessCopy = Object.assign({}, this.props.access);
        const accessUpdate = Object.assign(currentAccessCopy, this.state.proposedAccess);

        this.setState({loading: true});

        const result = await this.accessApi.updateAccess(accessUpdate.teamId, accessUpdate.userId, accessUpdate.name, accessUpdate.admin, accessUpdate.manager);

        if (result.success) {
            if (this.props.updateAccess) {
                this.props.updateAccess();
            }
        } else {
            alert('Could not update your access');
        }
    }

    accessChanged(event) {
        const name = event.target.getAttribute('name');
        const value = event.target.value;
        const proposedAccess = Object.assign({}, this.state.proposedAccess);
        proposedAccess[name] = value;

        this.setState({
            proposedAccess: proposedAccess
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

    requestAccess() {
        if (!this.state.request.name) {
            alert('You must enter a name');
            return;
        }

        if (!this.state.request.teamId) {
            alert('You must select a team');
            return;
        }

        // noinspection JSIgnoredPromiseFromCall
        this.sendAccessRequest(this.state.request);
    }

    recoverAccess() {
        if (!this.state.recovery.adminPassCode) {
            alert('You must enter the admin pass code');
            return;
        }

        if (!this.state.recovery.recoveryId) {
            alert('You must select an account to recover');
            return;
        }

        this.setState({loading: true});
        // noinspection JSIgnoredPromiseFromCall
        this.sendAccessRecovery(this.state.recovery);
    }

    requestChanged(event) {
        let input = event.target;
        let name = input.name;
        let stateUpdate = {request: this.state.request};
        stateUpdate.request[name] = input.value;
        this.setState(stateUpdate);
    }

    recoveryChanged(event) {
        let input = event.target;
        let name = input.name;
        let stateUpdate = {recovery: this.state.recovery};
        stateUpdate.recovery[name] = input.value;
        this.setState(stateUpdate);
    }

    removeError() {
        this.setState({error: null});
    }

    // renderers
    renderNav() {
        return (<ul className="nav nav-tabs">
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'access' ? ' active' : ''}`} href={`/home/access`}
                   onClick={this.changeMode}>Access</a>
            </li>
            {this.props.access ? null : (<li className="nav-item">
                <a className={`nav-link${this.state.mode === 'recover' ? ' active' : ''}`} href={`/home/recover`}
                   onClick={this.changeMode}>Recover</a>
            </li>)}
            {this.props.access ? (<li className="nav-item">
                <a className={`nav-link${this.state.mode === 'update' ? ' active' : ''}`} href={`/home/update`}
                   onClick={this.changeMode}>Update</a>
            </li>) : null}
        </ul>);
    }

    renderTeams(teams) {
        let setSelectedTeam = function (event) {
            let item = event.target;
            let id = item.getAttribute('data-id');
            let stateUpdate = {request: this.state.request};
            stateUpdate.request.teamId = id;
            this.setState(stateUpdate);
        }.bind(this);

        return teams.map(team => {
            let selected = team.id === this.state.request.teamId;
            return (<li key={team.id} className={`list-group-item ${selected ? ' active' : ''}`} data-id={team.id}
                        onClick={setSelectedTeam}>
                {team.name}
            </li>)
        });
    }

    renderTeam(team) {
        return (<span><strong>{team.name}</strong> (Coach {team.coach})</span>);
    }

    renderRecoveryAccounts(recoveryAccounts) {
        let setSelectedAccount = function (event) {
            let item = event.target;
            let id = item.getAttribute('data-id');
            let stateUpdate = {recovery: this.state.recovery};
            stateUpdate.recovery.recoveryId = id;
            this.setState(stateUpdate);
        }.bind(this);

        return recoveryAccounts.map(recoveryAccount => {
            let className = 'list-group-item' + (recoveryAccount.recoveryId === this.state.recovery.recoveryId ? ' active' : '');
            return (<li key={recoveryAccount.recoveryId} className={className} data-id={recoveryAccount.recoveryId}
                        onClick={setSelectedAccount}>{recoveryAccount.recoveryId} {recoveryAccount.name}</li>)
        });
    }

    renderAccess(access, teams) {
        // access granted
        let team = teams.filter(t => t.id === access.teamId)[0];
        return (<div>
            {this.renderNav()}
            <br/>
            Hello <strong>{access.name}</strong>, you have access to <strong>{this.renderTeam(team)}</strong>
            <br/>
            <a href={`/team/${team.id}/view`} className="btn btn-primary">View Games</a>
        </div>);
    }

    renderAccessRejected(request) {
        return (<div>
            {this.renderNav()}
            <br/>
            <p>Sorry, {request.name}, your access request was rejected.</p>
            <p>Reason: <b>{request.reason ? request.reason : 'No reason given'}</b></p>
        </div>);
    }

    renderAccessPending() {
        return (<div>
            {this.renderNav()}
            <br/>
            <p>Your access request hasn't been approved, yet...</p>
        </div>);
    }

    renderRequestAccess(access, request, teams) {
        if (request && !access) {
            // no access, but requested
            return (<div>
                {this.renderNav()}
                <br/>
                <p>Your access request hasn't been approved, yet...</p>
            </div>);
        }

        if (!teams.length) {
            return (<div>
                {this.renderNav()}
                <br/>
                <p>You don't currently have access. <strong>There are no teams to request access for</strong>.</p>
            </div>);
        }

        return (<div>
            {this.renderNav()}
            <br/>
            <p>You don't currently have access, enter your details below to request access</p>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Your name</span>
                </div>
                <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name"
                       value={this.state.request.name} onChange={this.requestChanged}/>
            </div>

            <p>Select your team</p>
            <ul className="list-group">
                {this.renderTeams(teams)}
            </ul>
            <hr/>
            <button type="button" className="btn btn-primary" onClick={this.requestAccess}>Request access</button>
        </div>);
    }

    renderRecoveryOptions(recoveryAccounts) {
        return (<div>
            {this.renderNav()}
            <br/>
            <p>Pick an account to recover</p>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Admin password</span>
                </div>
                <input type="password" className="form-control" id="basic-url" aria-describedby="basic-addon3"
                       name="adminPassCode" value={this.state.recovery.adminPassCode} onChange={this.recoveryChanged}/>
            </div>

            <p>Select your account</p>
            <ul className="list-group">
                {this.renderRecoveryAccounts(recoveryAccounts)}
            </ul>
            <hr/>
            <button type="button" className="btn btn-primary" onClick={this.recoverAccess}>Recover access</button>
        </div>);
    }

    renderLoading() {
        // show a spinner?
        return (<div className="d-flex justify-content-center">
            <div className="spinner-border" role="status">
                <span className="visually-hidden">Loading...</span>
            </div>
        </div>);
    }

    renderError(error) {
        return (<div>
            <Alert errors={[error]}/>
            <hr/>
            <button type="button" className="btn btn-primary" onClick={this.removeError}>Back</button>
        </div>);
    }

    renderAccessMode() {
        if (this.props.access) {
            return this.renderAccess(this.props.access, this.props.teams);
        }
        if (this.props.request && this.props.request.rejected) {
            return this.renderAccessRejected(this.props.request);
        }
        if (this.props.request && !this.props.access) {
            return this.renderAccessPending();
        }

        return this.renderRequestAccess(this.props.access, this.props.request, this.props.teams);
    }

    renderUpdateMode() {
        return (<div>
            {this.renderNav()}
            <br/>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Your name</span>
                </div>
                <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name"
                       value={this.state.proposedAccess.name} onChange={this.accessChanged}/>
            </div>
            <hr/>
            <button type="button" className="btn btn-primary" onClick={this.updateAccess}>Update details</button>
            &nbsp;
            <button type="button" className="btn btn-danger" onClick={this.removeAccess}>Remove access</button>
        </div>)
    }

    render() {
        try {
            if (this.state.loading) {
                return this.renderLoading();
            } else if (this.state.error) {
                return this.renderError(this.state.error);
            } else if (this.state.mode === 'access') {
                return this.renderAccessMode();
            } else if (this.state.mode === 'recover') {
                return this.renderRecoveryOptions(this.state.recoveryAccounts);
            } else if (this.state.mode === 'update') {
                return this.renderUpdateMode();
            }

            return (<div>Unset: {this.state.mode}</div>);
        } catch (e) {
            console.error(e);
            return (<Alert errors={[`Error rendering component: ${e.message}`]}/>);
        }
    }

    // api access
    async populateAccessForRecovery() {
        try {
            const recoveryAccounts = await this.accessApi.getAccessForRecovery();
            recoveryAccounts.sort(Functions.recoverySortFunction);

            this.setState({
                proposedAccess: Object.assign({}, this.props.access),
                loading: false,
                recoveryAccounts: recoveryAccounts
            });
        } catch (e) {
            console.error(e);
            this.setState({mode: 'access', error: e.message, loading: false});
        }
    }

    async sendAccessRequest(details) {
        this.setState({error: null, loading: true});
        try {
            const data = await this.accessApi.createAccessRequest(details.name, details.teamId);
            if (data.errors && data.errors.length > 0) {
                this.setState({error: data.errors, loading: false});
                return;
            }

            if (this.props.reloadAccess) {
                await this.props.reloadAccess();
            }
        } catch (e) {
            console.error(e);
            this.setState({error: e.message, loading: false});
        }
    }

    async sendAccessRecovery(recovery) {
        this.setState({error: null, loading: true});
        try {
            const data = await this.accessApi.recoverAccess(recovery.recoveryId, recovery.adminPassCode);
            if (data.errors && data.errors.length > 0) {
                this.setState({error: data.errors, loading: false});
                return;
            }

            if (this.props.updateAccess) {
                this.props.updateAccess();
            }
        } catch (e) {
            console.error(e);
            this.setState({error: e.message, loading: false});
        }
    }
}
