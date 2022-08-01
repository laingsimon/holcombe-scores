import React, { Component } from 'react';
import {Score} from "./Score";

/*
* Props:
* - game
* - team
* - showTeam        // TODO: Is this ever set/used?
*
* Events:
* -none-
* */
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
        const game = this.props.game;
        const location = game.playingAtHome ? 'Home' : 'Away';
        const date = new Date(Date.parse(game.date));
        const holcombe = game.goals.filter(g => g.holcombeGoal).length;
        const opponent = game.goals.filter(g => !g.holcombeGoal).length;
        const score = {
            holcombe: holcombe,
            opponent: opponent
        };

        return (<a href={`/game/${game.id}`} className="list-group-item d-flex justify-content-between align-items-center">
            {this.renderTeam()} {this.props.showTeam ? location : location.toLowerCase()} to {game.opponent} on {date.toDateString()}
            <Score playingAtHome={game.playingAtHome} score={score} />
        </a>);
    }

    renderTeam() {
        if (!this.props.showTeam) {
            return null;
        }
        return (<strong>{this.props.team.name}</strong>);
    }
}
