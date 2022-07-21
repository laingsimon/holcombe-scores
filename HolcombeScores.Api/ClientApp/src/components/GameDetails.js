import React, { Component } from 'react';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Game} from '../api/game';
import {Access} from '../api/access';
import {Team} from '../api/team';
import {Alert} from './Alert';
import {EditGame} from "./EditGame";
import {PlayGame} from "./PlayGame";

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
            mode: 'view-game'
        };
        this.changeMode = this.changeMode.bind(this);
        this.gameChanged = this.gameChanged.bind(this);
    }

    //event handlers
    async gameChanged() {
        await this.fetchGame(); // don't set the state to loading
    }

    changeMode(event) {
        event.preventDefault();
        const mode = event.target.getAttribute('href');
        this.setState({
            mode: mode,
        });
    }

    componentDidMount() {
        // noinspection JSIgnoredPromiseFromCall
        this.fetchGame();
    }

    // renderers
    renderNav() {
        const editNav = <li className="nav-item">
            <a className={`nav-link${this.state.mode === 'edit-game' ? ' active' : ''}`} href="edit-game" onClick={this.changeMode}>Edit Game</a>
        </li>;

        return (<ul className="nav nav-pills">
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'view-game' ? ' active' : ''}`} href="view-game" onClick={this.changeMode}>View Game</a>
            </li>
            {this.state.access.access.admin ? editNav : null}
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'play-game' ? ' active' : ''}`} href="play-game" onClick={this.changeMode}>Play Game</a>
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
        if (!this.state.access.access) {
            return <div>
                <h4>Not logged in</h4>
                <a href="/" className="btn btn-primary">Home</a>
            </div>
        }

        if (this.state.mode === 'view-game') {
            return this.renderViewGame();
        } else if (this.state.mode === 'edit-game') {
            return this.renderEditGame();
        } else if (this.state.mode === 'play-game') {
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
        return (<div>
            {this.renderHeading()}
            {this.renderNav()}
            <hr />
            <PlayGame teamId={this.state.team.id} gameId={this.state.game.id} onChanged={this.gameChanged} />
        </div>);
    }

    renderGoals(game, runningScore) {
        if (game.goals.length === 0) {
            return (<p>No goals</p>);
        }

        return (<ol>
            {game.goals.map(g => this.renderGoal(g, game, runningScore))}
        </ol>);
    }

    renderGoal(goal, game, runningScore) {
        const time = new Date(Date.parse(goal.time));

        if (goal.holcombeGoal) {
            runningScore.holcombe++;
            return (<li key={goal.time}>{this.renderRunningScore(runningScore, game.playingAtHome, "bg-success")} - {`${time.getHours()}:${time.getMinutes()}`} - {goal.player.name}</li>);
        }

        runningScore.opponent++;
        return (<li key={goal.time}>{this.renderRunningScore(runningScore, game.playingAtHome, "bg-danger")} - {`${time.getHours()}:${time.getMinutes()}`} - {game.opponent}</li>);
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
            const access = await this.accessApi.getMyAccess();
            const teams = await this.teamApi.getAllTeams();
            const team = teams.filter(t => t.id === game.teamId)[0];
            this.setState({game: game, access:access,team: team, loading: false});
        } catch (e) {
            console.log(e);
            this.setState({loading: false, error: e.message });
        }
    }
}
