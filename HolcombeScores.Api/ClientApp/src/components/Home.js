import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Team} from '../api/team';
import {Access} from '../api/access';
import {Alert} from "./Alert";

export class Home extends Component {
  constructor(props) {
    super(props);
    this.state = { access: null, loading: true, request: { name: '' }, mode: 'access', recovery: { adminPassCode: '' } };
    this.requestAccess = this.requestAccess.bind(this);
    this.recoverAccess = this.recoverAccess.bind(this);
    this.requestChanged = this.requestChanged.bind(this);
    this.recoveryChanged = this.recoveryChanged.bind(this);
    this.removeError = this.removeError.bind(this);
    let http = new Http(new Settings());
    this.accessApi = new Access(http);
    this.teamApi = new Team(http);
    this.history = props.history;
  }

  // hooks
  componentDidMount() {
    // noinspection JSIgnoredPromiseFromCall
    this.populateMyAccess();
  }

  //event handlers
  requestAccess() {
    if (this.state.mode !== 'access') {
      this.setState({ mode: 'access' });
      return;
    }

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
    if (this.state.mode !== 'recover') {
      this.setState({ mode: 'recover', loading: true });
      // noinspection JSIgnoredPromiseFromCall
      this.populateRecoveryOptions();
      return;
    }

    if (!this.state.recovery.adminPassCode) {
      alert('You must enter the admin pass code');
      return;
    }

    if (!this.state.recovery.recoveryId) {
      alert('You must select an account to recover');
      return;
    }

    this.setState({ mode: 'recover', loading: true });
    // noinspection JSIgnoredPromiseFromCall
    this.sendAccessRecovery(this.state.recovery);
  }

  requestChanged(event) {
    let input = event.target;
    let name = input.name;
    let stateUpdate = { request: this.state.request };
    stateUpdate.request[name] = input.value;
    this.setState(stateUpdate);
  }

  recoveryChanged(event) {
    let input = event.target;
    let name = input.name;
    let stateUpdate = { recovery: this.state.recovery };
    stateUpdate.recovery[name] = input.value;
    this.setState(stateUpdate);
  }

  removeError() {
    this.setState({ error: null });
  }

  // renderers
  renderTeams(teams) {
    let setSelectedTeam = function(event) {
      let item = event.target;
      let id = item.getAttribute('data-id');
      let stateUpdate = { request: this.state.request };
      stateUpdate.request.teamId = id;
      this.setState(stateUpdate);
    }.bind(this);

    return teams.map(team => {
      let selected = team.id === this.state.request.teamId;
      return (<li key={team.id} className={`list-group-item ${selected ? ' active' : ''}`} data-id={team.id} onClick={setSelectedTeam}>
        {team.name}
      </li>) });
  }

  renderTeam(team) {
    return (<span><strong>{team.name}</strong> (Coach {team.coach})</span>);
  }

  renderRecoveryAccounts(recoveryAccounts) {
    let setSelectedAccount = function(event) {
      let item = event.target;
      let id = item.getAttribute('data-id');
      let stateUpdate = { recovery: this.state.recovery };
      stateUpdate.recovery.recoveryId = id;
      this.setState(stateUpdate);
    }.bind(this);

    return recoveryAccounts.map(recoveryAccount => {
      let className = 'list-group-item' + (recoveryAccount.recoveryId === this.state.recovery.recoveryId ? ' active' : '');
      return (<li key={recoveryAccount.recoveryId} className={className} data-id={recoveryAccount.recoveryId} onClick={setSelectedAccount}>{recoveryAccount.recoveryId} {recoveryAccount.name}</li>) });
  }

  renderAccess(access, teams) {
    // access granted
    let team = teams.filter(t => t.id === access.access.teamId)[0];
    return (<div>
      Hello <strong>{access.access.name}</strong>, you have access to <a href={`/team/${team.id}`} onClick={this.showGames}>{this.renderTeam(team)}</a>
      <hr />
      <a href={`/team/${team.id}`} className="btn btn-primary">Show games</a>
    </div>);
  }

