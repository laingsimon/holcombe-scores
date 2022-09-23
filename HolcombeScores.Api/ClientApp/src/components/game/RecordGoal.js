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
            captureAssist: false,
            assistedByPlayerId: null
        };

        this.recordGoal = this.recordGoal.bind(this);
        this.recordTheGoal = this.recordTheGoal.bind(this);
        this.cancelCaptureAssist = this.cancelCaptureAssist.bind(this);
        this.recordAssistedGoal = this.recordAssistedGoal.bind(this);
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
    async recordAssistedGoal(event) {
        const target = event.target;
        const playerId = target.getAttribute('data-player-id');

        this.setState({
            assistedByPlayerId: playerId,
            captureAssist: false
        });

        await this.recordTheGoal();
    }

    cancelCaptureAssist() {
        this.setState({
            captureAssist: false
        });
    }

    async recordGoal() {
        if (this.props.game.readOnly) {
            return;
        }

        if (this.props.player) {
            this.setState({
                captureAssist: true,
                assistedByPlayerId: null
            });
            return;
        }

        await this.recordTheGoal();
    }

    async recordTheGoal() {
        this.setState({
            goalJustScored: true,
            recordingGoal: true,
            captureAssist: false
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

        return (<div>
            <div className={`btn ${isLatestScorer ? ' btn-outline-warning' : colour} btn-large-player-button`} onClick={this.recordGoal}>
                {this.props.player.name} {suffix}
            </div>
            {this.state.captureAssist ? this.renderCaptureAssist() : null}
        </div>);
    }

    renderCaptureAssist() {
        return (<div>
            <div className="modal fade show" role="dialog" style={{display: 'block'}}>
                <div className="modal-dialog modal-dialog-centered">
                    <div className="modal-content">
                        <div className="modal-header justify-content-center">
                            <h5>{this.props.player.name}'s goal, was there an assist?</h5>
                        </div>
                        <div className="modal-body">
                            {this.props.game.squad.filter(p => p.id !== this.props.player.id).map(player => {
                                return (<button type="button" className="btn btn-primary margin-right" data-player-id={player.id} onClick={this.recordAssistedGoal}>{player.name}</button>);
                            })}

                            <button type="button" className="btn btn-secondary" onClick={this.recordTheGoal}>No assist</button>
                        </div>
                        <div className="modal-footer">
                            <button className="btn btn-primary" onClick={this.cancelCaptureAssist}>Close</button>
                        </div>
                    </div>
                </div>
            </div>
            <div className="modal-backdrop fade show"></div>
        </div>);
    }

    renderOpponentScoreButton() {
        return (<div key="opponent" className={`btn ${this.state.latestScorer ? ' btn-outline-warning' : 'btn-secondary'} btn-large-player-button`} onClick={this.recordGoal}>{this.props.game.opponent} Scored</div>);
    }
}
