import React, { Component } from 'react';
import {Score} from "./Score";
import { Link } from "react-router-dom";

/*
* Props:
* - game
* - team
*
* Events:
* -none-
* */
export class GameOverview extends Component {
    // renderers
    render() {
        const game = this.props.game;
        const location = game.playingAtHome ? 'home' : 'away';
        const date = new Date(Date.parse(game.date));
        const holcombe = game.goals.filter(g => g.holcombeGoal).length;
        const opponent = game.goals.filter(g => !g.holcombeGoal).length;
        const score = {
            holcombe: holcombe,
            opponent: opponent
        };

        return (<Link to={`/game/${game.id}`} className="list-group-item d-flex justify-content-between align-items-center">
            {location} to {game.opponent} on {date.toDateString()}
            <Score playingAtHome={game.playingAtHome} score={score} />
        </Link>);
    }
}
