import React, {Component} from 'react';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Game} from '../api/game';
import {Access} from '../api/access';
import {Team} from '../api/team';
import {Alert} from './Alert';
import {EditGame} from "./EditGame";
import {PlayGame} from "./PlayGame";
import {Score} from "./Score";
import {ViewGame} from "./ViewGame";

// noinspection JSUnresolvedVariable
/*
* Props:
* - game
* - team
* - access
* - reloadGame(gameId)
*
* Events:
* -none-
*/
export class GameDetails extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.accessApi = new Access(http);
        this.teamApi = new Team(http);
        this.history = props.history;
        this.gameId = props.match.params.gameId;
        this.state = {
            loading: true,
            error: null,
            mode: props.match.params.mode || 'view'
        };
        this.changeMode = this.changeMode.bind(this);
        this.gameChanged = this.gameChanged.bind(this);
        // this.onGoalChanged = this.onGoalChanged.bind(this);
        // this.onGoalRemoved = this.onGoalRemoved.bind(this);
    }

    //event handlers
    async gameChanged(gameId, holcombeGoal, playerId) {
        if (!gameId) {
            // refresh
            await this.props.reloadGame(gameId); // don't set the state to loading
            return;
        }

        const game = Object.assign({}, this.props.game);
        game.goals.push({
            time: null,
            holcombeGoal: holcombeGoal,
            gameId: gameId,
            player: holcombeGoal
                ? {
                    id: playerId,
                    teamId: game.teamId
                }
                : null
        });

        this.setState({
            game: game
        });

        await this.props.reloadGame(this.gameId); // don't set the state to loading
    }

    changeMode(event) {
        event.preventDefault();
        const url = event.target.getAttribute('href');
        const segments = url.split('/')
        const mode = segments[segments.length - 1];
        this.setState({
            mode: mode,
        });
        window.history.replaceState(null, event.target.textContent, url);
    }

    async componentDidMount() {
        if (!this.props.game && this.gameId) {
            await this.props.reloadGame(this.gameId);

            this.setState({
                loading: false
            });
        }
    }

    // renderers
    renderNav() {
        const editNav = <li className="nav-item">
            <a className={`nav-link${this.state.mode === 'edit' ? ' active' : ''}`} href={`/game/${this.gameId}/edit`}
               onClick={this.changeMode}>Edit Game</a>
        </li>;

        return (<ul className="nav nav-tabs">
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'view' ? ' active' : ''}`}
                   href={`/game/${this.gameId}/view`} onClick={this.changeMode}>View Game</a>
            </li>
            {this.props.access.admin || this.props.access.manager ? editNav : null}
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'play' ? ' active' : ''}`}
                   href={`/game/${this.gameId}/play`} onClick={this.changeMode}>Play Game</a>
            </li>
        </ul>);
    }

    render() {
        if (this.state.loading) {
            return (<div className="d-flex justify-content-center">
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>);
        }
        if (this.state.error) {
            return (<div>
                <Alert errors={[this.state.error]}/>
                <a className="btn btn-primary" href="/">Home</a>
            </div>);
        }
        if (!this.props.access) {
            return (<div>
                <h4>Not logged in</h4>
                <a href="/" className="btn btn-primary">Home</a>
            </div>)
        }

        if (!this.props.game) {
            return (<Alert errors={[ 'Game not found' ]}/>);
        }

        let component = (<Alert warnings={[`Unknown mode ${this.state.mode}`]}/>);

        if (this.state.mode === 'view') {
            component = (
                <ViewGame game={this.props.game} readOnly={this.isReadOnly()} onGoalRemoved={this.onGoalRemoved}/>);
        } else if (this.state.mode === 'edit') {
            component = (<EditGame team={this.props.team} game={this.props.game} onChanged={this.gameChanged}/>);
        } else if (this.state.mode === 'play') {
            component = (<PlayGame team={this.props.team} game={this.props.game} readOnly={this.isReadOnly()}
                                   onChanged={this.gameChanged} />);
        }

        return (<div>
            {this.renderHeading()}
            {this.renderNav()}
            <hr/>
            {component}
        </div>)
    }

    renderHeading() {
        const location = this.props.game.playingAtHome ? 'Home' : 'Away';
        const date = new Date(Date.parse(this.props.game.date));
        const score = {
            holcombe: this.props.game.goals.filter(g => g.holcombeGoal).length,
            opponent: this.props.game.goals.filter(g => !g.holcombeGoal).length
        };

        return (<h4>
            {this.props.team.name}: {location} to {this.props.game.opponent} on {date.toDateString()} <Score
            playingAtHome={this.props.game.playingAtHome} score={score}/>
        </h4>);
    }

    // api access
    isReadOnly() {
        const date = new Date(this.props.game.date);
        const timeDiff = this.props.game.asAt.getTime() - date.getTime();
        const hourDiff = Math.floor(timeDiff / 1000 / 60 / 60);
        const dayDiff = Math.floor(hourDiff / 24);
        return dayDiff > 2;
    }
}
