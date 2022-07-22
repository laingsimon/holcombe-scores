import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {Team} from '../api/team';
import {Access} from '../api/access';
import {GameOverview} from './GameOverview';
import {EditGame} from './EditGame';
import {EditTeam} from './EditTeam';
import {Alert} from './Alert';

export class TeamDetails extends Component {
  constructor(props) {
    super(props);
    const http = new Http(new Settings());
    this.gameApi = new Game(http);
    this.teamApi = new Team(http);
    this.accessApi = new Access(http);
    this.teamId = props.match.params.teamId;
    this.history = props.history;
    this.state = {
      loadingGames: true,
      loadingNewGame: false,
      games: null,
      error: null,
      team: null,
      mode: props.match.params.mode || 'view'
    };
    this.onNewGameLoaded = this.onNewGameLoaded.bind(this);
    this.changeMode = this.changeMode.bind(this);
    this.reloadGames = this.reloadGames.bind(this);
    this.reloadTeam = this.reloadTeam.bind(this);
  }

  componentDidMount() {
    // noinspection JSIgnoredPromiseFromCall
    this.fetchGames();
  }

  // event handlers
  async reloadGames() {
    this.setState({
      loadingGames: true,
    });

    await this.fetchGames();
  }

  async reloadTeam() {
    const team = await this.teamApi.getTeam(this.teamId);
    this.setState({team: team });
  }

  onNewGameLoaded() {
    this.setState({
      loadingNewGame: false,
    });
  }

  changeMode(event) {
    event.preventDefault();
    const url = event.target.getAttribute('href');
    const segments = url.split('/')
    const mode = segments[segments.length - 1];
    this.setState({
      mode: mode,
      loadingNewGame: mode === 'new-game',
    });
  }

  // renderers
  renderNav() {
    return (<ul className="nav nav-pills">
      <li className="nav-item">
        <a className={`nav-link${this.state.mode === 'view' ? ' active' : ''}`} href={`/team/${this.teamId}/view`} onClick={this.changeMode}>View Games</a>
      </li>
      <li className="nav-item">
        <a className={`nav-link${this.state.mode === 'new-game' ? ' active' : ''}`} href={`/team/${this.teamId}/new-game`} onClick={this.changeMode}>New Game</a>
      </li>
      <li className="nav-item">
        <a className={`nav-link${this.state.mode === 'edit' ? ' active' : ''}`} href={`/team/${this.teamId}/edit`} onClick={this.changeMode}>Edit Team</a>
      </li>
    </ul>);
  }

  render() {
    if (this.state.error) {
      return (<Alert errors={[ this.state.error ]} />);
    }

    if (!this.state.team) {
      return (<div className="d-flex justify-content-center">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>);
    }

    if (!this.state.access) {
      return (<Alert warnings={[ "You need to login again, click on 'Home'" ]} />);
    }

    if (this.state.mode === 'view') {
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
        <EditGame teamId={this.teamId} onLoaded={this.onNewGameLoaded} onChanged={this.reloadGames} />
      </div>);
    } else if (this.state.mode === 'edit') {
      return (<div>
        <h3>{this.state.team.name}</h3>
        {this.renderNav()}
        <hr />
        <EditTeam teamId={this.teamId} onChanged={this.reloadTeam} />
      </div>);
    }

    return (<div>
      <h3>{this.state.team.name}</h3>
      {this.renderNav()}
      <hr />
      <Alert warnings={[ `Unknown mode ${this.state.mode}` ]} />
    </div>);
  }

  renderGames(games) {
    if (this.state.loadingGames) {
      return (<div className="d-flex justify-content-center">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>);
    }

    return (<div className="list-group">
      {games.map(g => (<GameOverview key={g.id} game={g} team={this.state.team} history={this.history} />))}
    </div>);
  }

  // api access
  async fetchGames() {
    try {
      const allGames = await this.gameApi.getAllGames();
      const games = allGames.filter(g => !this.teamId || g.teamId === this.teamId);
      const team = await this.teamApi.getTeam(this.teamId);
      const access = await this.accessApi.getMyAccess();
      this.setState({games: games, team: team, access: access.access, loadingGames: false});
    } catch (e) {
      console.error(e);
      this.setState({loadingGames: false, error: e.message });
    }
  }
}
