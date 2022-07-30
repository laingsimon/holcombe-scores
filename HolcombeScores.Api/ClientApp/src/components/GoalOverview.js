import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {Functions} from "../functions";
import {Score} from "./Score";

// noinspection JSUnresolvedVariable
export class GoalOverview extends Component {
    constructor (props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.state = {
            deleting: false
        }

        this.goal = props.goal;
        this.game = props.game;
        this.runningScore = props.score;
        this.removeGoal = this.removeGoal.bind(this);
    }

    // events
    goalChanged() {
        if (this.props.onGoalChanged) {
            this.props.onGoalChanged(this.goal.id, this.game.id);
        }
    }

    //event handlers
    async removeGoal() {
        const detail = this.goal.holcombeGoal
            ? this.goal.player.name
            : this.game.opponent;

        if (!window.confirm(`Are you sure you want to remove ${detail}'s goal?`)) {
            return;
        }

        this.setState({
            deleting: true
        });

        const result = await this.gameApi.removeGoal(this.game.id, this.goal.goalId);

        if (result.success) {
            this.goalChanged();
        } else {
            alert(`Could not delete goal: ${Functions.getResultMessages(result)}`);
            this.setState({
                deleting: false
            });
        }
    }

    // renderers
    render() {
        const time = new Date(Date.parse(this.goal.time)).toTimeString().substring(0, 5);
        const name = this.goal.holcombeGoal ? this.goal.player.name : this.game.opponent;
        const deleteContent = this.state.deleting
            ? (<span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>)
            : '🗑';
        const timeAndName = `${time} - ${name}`;

        return (
            <li>
                <Score playingAtHome={this.game.playingAtHome} score={this.runningScore} /> {this.state.deleting ? (<s>{timeAndName}</s>) : timeAndName}
                <button className="delete-goal" onClick={this.removeGoal}>{deleteContent}</button>
            </li>);

    }
}