import React, { Component } from 'react';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Game} from '../api/game';

export class GameComponent extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.history = props.history;
        this.gameId = props.match.params.gameId;
        this.state = {
            loading: true,
            game: null,
            team: null,
            error: null
        }
    }
    
    componentDidMount() {
        // noinspection JSIgnoredPromiseFromCall
        this.fetchGame();
    }

    // renderers
    render() {
        if (this.state.loading) {
            return (<div>Loading...</div>);
        }
        if (this.state.error) {
            return <div><h4>Error</h4>{this.state.error}</div>
        }
        
        const game = this.state.game;
        const location = game.playingAtHome ? 'home' : 'away';
        const date = new Date(Date.parse(game.date));
        const holcombeGoals = game.goals.filter(g => g.holcombeGoal).length;
        const opponentGoals = game.goals.filter(g => !g.holcombeGoal).length;
        const score = game.playingAtHome
            ? `${holcombeGoals}-${opponentGoals}`
                : `${opponentGoals}-${holcombeGoals}`;

        return (<div className="list-group-item d-flex justify-content-between align-items-center">
            {location} to {game.opponent} on {date.toDateString()}
            <span className="badge rounded-pill bg-primary">{score}</span>
            <hr />
            <div>
                <p>Goals</p>
            </div>
        </div>);
    }

    // api access
    async fetchGame() {
        try {
            const game = await this.gameApi.getGame(this.gameId);
            this.setState({game: game, loading: false});
        } catch (e) {
            console.log(e);
            this.setState({loading: false, error: e.message });
        }
    }
}
