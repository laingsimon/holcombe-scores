import React, { Component } from 'react';
import {Score} from './Score';
import { Link } from 'react-router-dom';

/*
* Props:
* - game
* - team
* - reloadGame(id)
*
* Events:
* -none-
* */
export class GameOverview extends Component {
    constructor(props) {
        super(props);
        this.state = {
            navigating: false
        }
        this.beforeNavigate = this.beforeNavigate.bind(this);
    }

    async beforeNavigate(event) {
        event.preventDefault();
        this.setState({
            navigating: true
        });

        await this.props.reloadGame(this.props.game.id);

        this.setState({
            navigating: false
        });

        this.props.history.push(event.target.getAttribute('href'));
    }

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
        const content = game.training
            ? `training at ${game.playingAtHome ? 'home' : game.opponent} on ${date.toDateString()}`
            : `${location} to ${game.opponent} on ${date.toDateString()}`;

        return (<Link to={`/game/${game.id}`} onClick={this.beforeNavigate} className="list-group-item d-flex justify-content-between align-items-center">
            {content}{game.postponed ? ' (postponed)' : null}
            {this.state.navigating ? (<span className="float-end spinner-border spinner-border-sm text-success" role="status" aria-hidden="true"></span>) : null}
            {game.training || game.postponed || !game.hasStarted ? null : (<Score playingAtHome={game.playingAtHome} score={score} />)}
        </Link>);
    }
}
