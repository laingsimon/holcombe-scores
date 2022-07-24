import React, { Component } from 'react';

export class GameOverview extends Component {
    constructor(props) {
        super(props);
        this.game = props.game;
        this.team = props.team;
        this.history = props.history;
        this.showTeam = props.showTeam;
    }

    // renderers
    render() {
        const location = this.game.playingAtHome ? 'Home' : 'Away';
        const date = new Date(Date.parse(this.game.date));
        const holcombeGoals = this.game.goals.filter(g => g.holcombeGoal).length;
        const opponentGoals = this.game.goals.filter(g => !g.holcombeGoal).length;
        const score = this.game.playingAtHome
            ? `${holcombeGoals}-${opponentGoals}`
                : `${opponentGoals}-${holcombeGoals}`;

        return (<a href={`/game/${this.game.id}`} className="list-group-item d-flex justify-content-between align-items-center">
            {this.renderTeam()} {this.showTeam ? location : location.toLowerCase()} to <strong>{this.game.opponent}</strong> on {date.toDateString()}
          <span className="badge rounded-pill bg-primary">{score}</span>
        </a>);
    }
    
    renderTeam() {
        if (!this.showTeam) {
            return null;
        }
        return (<strong>{this.team.name}</strong>);
    }
}
