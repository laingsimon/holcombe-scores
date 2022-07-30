import React, { Component } from 'react';
import {Score} from "./Score";

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
        const holcombe = this.game.goals.filter(g => g.holcombeGoal).length;
        const opponent = this.game.goals.filter(g => !g.holcombeGoal).length;
        const score = {
            holcombe: holcombe,
            opponent: opponent
        };

        return (<a href={`/game/${this.game.id}`} className="list-group-item d-flex justify-content-between align-items-center">
            {this.renderTeam()} {this.showTeam ? location : location.toLowerCase()} to {this.game.opponent} on {date.toDateString()}
            <Score playingAtHome={this.game.playingAtHome} score={score} />
        </a>);
    }

    renderTeam() {
        if (!this.showTeam) {
            return null;
        }
        return (<strong>{this.team.name}</strong>);
    }
}
