import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';

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
        this.runningScore = props.runningScore;
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
            let messages = [];
            result.messages.forEach(m => messages.push(m));
            result.warnings.forEach(m => messages.push('Warning: ' + m));
            result.errors.forEach(m => messages.push('Error: ' + m));

            alert(`Could not delete goal: ${messages.join('\n')}`);
        }
    }

    // renderers
    render() {
        const time = new Date(Date.parse(this.goal.time)).toTimeString().substring(0, 5);
        const colour = this.state.deleting
            ? 'bg-secondary'
            : this.goal.holcombeGoal ? 'bg-success' : 'bg-danger';
        const name = this.goal.holcombeGoal ? this.goal.player.name : this.game.opponent;
        const deleteContent = this.state.deleting
            ? (<span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>)
            : '🗑';
        const timeAndName = `${time} - ${name}`;

        return (
            <li key={this.goal.goalId}>{this.renderRunningScore(this.runningScore, this.game.playingAtHome, colour)} {this.state.deleting ? (<s>{timeAndName}</s>) : timeAndName}
                <button className="delete-goal" onClick={this.removeGoal}>{deleteContent}</button>
            </li>);

    }

    renderRunningScore(runningScore, playingAtHome, colour) {
        const score = this.game.playingAtHome
            ? `${runningScore.holcombe} - ${runningScore.opponent}`
            : `${runningScore.opponent} - ${runningScore.holcombe}`;

        return (<span className={`badge rounded-pill ${colour}`}>{score}</span>);
    }
}