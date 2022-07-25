import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {Team} from '../api/team';
import {Functions} from '../functions'

export class PlayGame extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.teamApi = new Team(http);
        this.state = {
            loading: true,
            latestScorer: null,
        };
        this.holcombeGoal = this.holcombeGoal.bind(this);
        this.opponentGoal = this.opponentGoal.bind(this);
        this.updateGameData = this.updateGameData.bind(this);
        this.stopRefresh = this.stopRefresh.bind(this);
    }

    // event handler
    stopRefresh() {
        window.clearInterval(this.state.refreshHandle);
        this.setState({
            refreshHandle: null
        })
    }

    async opponentGoal() {
        this.setState({
            latestScorer: 'opponent'
        });

        await this.recordGoal(false, null);
    }

    async holcombeGoal(event) {
        const playerNumber = event.target.getAttribute('data-player-number');

        if (this.state.readOnly) {
            alert('Game has finished, no goals can be recorded');
            return;
        }

        this.setState({
            latestScorer: Number.parseInt(playerNumber)
        });

        await this.recordGoal(true, playerNumber);
    }

    async recordGoal(holcombeGoal, playerNumber) {
        await this.gameApi.recordGoal(this.props.gameId, new Date().toISOString(), holcombeGoal, playerNumber);
        await this.updateGameData();
        if (this.props.onChanged) {
            this.props.onChanged(this.props.gameId);
        }

        window.setTimeout(() => {
            this.setState({
                latestScorer: null
            });
        }, 1500);
    }

    componentDidMount() {
        // noinspection JSIgnoredPromiseFromCall
        this.getData();
        this.setState({
            refreshHandle: window.setInterval(this.updateGameData, 5000)
        });
    }

    componentWillUnmount() {
        this.stopRefresh();
    }

    render() {
        if (this.state.loading) {
            return (<div className="d-flex justify-content-center" style={{ height: '247px' }}>
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>);
        }

        const notRefreshStatus = this.state.readOnly
            ? 'Game over'
            : 'Not refreshing';

        return (<div>
            {this.renderScore()}
            <div className="d-flex flex-wrap justify-content-center score-goals-container">
                {this.state.game.squad.map(player => this.renderHolcombeScoreButton(player))}
                {this.renderOpponentScoreButton()}
            </div>
            <hr />
            <div className="text-center">
                Score as at {this.state.asAt.toLocaleTimeString()}
                &nbsp;-&nbsp;
                {this.state.refreshHandle ? (<button className="btn btn-secondary" onClick={this.stopRefresh}>Stop Refresh</button>) : <span>{notRefreshStatus}</span>}
            </div>
        </div>);
    }

    renderHolcombeScoreButton(player) {
        const isLatestScorer = this.state.latestScorer === player.number;
        const colour = this.state.readOnly ? 'btn-light' : 'btn-primary';

        return (<button key={player.number} type="button" className={`btn ${isLatestScorer ? ' btn-outline-success' : colour} btn-goal-scorer`} onClick={this.holcombeGoal} data-player-number={player.number}>
            {player.name} Scored!
        </button>);
    }

    renderOpponentScoreButton() {
        if (this.state.readOnly) {
            return null;
        }

        const isLatestScorer = this.state.latestScorer === 'opponent';
        return (<button key="opponent" type="button" className={`btn ${isLatestScorer ? ' btn-outline-success' : 'btn-secondary'} btn-goal-scorer`} onClick={this.opponentGoal}>{this.state.game.opponent} Scored</button>);
    }

    renderScore() {
        const game = this.state.game;
        const holcombeGoals = game.goals.filter(g => g.holcombeGoal).length;
        const opponentGoals = game.goals.filter(g => !g.holcombeGoal).length;
        const score = game.playingAtHome
            ? `${holcombeGoals} - ${opponentGoals}`
            : `${opponentGoals} - ${holcombeGoals}`;
        const winning = holcombeGoals > opponentGoals;
        const drawing = holcombeGoals === opponentGoals;
        const colour = winning ? 'bg-success' : (drawing ? 'bg-primary' : 'bg-danger');

        return (<h4 className="text-center"><span className={`rounded-pill play-current-score ${colour}`}>{score}</span></h4>);
    }

    async updateGameData() {
        this.setState(await this.getGameData(true));
    }

    async getGameData(bypassCache) {
        const game = await this.gameApi.getGame(this.props.gameId, bypassCache);
        game.squad.sort(Functions.playerSortFunction);

        const asAt = new Date();
        const date = new Date(game.date);
        const timeDiff = asAt.getTime() - date.getTime();
        const hourDiff = Math.floor(timeDiff / 1000 / 60 / 60);
        const dayDiff = Math.floor(hourDiff / 24);
        const readOnly = dayDiff > 2;

        if (readOnly && this.state.refreshHandle) {
            this.stopRefresh();
        }

        return {
            game: game,
            asAt: asAt,
            dayDiff: dayDiff,
            readOnly: readOnly
        };
    }

    async getData() {
        const team = await this.teamApi.getTeam(this.props.teamId);
        const gameData = await this.getGameData(false);
        const stateData = Object.assign({
            asAt: new Date(),
            team: team,
            loading: false,
        }, gameData);

        this.setState(stateData);
    }
}
