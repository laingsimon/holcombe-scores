import React, { Component } from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Game} from '../../api/game';
import {RecordGoal} from './RecordGoal';
import {Score} from './Score';
import {Functions} from "../../functions";

/*
* Props:
* - game
*
* Events:
* - onGoalScored(gameId, holcombeGoal, playerId)
* - onGoalNotRecorded(gameId, holcombeGoal, playerId)
* */
// noinspection JSUnresolvedVariable
export class PlayGame extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.state = {
            refreshHandle: null,
        };
        this.onGoalScored = this.onGoalScored.bind(this);
        this.onGoalNotRecorded = this.onGoalNotRecorded.bind(this);
        this.clearErrorRecordingGoal = this.clearErrorRecordingGoal.bind(this);
    }

    // events
    async onGoalScored(holcombeGoal, playerId) {
        if (this.props.onGoalScored) {
            await this.props.onGoalScored(this.props.game.id, holcombeGoal, playerId);
        }
    }

    async onGoalNotRecorded(holcombeGoal, playerId, resultOrException) {
        if (this.props.onGoalNotRecorded) {
            await this.props.onGoalNotRecorded(this.props.game.id, holcombeGoal, playerId);
        }

        this.setState({
            errorRecordingGoal: {
                holcombeGoal,
                playerId,
                resultOrException
            }
        });
    }

    // event handler
    clearErrorRecordingGoal() {
        this.setState({
            errorRecordingGoal: null
        });
    }

    render() {
        const score = {
            holcombe: this.props.game.goals.filter(g => g.holcombeGoal).length,
            opponent: this.props.game.goals.filter(g => !g.holcombeGoal).length
        };

        return (<div>
            <h1 className="text-center"><Score playingAtHome={this.props.game.playingAtHome} score={score} /></h1>
            {this.state.errorRecordingGoal ? this.renderErrorRecordingGoal(this.state.errorRecordingGoal) : null}
            <div className="d-flex flex-wrap justify-content-center score-goals-container">
                {this.props.game.squad.map(player => (<RecordGoal key={player.id} player={player} game={this.props.game} reloadGame={this.props.reloadGame} onGoalScored={this.onGoalScored} onGoalNotRecorded={this.onGoalNotRecorded} />))}
                {this.props.game.readOnly ? null : (<RecordGoal key={'opponent'} game={this.props.game} reloadGame={this.props.reloadGame} onGoalScored={this.onGoalScored} onGoalNotRecorded={this.onGoalNotRecorded} />)}
            </div>
        </div>);
    }

    renderErrorRecordingGoal(details) {
        return (<div>
            <div className="modal fade show" role="dialog" style={{display: 'block'}}>
                <div className="modal-dialog modal-dialog-centered">
                    <div className="modal-content">
                        <div className="modal-header justify-content-center">
                            <h5>🥅 Sorry, the goal couldn't be recorded</h5>
                        </div>
                        <div className="modal-body">
                            <p>
                                {details.resultOrException.success !== undefined
                                    ? Functions.getResultMessages(details.resultOrException)
                                    : details.resultOrException.message}
                            </p>
                        </div>
                        <div className="modal-footer">
                            <button className="btn btn-primary" onClick={this.clearErrorRecordingGoal}>Close</button>
                        </div>
                    </div>
                </div>
            </div>
            <div className="modal-backdrop fade show"></div>
        </div>)
    }
}
