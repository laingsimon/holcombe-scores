import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {Functions} from '../functions';

/*
* Props:
* - [player]
* - game
* - [readOnly]
*
*
* Events:
* - onGoalScored(holcombeGoal, playerId)
* */
// noinspection JSUnresolvedVariable
export class RecordGoal extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.state = {
            latestScorer: false
        };
        this.readOnly = props.readOnly;

        this.recordGoal = this.recordGoal.bind(this);
    }

    // events
    async goalScored() {
        if (this.props.onGoalScored) {
            await this.props.onGoalScored(!!this.props.player, this.props.player ? this.props.player.id : null);
        }
    }

    // event handlers
    async recordGoal() {
        if (this.readOnly) {
            return;
        }

        this.setState({
            latestScorer: true
        });

        window.setTimeout(() => {
            this.setState({
                latestScorer: false
            });
        }, 1500);

        await this.goalScored();
        const result = await this.gameApi.recordGoal(this.props.game.id, new Date().toISOString(), !!this.props.player, this.props.player ? this.props.player.id : null);
        if (!result.success) {
            alert(`Could not record goal: ${Functions.getResultMessages(result)}`);
        }
    }

    // renderers
    render() {
        return this.props.player
            ? this.renderHolcombeScoreButton()
            : this.renderOpponentScoreButton();
    }

    renderHolcombeScoreButton() {
        const hasScored = this.props.game.goals.filter(g => g.holcombeGoal && g.player.id === this.props.player.id).length > 0;
        const isLatestScorer = this.state.latestScorer || (this.readOnly && hasScored);
        const colour = this.readOnly ? 'btn-light' : 'btn-primary';
        const suffix = !this.readOnly || hasScored ? 'scored!' : 'played';

        return (<button type="button" className={`btn ${isLatestScorer ? ' btn-outline-warning' : colour} btn-goal-scorer`} onClick={this.recordGoal}>
            {this.props.player.name} {suffix}
        </button>);
    }

    renderOpponentScoreButton() {
        return (<button key="opponent" type="button" className={`btn ${this.state.latestScorer ? ' btn-outline-warning' : 'btn-secondary'} btn-goal-scorer`} onClick={this.recordGoal}>{this.props.game.opponent} Scored</button>);
    }
}