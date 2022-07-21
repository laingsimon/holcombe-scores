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
      loadingGames: true,
      loadingNewGame: false,
      games: null,
      error: null,
      team: null,
      mode: 'view-games'
    };
    this.onNewGameLoaded = this.onNewGameLoaded.bind(this);
    this.changeMode = this.changeMode.bind(this);
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

  changeMode(event) {
    event.preventDefault();
    const mode = event.target.getAttribute('href');
    this.setState({
      mode: mode,
      loadingNewGame: mode === 'new-game',
    });
  }

  // renderers
  renderNav() {
    return (<ul className="nav nav-pills">
      <li className="nav-item">
        <a className={`nav-link${this.state.mode === 'view-games' ? ' active' : ''}`} aria-current="page" href="view-games" onClick={this.changeMode}>View Games</a>
      </li>
      <li className="nav-item">
        <a className={`nav-link${this.state.mode === 'new-game' ? ' active' : ''}`} href="new-game" onClick={this.changeMode}>New Game</a>
      </li>
    </ul>);
  }

  render() {
    if (this.state.error) {
      return (<div>Error<br /><p>{this.state.error}</p></div>);
    }

    if (!this.state.team) {
      return (<div className="d-flex justify-content-center">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>);
    }

    if (this.state.mode === 'view-games') {
      return (<div>
        <h3>{this.state.team.name}</h3>
        {this.renderNav()}
        <hr />
        {this.renderGames(this.state.games)}
      </div>);
    } else if (this.state.mode === 'new-game') {
      return (<div>
        <h3>{this.state.team.name}</h3>
        {this.renderNav()}
        <hr />
        <NewGame teamId={this.teamId} onLoaded={this.onNewGameLoaded} />
      </div>)
    }

    return (<div>Unknown mode: {this.state.mode}</div>);
  }

  renderGames(games) {
    if (this.state.loadingGames) {
      return (<div className="d-flex justify-content-center">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>)
    }

    return (<div className="list-group">
      {games.map(g => (<GameOverview key={g.id} game={g} team={this.getTeam(g)} history={this.history} />))}
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
      this.setState({games: games, teams: teams, team: team, loadingGames: false});
    } catch (e) {
      console.log(e);
      this.setState({loadingGames: false, error: e.message });
    }
  }
}
