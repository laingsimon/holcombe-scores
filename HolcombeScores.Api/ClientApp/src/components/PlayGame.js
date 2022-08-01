import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {RecordGoal} from './RecordGoal';
import {Score} from "./Score";

/*
* Props:
* - game
* - [readOnly]
*
* Events:
* - onGoalScored(gameId, holcombeGoal, playerId)
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
        this.stopRefresh = this.stopRefresh.bind(this);
        this.onGoalScored = this.onGoalScored.bind(this);
        this.refresh = this.refresh.bind(this);
    }

    // events
    async onGoalScored(holcombeGoal, playerId) {
        if (this.props.onChanged) {
            await this.props.onGoalScored(this.props.game.id, holcombeGoal, playerId);
        }
    }

    // event handler
    async refresh() {
        await this.onGoalScored(); // pretend a goal was scored, causing the outer component to refresh
    }

    stopRefresh() {
        window.clearInterval(this.state.refreshHandle);
        this.setState({
            refreshHandle: null
        })
    }

    componentDidMount() {
        if (!this.props.readOnly) {
            this.setState({
                refreshHandle: window.setInterval(this.refresh, 5000)
            });
        }
    }

    componentWillUnmount() {
        this.stopRefresh();
    }

    render() {
        const notRefreshStatus = this.props.readOnly
            ? 'Game over'
            : 'Not refreshing';

        return (<div>
            {this.renderScore()}
            <div className="d-flex flex-wrap justify-content-center score-goals-container">
                {this.props.game.squad.map(player => (<RecordGoal key={player.id} player={player} game={this.props.game} readOnly={this.props.readOnly} onGoalScored={this.onGoalScored} />))}
                {this.props.readOnly ? null : (<RecordGoal key={'opponent'} game={this.props.game} readOnly={this.props.readOnly} onGoalScored={this.onGoalScored} />)}
            </div>
            <hr />
            <div className="text-center">
                Score as at {this.props.game.asAt.toLocaleTimeString()}
                &nbsp;-&nbsp;
                {this.state.refreshHandle ? (<button className="btn btn-secondary" onClick={this.stopRefresh}>Stop Refresh</button>) : <span>{notRefreshStatus}</span>}
            </div>
        </div>);
    }

    renderScore() {
        const score = {
            holcombe: this.props.game.goals.filter(g => g.holcombeGoal).length,
            opponent: this.props.game.goals.filter(g => !g.holcombeGoal).length
        };

        return (<h4 className="text-center"><Score playingAtHome={this.props.game.playingAtHome} score={score} /></h4>);
    }
}
