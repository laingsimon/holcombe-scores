import React, { Component } from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Game} from '../../api/game';

/*
* Props:
* - [player]
* - game
* - reloadGame()
*
* Events:
* - onGoalScored(holcombeGoal, [playerId])
* - onGoalNotRecorded(holcombeGoal, [playerId], result)
* */
// noinspection JSUnresolvedVariable
export class RecordGoal extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.state = {
            latestScorer: false,
            assistedByPlayerId: null
        };

        this.recordGoal = this.recordGoal.bind(this);
    }

    // events
    async goalScored() {
        if (this.props.onGoalScored) {
            await this.props.onGoalScored(!!this.props.player, this.props.player ? this.props.player.id : null);
        }
    }

    async goalNotRecorded(result) {
        if (this.props.onGoalNotRecorded) {
            await this.props.onGoalNotRecorded(!!this.props.player, this.props.player ? this.props.player.id : null, result);
        }
    }

    // event handlers
    async recordGoal() {
        if (this.props.game.readOnly) {
            return;
        }

        this.setState({
            goalJustScored: true,
            recordingGoal: true
        });

        window.setTimeout(() => {
            this.setState({
                goalJustScored: false
            });
        }, 1500);

        await this.goalScored();
        try {
            const result = await this.gameApi.recordGoal(
                this.props.game.id,
                new Date().toISOString(),
                !!this.props.player,
                this.props.player
                    ? this.props.player.id
                    : null,
                this.props.game.recordGoalToken,
                this.state.assistedByPlayerId);

            if (result.success) {
                await this.props.reloadGame(this.props.game.id);
            } else {
                await this.goalNotRecorded(result);
            }
        } catch (exc) {
            await this.goalNotRecorded(exc);
        } finally {
            this.setState({
                recordingGoal: false
            });
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
        const shouldHighlight = this.state.goalJustScored || this.state.recordingGoal;
        const isLatestScorer = shouldHighlight || (this.props.game.readOnly && hasScored);
        const colour = this.props.game.readOnly ? 'btn-light' : 'btn-primary';
        const suffix = !this.props.game.readOnly || hasScored ? 'scored!' : 'played';

        return (<button type="button" className={`btn ${isLatestScorer ? ' btn-outline-warning' : colour} btn-large-player-button`} onClick={this.recordGoal}>
            {this.props.player.name} {suffix}
        </button>);
    }

    renderOpponentScoreButton() {
        return (<button key="opponent" type="button" className={`btn ${this.state.latestScorer ? ' btn-outline-warning' : 'btn-secondary'} btn-large-player-button`} onClick={this.recordGoal}>{this.props.game.opponent} Scored</button>);
    }
}
