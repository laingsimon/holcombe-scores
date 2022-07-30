import React, { Component } from 'react';
import {Functions} from '../functions'
import {GoalOverview} from "./GoalOverview";

export class ViewGame extends Component {
    constructor(props) {
        super(props);
    }

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
            <div>
                <h5>Goals</h5>
                {this.renderGoals(game, runningScore)}
            </div>
            <div>
                <h5>Holcombe Players</h5>
                <ul>
                    {game.squad.map(p => this.renderPlayer(p))}
                </ul>
            </div>
        </div>);
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

        return (<GoalOverview key={goal.goalId} goal={goal} game={game} readOnly={this.props.readOnly} score={Object.assign({}, runningScore)} onGoalChanged={this.onGoalChanged} />);
    }

    renderPlayer(player) {
        return (<li key={player.id}>{player.number ? (<span className="badge rounded-pill bg-primary">{player.number}</span>) : null} {player.name}</li>);
    }
}