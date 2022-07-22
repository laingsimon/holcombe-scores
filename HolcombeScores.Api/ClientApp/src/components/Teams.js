import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Team} from '../api/team';
import {Access} from '../api/access';
import {TeamOverview} from "./TeamOverview";
import {Alert} from "./Alert";

export class Teams extends Component {
  constructor(props) {
    super(props);
    const http = new Http(new Settings());
    this.teamApi = new Team(http);
    this.accessApi = new Access(http);
    this.history = props.history;
    this.state = {
      loading: true,
      teams: null,
      error: null,
      mode: props.match.params.mode || 'view'
    };
    this.changeMode = this.changeMode.bind(this);
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

  componentDidMount() {
    // noinspection JSIgnoredPromiseFromCall
    this.fetchTeams();
  }

  // renderers
  renderNav() {
    return (<ul className="nav nav-pills">
      <li className="nav-item">
        <a className={`nav-link${this.state.mode === 'view' ? ' active' : ''}`} href={`/teams/view`} onClick={this.changeMode}>View Teams</a>
      </li>
      <li className="nav-item">
        <a className={`nav-link${this.state.mode === 'new' ? ' active' : ''}`} href={`/teams/new`} onClick={this.changeMode}>New Team</a>
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
      return (<Alert errors={[ this.state.error ]} />);
    }

    if (this.state.mode === 'view') {
      return this.renderTeams(this.state.teams);
    }
    if (this.state.mode === 'new') {
      return this.renderNewTeam();
    }
  }

  renderNewTeam() {
    return (<div>
      {this.renderNav()}
      <hr />
      <p>Todo: New team</p>
    </div>);
  }

  renderTeams(teams) {
    return (<div>
      {this.renderNav()}
      <hr />
      <div className="list-group">
        {teams.map(team => <TeamOverview key={team.id} team={team} history={this.history} />)}
      </div>
    </div>);
  }

  // api access
  async fetchTeams() {
    try {
      const access = await this.accessApi.getMyAccess();
      if (access.access) {
        const teams = await this.teamApi.getAllTeams();
        this.setState({teams: teams, loading: false});
      } else {
        this.setState({loading: false, error: 'You need tor request access first' });
      }
    } catch (e) {
      console.error(e);
      this.setState({loading: false, error: e.message });
    }
  }
}
