import React, { Component } from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Game} from '../../api/game';
import {RecordGoal} from './RecordGoal';
import {Score} from './Score';

/*
* Props:
* - game
*
* Events:
* - onGoalScored(gameId, holcombeGoal, playerId)
* */
// noinspection JSUnresolvedVariable
export class PlayGame extends Component {
    static REFRESH_INTERVAL = 5000;

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
        if (this.props.onGoalScored) {
            await this.props.onGoalScored(this.props.game.id, holcombeGoal, playerId);
        }
    }

    // event handler
    async refresh() {
        const now = new Date().getTime();
        const asAt = this.props.game.asAt.getTime();
        const diff = now - asAt;

        if (diff > (PlayGame.REFRESH_INTERVAL)) { // half the refresh interval
            await this.props.reloadGame(this.props.game.id);
        }
    }

    stopRefresh() {
        window.clearInterval(this.state.refreshHandle);
        this.setState({
            refreshHandle: null
        })
    }

    componentDidMount() {
        if (this.props.game.gamePlayable) {
            this.setState({
                refreshHandle: window.setInterval(this.refresh, PlayGame.REFRESH_INTERVAL)
            });
        }
    }

    componentWillUnmount() {
        this.stopRefresh();
    }

    render() {
        const score = {
            holcombe: this.props.game.goals.filter(g => g.holcombeGoal).length,
            opponent: this.props.game.goals.filter(g => !g.holcombeGoal).length
        };

        const notRefreshStatus = this.props.game.readOnly
            ? 'Game over'
            : 'Not refreshing';

        return (<div>
            <h1 className="text-center"><Score playingAtHome={this.props.game.playingAtHome} score={score} /></h1>
            <div className="d-flex flex-wrap justify-content-center score-goals-container">
                {this.props.game.squad.map(player => (<RecordGoal key={player.id} player={player} game={this.props.game} onGoalScored={this.onGoalScored} />))}
                {this.props.game.readOnly ? null : (<RecordGoal key={'opponent'} game={this.props.game} onGoalScored={this.onGoalScored} />)}
            </div>
            <hr />
            <div className="text-center">
                Score as at {this.props.game.asAt.toLocaleTimeString()}
                &nbsp;-&nbsp;
                {this.state.refreshHandle ? (<button className="btn btn-secondary" onClick={this.stopRefresh}>Stop Refresh</button>) : <span>{notRefreshStatus}</span>}
            </div>
        </div>);
    }
}
