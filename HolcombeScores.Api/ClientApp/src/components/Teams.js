import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Team} from '../api/team';
import {Access} from '../api/access';
import {TeamOverview} from "./TeamOverview";

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
      error: null
    }
  }

  componentDidMount() {
    // noinspection JSIgnoredPromiseFromCall
    this.fetchTeams();
  }

  // renderers
  render() {
    if (this.state.loading) {
      return (<div className="d-flex justify-content-center">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>);
    }
    if (this.state.error) {
      return (<div>Error<br /><p>{this.state.error}</p></div>);
    }
    return this.renderTeams(this.state.teams);
  }

  renderTeams(teams) {
    return (<div className="list-group">
      {teams.map(team => <TeamOverview key={team.id} team={team} history={this.history} />)}
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
      console.log(e);
      this.setState({loading: false, error: e.message });
    }
  }
}
