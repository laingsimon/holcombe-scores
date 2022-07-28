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
        this.removeGoal = this.removeGoal.bind(this);
    }

    //event handlers
    async removeGoal(event) {
        const goalId = event.target.getAttribute('data-goal-id');
        const goal = this.state.game.goals.filter(g => g.goalId === goalId)[0];
        const detail = goal.holcombeGoal ? goal.player.name : this.state.game.opponent;

        if (!window.confirm(`Are you sure you want to remove ${detail}'s goal?`)) {
            return;
        }

        await this.gameApi.removeGoal(this.gameId, goalId);
        await this.fetchGame();
    }

    async gameChanged() {
        await this.fetchGame(); // don't set the state to loading
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
        this.fetchGame();
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
        const game = this.state.game;
        const location = game.playingAtHome ? 'Home' : 'Away';
        const date = new Date(Date.parse(game.date));
        const holcombeGoals = game.goals.filter(g => g.holcombeGoal).length;
        const opponentGoals = game.goals.filter(g => !g.holcombeGoal).length;
        const score = game.playingAtHome
            ? `${holcombeGoals}-${opponentGoals}`
            : `${opponentGoals}-${holcombeGoals}`;

        return (<h4>
                {this.state.team.name}: {location} to {game.opponent} on {date.toDateString()} <span className="badge rounded-pill bg-primary">{score}</span>
            </h4>);
    }

    renderViewGame() {
        const game = this.state.game;
        const date = new Date(Date.parse(game.date));
        const runningScore = {
            holcombe: 0,
            opponent: 0,
        }

        game.squad.sort(Functions.playerSortFunction);

        return (<div>
            {this.renderHeading()}
            {this.renderNav()}
            <hr />
            <h6>Start time: {date.toLocaleTimeString()}</h6>
            <div>
                <h5>Goals</h5>
                {this.renderGoals(game, runningScore)}
            </div>
            <div>
                <h5>Holcombe Players</h5>
                <ul>
                    {game.squad.map(p => this.renderPlayer(p))}
                </ul>
            </div>
        </div>);
    }

    renderEditGame() {
        return (<div>
            {this.renderHeading()}
            {this.renderNav()}
            <hr />
            <EditGame teamId={this.state.team.id} gameId={this.state.game.id} onChanged={this.gameChanged} />
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
            <PlayGame teamId={this.state.team.id} gameId={this.state.game.id} onChanged={this.gameChanged} />
        </div>);
    }

    renderGoals(game, runningScore) {
        if (game.goals.length === 0) {
            return (<p>No goals</p>);
        }

        game.goals.map(goal => {
            goal.jsTime = new Date(goal.time);
            return goal;
        });
        game.goals.sort((a, b) => a.jsTime - b.jsTime);

        return (<ol>
            {game.goals.map(g => this.renderGoal(g, game, runningScore))}
        </ol>);
    }

    renderGoal(goal, game, runningScore) {
        const time = new Date(Date.parse(goal.time)).toTimeString().substring(0, 5);

        if (goal.holcombeGoal) {
            runningScore.holcombe++;
            return (<li key={goal.goalId}>{this.renderRunningScore(runningScore, game.playingAtHome, "bg-success")} - {`${time}`} - {goal.player.name} <button className="delete-goal" data-goal-id={goal.goalId} onClick={this.removeGoal}>🗑</button></li>);
        }

        runningScore.opponent++;
        return (<li key={goal.goalId}>{this.renderRunningScore(runningScore, game.playingAtHome, "bg-danger")} - {`${time}`} - {game.opponent} <button className="delete-goal" data-goal-id={goal.goalId} onClick={this.removeGoal}>🗑</button></li>);
    }

    renderRunningScore(runningScore, playingAtHome, colour) {
        const score = playingAtHome
            ? `${runningScore.holcombe} - ${runningScore.opponent}`
            : `${runningScore.opponent} - ${runningScore.holcombe}`;

        return (<span className={`badge rounded-pill ${colour}`}>{score}</span>);
    }

    renderPlayer(player) {
        return (<li key={player.number}><span className="badge rounded-pill bg-primary">{player.number}</span> {player.name}</li>);
    }

    // api access
    async fetchGame() {
        try {
            const game = await this.gameApi.getGame(this.gameId);
            if (!game) {
                this.setState({loading: false, error: 'Game not found, or no access to game' });
                return;
            }

            const access = await this.accessApi.getMyAccess();
            const team = await this.teamApi.getTeam(game.teamId);
            this.setState({game: game, access:access,team: team, loading: false});
        } catch (e) {
            console.log(e);
            this.setState({loading: false, error: e.message });
        }
    }
}
