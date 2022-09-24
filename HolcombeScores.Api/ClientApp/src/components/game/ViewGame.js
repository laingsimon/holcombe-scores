import React, { Component } from 'react';
import {Functions} from '../../functions'
import {GoalOverview} from './GoalOverview';

/*
* Props:
* - game
*
* Events:
* - onGoalRemoved(goalId, gameId)
* */
// noinspection JSUnresolvedVariable
export class ViewGame extends Component {
    constructor(props) {
        super(props);
        this.goalRemoved = this.goalRemoved.bind(this);
    }

    // event handlers
    async goalRemoved(goalId, gameId) {
        if (this.props.onGoalRemoved) {
            await this.props.onGoalRemoved(goalId, gameId);
        }
    }

    // renderers
    render() {
        const game = this.props.game;
        const date = new Date(Date.parse(game.date));
        const runningScore = {
            holcombe: 0,
            opponent: 0,
        }

        game.squad.sort(Functions.playerSortFunction);

        return (<div>
            <h6>Start time: {date.toLocaleTimeString()}</h6>
            {!game.address ? null : this.renderAddress(game.address, game.googleMapsApiKey)}
            {game.training ? null : (<div>
                <h5>Goals</h5>
                {this.renderGoals(game, runningScore)}
            </div>)}
            <div>
                <h5>Holcombe Players</h5>
                <ul>
                    {game.squad.map(p => this.renderPlayer(p))}
                </ul>
            </div>
        </div>);
    }

    renderAddress(address, apiKey) {
        return (<div>
            <h5>Address</h5>
            <p>{address}</p>
            <iframe
                title="Game address - HolcombeScores"
                width="100%"
                height="250"
                referrerPolicy="no-referrer-when-downgrade"
                src={`https://www.google.com/maps/embed/v1/place?key=${apiKey}&q=${address}`}
                allowFullScreen>
            </iframe>
            <br />
        </div>)
    }

    renderGoals(game, runningScore) {
        if (game.goals.length === 0) {
            return (<p>No goals</p>);
        }

        game.goals.map(goal => {
            goal.jsTime = new Date(goal.time);
            return goal;
        });
        game.goals.sort((a, b) => a.jsTime - b.jsTime);

        return (<ol>
            {game.goals.map(g => this.renderGoal(g, game, runningScore))}
        </ol>);
    }

    renderGoal(goal, game, runningScore) {
        if (goal.holcombeGoal) {
            runningScore.holcombe++;
        } else {
            runningScore.opponent++;
        }

        return (<GoalOverview key={`${goal.goalId}_${runningScore.holcombe + runningScore.opponent}`} goal={goal} game={game} score={Object.assign({}, runningScore)} onGoalDeleted={this.goalRemoved} />);
    }

    renderPlayer(player) {
        return (<li key={player.id}>{player.number ? (<span className="badge rounded-pill bg-primary">{player.number}</span>) : null} {player.name}</li>);
    }
}
