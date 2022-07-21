import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {Team} from '../api/team';

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
    }
    
    // event handler
    async opponentGoal() {
        this.setState({
            latestScorer: 'opponent' 
        });
        
        await this.recordGoal(false, null);
    }
    
    async holcombeGoal(event) {
        const playerNumber = event.target.getAttribute('data-player-number');

        this.setState({
            latestScorer: Number.parseInt(playerNumber)
        });

        await this.recordGoal(true, playerNumber);
    }

    async recordGoal(holcombeGoal, playerNumber) {
        await this.gameApi.recordGoal(this.props.gameId, new Date().toISOString(), holcombeGoal, playerNumber);
        this.getGameData();
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
        this.getGameData();
    }

    render() {
        if (this.state.loading) {
            return (<div className="d-flex justify-content-center" style={{ height: '247px' }}>
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>);
        }
        
        return (<div>
            {this.renderScore()}
            <div className="d-flex flex-wrap justify-content-center score-goals-container">
                {this.state.game.squad.map(player => this.renderHolcombeScoreButton(player))}
                {this.renderOpponentScoreButton()}
            </div>
        </div>);
    }
    
    renderHolcombeScoreButton(player) {
        const isLatestScorer = this.state.latestScorer === player.number;
        return (<button key={player.number} type="button" className={`btn ${isLatestScorer ? ' btn-success' : 'btn-primary'} btn-goal-scorer`} onClick={this.holcombeGoal} data-player-number={player.number}>
            {player.name} Scored!
        </button>);
    }
    
    renderOpponentScoreButton() {
        const isLatestScorer = this.state.latestScorer === 'opponent';
        return (<button key="opponent" type="button" className={`btn ${isLatestScorer ? ' btn-success' : 'btn-secondary'} btn-goal-scorer`} onClick={this.opponentGoal}>{this.state.game.opponent} Scored</button>);
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

    async getGameData() {
        const game = await this.gameApi.getGame(this.props.gameId);
        const team = await this.teamApi.getTeam(this.props.teamId);
        this.setState({
            game: game,
            team: team,
            loading: false,
        });
    }
}