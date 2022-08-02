import React, { Component } from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Game} from '../../api/game';
import {Functions} from '../../functions';
import {Score} from './Score';

// noinspection JSUnresolvedVariable
/*
* Props:
* - goal
* - game
* - score
*
* Events:
* - onGoalDeleted(goalId, gameId)
*/
export class GoalOverview extends Component {
    constructor (props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.state = {
            deleting: false
        }

        this.removeGoal = this.removeGoal.bind(this);
    }

    //event handlers
    async removeGoal() {
        const detail = this.props.goal.holcombeGoal
            ? this.props.goal.player.name
            : this.props.game.opponent;

        if (!window.confirm(`Are you sure you want to remove ${detail}'s goal?`)) {
            return;
        }

        this.setState({
            deleting: true
        });

        const result = await this.gameApi.removeGoal(this.props.game.id, this.props.goal.goalId);

        if (result.success) {
            if (this.props.onGoalDeleted) {
                await this.props.onGoalDeleted(this.props.goal.goalId, this.props.game.id);
            }
        } else {
            alert(`Could not delete goal: ${Functions.getResultMessages(result)}`);
            this.setState({
                deleting: false
            });
        }
    }

    // renderers
    render() {
        const time = new Date(Date.parse(this.props.goal.time)).toTimeString().substring(0, 5);
        const name = this.props.goal.holcombeGoal ? this.props.goal.player.name : this.props.game.opponent;
        const deleteContent = this.state.deleting
            ? (<span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>)
            : '🗑';
        const timeAndName = `${time} - ${name}`;

        return (
            <li>
                <Score playingAtHome={this.props.game.playingAtHome} score={this.props.score} /> {this.state.deleting ? (<s>{timeAndName}</s>) : timeAndName}
                {this.props.game.readOnly ? null : (<button className="delete-goal" onClick={this.removeGoal}>{deleteContent}</button>)}
            </li>);

    }
}