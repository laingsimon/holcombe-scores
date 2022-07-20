import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {Team} from '../api/team';
import {GameOverview} from './GameOverview';
import {NewGame} from './NewGame';

export class Games extends Component {
  constructor(props) {
    super(props);
    const http = new Http(new Settings());
    this.gameApi = new Game(http);
    this.teamApi = new Team(http);
    this.teamId = props.match.params.teamId;
    this.history = props.history;
    this.state = {
      loadingData: true,
      loadingNewGame: false,
      games: null,
      error: null,
      team: null
    };
    this.onNewGameLoaded = this.onNewGameLoaded.bind(this);
  }

  componentDidMount() {
    // noinspection JSIgnoredPromiseFromCall
    this.fetchGames();
  }

  // event handlers
  onNewGameLoaded() {
    this.setState({
      loadingNewGame: false,
    });
  }

  // renderers
  render() {
    if (this.state.loadingData || this.state.loadingNewGame) {
      return (<div>Loading...</div>);
    }
    if (this.state.error) {
      return (<div>Error<br /><p>{this.state.error}</p></div>);
    }
    if (this.state.team) {
      return (<div>
        <h2>{this.state.team.name}</h2>
        {this.renderGames(this.state.games, false)}
        <div>
          <hr />
          <h3>Create a new game...</h3>
          <NewGame teamId={this.teamId} onLoaded={this.onNewGameLoaded} />
        </div>
      </div>);
    }

    return this.renderGames(this.state.games, true);
  }

  renderGames(games, showTeam) {
    return (<div className="list-group">
      {games.map(g => (<GameOverview key={g.id} game={g} team={this.getTeam(g)} history={this.history} showTeam={showTeam} />))}
    </div>);
  }

  getTeam(game) {
    return this.state.teams.filter(t => t.id === game.teamId)[0];
  }

  // api access
  async fetchGames() {
    try {
      const allGames = await this.gameApi.getAllGames();
      const games = allGames.filter(g => !this.teamId || g.teamId === this.teamId);
      const teams = await this.teamApi.getAllTeams();
      const team = this.teamId ? teams.filter(t => t.id === this.teamId)[0] : null;
      this.setState({games: games, teams: teams, team: team, loadingData: false});
    } catch (e) {
      console.log(e);
      this.setState({loadingData: false, error: e.message });
    }
  }
}
