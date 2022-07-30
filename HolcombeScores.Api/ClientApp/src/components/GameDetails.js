import React, { Component } from 'react';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Game} from '../api/game';
import {Access} from '../api/access';
import {Team} from '../api/team';
import {Alert} from './Alert';
import {EditGame} from "./EditGame";
import {PlayGame} from "./PlayGame";
import {Functions} from '../functions'
import {Score} from "./Score";
import {ViewGame} from "./ViewGame";

export class GameDetails extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.accessApi = new Access(http);
        this.teamApi = new Team(http);
        this.history = props.history;
        this.gameId = props.match.params.gameId;
        this.state = {
            loading: true,
            game: null,
            team: null,
            error: null,
            mode: props.match.params.mode || 'view'
        };
        this.changeMode = this.changeMode.bind(this);
        this.gameChanged = this.gameChanged.bind(this);
        this.onGoalChanged = this.onGoalChanged.bind(this);

    }

    //event handlers
    async onGoalChanged() {
        await this.updateGame(); // don't set the state to loading
    }

    async gameChanged(gameId, holcombeGoal, playerId) {
        if (!gameId) {
            // refresh
            await this.updateGame(); // don't set the state to loading
            return;
        }

        const game = Object.assign({}, this.state.game);
        game.goals.push({
            time: null,
            holcombeGoal: holcombeGoal,
            gameId: gameId,
            player: holcombeGoal
                ? {
                    id: playerId,
                    teamId: game.teamId
                }
                : null
        });

        this.setState({
            game: game
        });

        await this.updateGame(); // don't set the state to loading
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

    componentDidMount() {
        // noinspection JSIgnoredPromiseFromCall
        this.fetchAllData();
    }

    // renderers
    renderNav() {
        const editNav = <li className="nav-item">
            <a className={`nav-link${this.state.mode === 'edit' ? ' active' : ''}`} href={`/game/${this.gameId}/edit`} onClick={this.changeMode}>Edit Game</a>
        </li>;

        return (<ul className="nav nav-tabs">
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'view' ? ' active' : ''}`} href={`/game/${this.gameId}/view`} onClick={this.changeMode}>View Game</a>
            </li>
            {this.state.access.access.admin || this.state.access.access.manager ? editNav : null}
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'play' ? ' active' : ''}`} href={`/game/${this.gameId}/play`} onClick={this.changeMode}>Play Game</a>
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
                <Alert errors={[ this.state.error ]} />
                <a className="btn btn-primary" href="/">Home</a>
            </div>);
        }
        if (!this.state.access.access) {
            return <div>
                <h4>Not logged in</h4>
                <a href="/" className="btn btn-primary">Home</a>
            </div>
        }

        if (this.state.mode === 'view') {
            return this.renderViewGame();
        } else if (this.state.mode === 'edit') {
            return this.renderEditGame();
        } else if (this.state.mode === 'play') {
            return this.renderPlayGame();
        } else {
            return (<div>
                {this.renderHeading()}
                {this.renderNav()}
                <hr />
                <Alert warnings={[ `Unknown mode ${this.state.mode}` ]} />
            </div>);
        }
    }

    renderHeading() {
        const location = this.state.game.playingAtHome ? 'Home' : 'Away';
        const date = new Date(Date.parse(this.state.game.date));
        const score = {
            holcombe: this.state.game.goals.filter(g => g.holcombeGoal).length,
            opponent: this.state.game.goals.filter(g => !g.holcombeGoal).length
        };

        return (<h4>
                {this.state.team.name}: {location} to {this.state.game.opponent} on {date.toDateString()} <Score playingAtHome={this.state.game.playingAtHome} score={score} />
            </h4>);
    }

    renderViewGame() {
        return (<div>
            {this.renderHeading()}
            {this.renderNav()}
            <hr />
            <ViewGame game={this.state.game} readOnly={this.state.readOnly} />
        </div>);
    }

    renderEditGame() {
        return (<div>
            {this.renderHeading()}
            {this.renderNav()}
            <hr />
            <EditGame team={this.state.team} game={this.state.game} onChanged={this.gameChanged} />
        </div>);
    }

    renderPlayGame() {
        const game = this.state.game;
        const homeTeam = game.playingAtHome ? this.state.team.name : game.opponent;
        const awayTeam = game.playingAtHome ? game.opponent : this.state.team.name;

        return (<div>
            <h4>{homeTeam} vs {awayTeam}</h4>
            {this.renderNav()}
            <hr />
            <PlayGame team={this.state.team} game={this.state.game} readOnly={this.state.readOnly} onChanged={this.gameChanged} asAt={this.state.asAt} />
        </div>);
    }

    // api access
    isReadOnly(game, asAt) {
        const date = new Date(game.date);
        const timeDiff = asAt.getTime() - date.getTime();
        const hourDiff = Math.floor(timeDiff / 1000 / 60 / 60);
        const dayDiff = Math.floor(hourDiff / 24);
        return dayDiff > 2;
    }

    async updateGame() {
        const game = await this.gameApi.getGame(this.gameId);
        if (!game) {
            this.setState({loading: false, error: 'Game not found, or no access to game' });
            return;
        }

        game.squad.sort(Functions.playerSortFunction);
        const asAt = new Date();
        this.setState({ game: game, asAt: asAt, readOnly: this.isReadOnly(game, asAt) });
    }

    async fetchAllData() {
        try {
            const game = await this.gameApi.getGame(this.gameId);
            if (!game) {
                this.setState({loading: false, error: 'Game not found, or no access to game' });
                return;
            }

            const access = await this.accessApi.getMyAccess();
            const team = await this.teamApi.getTeam(game.teamId);
            const asAt = new Date();
            game.squad.sort(Functions.playerSortFunction);

            this.setState({
                game: game,
                asAt: asAt,
                readOnly: this.isReadOnly(game, asAt),
                access:access,
                team: team,
                loading: false
            });
        } catch (e) {
            console.log(e);
            this.setState({loading: false, error: e.message });
        }
    }
}