  renderRequestAccess(access, teams) {
    if (access.request && !access.access) {
      // no access, but requested
      return (<p>Your access request hasn't been approved, yet...</p>);
    }

    if (!teams.length) {
      return (<p>You don't currently have access. <strong>There are no teams to request access for</strong>.</p>);
    }

    return (<div>
      <p>You don't currently have access, enter your details below to request access</p>
      <div className="input-group mb-3">
        <div className="input-group-prepend">
          <span className="input-group-text" id="basic-addon3">Your name</span>
        </div>
        <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name" value={this.state.request.name} onChange={this.requestChanged} />
      </div>

      <p>Select your team</p>
      <ul className="list-group">
        {this.renderTeams(teams)}
      </ul>
      <hr />
      <button type="button" className="btn btn-primary" onClick={this.requestAccess}>Request access</button>
      <button type="button" className="btn btn-light" onClick={this.recoverAccess}>Recover access</button>
    </div>);
  }

  renderRecoveryOptions(recoveryAccounts) {
    return (<div>
      <p>Pick an account to recover</p>
      <div className="input-group mb-3">
        <div className="input-group-prepend">
          <span className="input-group-text" id="basic-addon3">Admin password</span>
        </div>
        <input type="password" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="adminPassCode" value={this.state.recovery.adminPassCode} onChange={this.recoveryChanged} />
      </div>

      <p>Select your team</p>
      <ul className="list-group">
        {this.renderRecoveryAccounts(recoveryAccounts)}
      </ul>
      <hr />
      <button type="button" className="btn btn-light" onClick={this.requestAccess}>Request access</button>
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
      <Alert errors={[ error ]} />
      <hr />
      <button type="button" className="btn btn-primary" onClick={this.removeError}>Back</button>
    </div>);
  }

  renderCreateAccessRequest(access, teams) {
    return (<div>
      <h1>Request access</h1>
      {this.renderRequestAccess(access, teams)}
    </div>);
  }

  renderRecoverAccess(recoveryAccounts) {
    return (<div>
      <h1>Recover access</h1>
      {this.renderRecoveryOptions(recoveryAccounts)}
    </div>);
  }

  render () {
    try {
      if (this.state.loading) {
        return this.renderLoading();
      } else if (this.state.error) {
        return this.renderError(this.state.error);
      } else if (this.state.mode === 'access' && this.state.access && ((this.state.access.request && this.state.access.access) || (this.state.access.access && this.state.access.access.admin))) {
        return this.renderAccess(this.state.access, this.state.teams);
      } else if (this.state.mode === 'access' && this.state.access) {
        return this.renderCreateAccessRequest(this.state.access, this.state.teams);
      } else if (this.state.mode === 'access') {
        return (<div>Unable to retrieve your access, please check your internet connection</div>);
      } else if (this.state.mode === 'recover') {
        return this.renderRecoverAccess(this.state.recoveryAccounts);
      }

      return (<div>Unset: {this.state.mode}</div>);
    } catch (e) {
      console.error(e);
      return (<Alert errors={[ `Error rendering component: ${e.message}` ]} />);
    }
  }

  // api access
  async populateMyAccess() {
    try {
      const access = await this.accessApi.getMyAccess();
      const teams = await this.teamApi.getAllTeams();

      this.setState({ mode: 'access', access: access, teams: teams, loading: false});
    } catch (e) {
      console.error(e);
      this.setState({ mode: 'access', error: e.message, loading: false});
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

      await this.populateMyAccess(); // reload the component
    } catch (e) {
      console.error(e);
      this.setState({error: e.message, loading: false});
    }
  }

  async populateRecoveryOptions() {
    try {
      const recoveryAccounts = await this.accessApi.getAccessForRecovery();

      this.setState({recoveryAccounts: recoveryAccounts, loading: false});
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

      await this.populateMyAccess(); // reload the component
    } catch (e) {
      console.error(e);
      this.setState({ error: e.message, loading: false});
    }
  }
}
