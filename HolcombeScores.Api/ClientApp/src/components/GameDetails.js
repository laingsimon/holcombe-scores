import React, { Component } from 'react';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Game} from '../api/game';
import {Access} from '../api/access';
import {Team} from '../api/team';

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
            error: null
        };
        this.deleteGame = this.deleteGame.bind(this);
    }

    //event handlers
    async deleteGame() {
        if (!window.confirm('Are you sure you want to delete this game?')) {
            return;
        }

        this.setState({
            loading: true
        });

        const result = await this.gameApi.deleteGame(this.gameId);
        if (result.success) {
           document.location.href = '/games';
        }
    }

    componentDidMount() {
        // noinspection JSIgnoredPromiseFromCall
        this.fetchGame();
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
            return <div><h4>Error</h4>{this.state.error}</div>
        }
        if (!this.state.access.access) {
            return <div>
                <h4>Not logged in</h4>
                <a href="/" className="btn btn-primary">Home</a>
            </div>
        }

        const game = this.state.game;
        const location = game.playingAtHome ? 'Home' : 'Away';
        const date = new Date(Date.parse(game.date));
        const holcombeGoals = game.goals.filter(g => g.holcombeGoal).length;
        const opponentGoals = game.goals.filter(g => !g.holcombeGoal).length;
        const score = game.playingAtHome
            ? `${holcombeGoals}-${opponentGoals}`
                : `${opponentGoals}-${holcombeGoals}`;
        const runningScore = {
            holcombe: 0,
            opponent: 0,
        }

        return (<div>
            <h4>
                {this.state.team.name}: {location} to {game.opponent} on {date.toDateString()} <span className="badge rounded-pill bg-primary">{score}</span>
            </h4>
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
            {this.renderOptions()}
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

    renderOptions() {
        if (!this.state.access.access.admin) {
            return null;
        }

        return (<div>
            <hr />
            <button className="btn btn-danger" onClick={this.deleteGame}>Delete game</button>
        </div>)
    }

    renderGoal(goal, game, runningScore) {
        const time = new Date(Date.parse(goal.time));

        if (goal.holcombeGoal) {
            runningScore.holcombe++;
            return (<li>{this.renderRunningScore(runningScore, game.playingAtHome, "bg-success")} - {`${time.getHours()}:${time.getMinutes()}`} - {goal.player.name}</li>);
        }

        runningScore.opponent++;
        return (<li>{this.renderRunningScore(runningScore, game.playingAtHome, "bg-danger")} - {`${time.getHours()}:${time.getMinutes()}`} - {game.opponent}</li>);
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
