import React, {Component} from 'react';
import {Http} from '../../api/http';
import {Settings} from '../../api/settings';
import {Game} from '../../api/game';
import {Access} from '../../api/access';
import {Team} from '../../api/team';
import {Alert} from '../Alert';
import {EditGame} from './EditGame';
import {PlayGame} from './PlayGame';
import {Score} from './Score';
import {ViewGame} from './ViewGame';
import { Link } from 'react-router-dom';
import {EditAvailability} from "./EditAvailability";

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
            gameDeleted: false,
            error: null,
            mode: props.match.params.mode || 'view'
        };
        this.changeMode = this.changeMode.bind(this);
        this.goalScored = this.goalScored.bind(this);
        this.goalRemoved = this.goalRemoved.bind(this);
        this.gameChanged = this.gameChanged.bind(this);
        this.gameDeleted = this.gameDeleted.bind(this);
        this.availabilityChanged = this.availabilityChanged.bind(this);
    }

    //event handlers
    async availabilityChanged() {
        await this.props.reloadAvailability(this.props.game.teamId, this.gameId);
    }

    async goalRemoved(goalId, gameId) {
        await this.props.reloadGame(gameId); // don't set the state to loading
    }

    async gameDeleted(gameId, teamId) {
        const reloadTeam = false;
        const reloadPlayers = false;
        const reloadGames = true;
        await this.props.reloadTeam(teamId, reloadTeam, reloadPlayers, reloadGames);

        this.setState({
            gameDeleted: true
        });
    }

    async gameChanged(gameId, teamId) {
        await this.props.reloadGame(gameId); // don't set the state to loading
    }

    async goalScored(gameId, holcombeGoal, playerId) {
        if (!gameId) {
            // refresh
            await this.props.reloadGame(gameId); // don't set the state to loading
            return;
        }

        this.props.game.goals.push({
            time: new Date().toISOString(),
            holcombeGoal: holcombeGoal,
            gameId: gameId,
            player: holcombeGoal
                ? this.props.game.squad.filter(p => p.id === playerId)[0]
                : null,
            local: true
        });

        await this.props.updateGame(this.props.game); // will also trigger a reload
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
        if (this.props.game) {
            this.setState({
                loading: false
            });
            return;
        }

        if (this.gameId) {
            await this.props.reloadGame(this.gameId);

            this.setState({
                loading: false
            });
        }
    }

    // renderers
    renderNav() {
        const editNav = <li className="nav-item">
            <Link className={`nav-link${this.state.mode === 'edit' ? ' active' : ''}`} to={`/game/${this.gameId}/edit`}
               onClick={this.changeMode}>
                {this.state.gameDeleted ? 'Game deleted' : 'Edit'}
            </Link>
        </li>;

        return (<ul className="nav nav-tabs">
            <li className="nav-item">
                <Link className="nav-link" to={`/team/${this.props.team.id}`}>⬅️</Link>
            </li>
            {this.state.gameDeleted ? null : (<li className="nav-item">
                <a className={`nav-link${this.state.mode === 'view' ? ' active' : ''}`}
                   href={`/game/${this.gameId}/view`} onClick={this.changeMode}>Overview</a>
            </li>)}
            {(this.props.access.admin || this.props.access.manager) && !this.props.game.readOnly ? editNav : null}
            {!this.props.game.playable || this.state.gameDeleted || this.props.game.training ? null : (<li className="nav-item">
                <a className={`nav-link${this.state.mode === 'play' ? ' active' : ''}`}
                   href={`/game/${this.gameId}/play`} onClick={this.changeMode}>Play</a>
            </li>)}
            {this.state.gameDeleted || this.props.game.readOnly ? null : (<li className="nav-item">
                <a className={`nav-link${this.state.mode === 'availability' ? ' active' : ''}`}
                   href={`/game/${this.gameId}/availability`} onClick={this.changeMode}>Availability</a>
            </li>)}
        </ul>);
    }

    render() {
        if (this.state.loading) {
            return (<div className="d-flex justify-content-center">
                <div className="spinner-border spinner-football" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>);
        }
        if (this.state.error) {
            return (<div>
                <Alert errors={[this.state.error]}/>
                <Link className="btn btn-primary" to="/">Home</Link>
            </div>);
        }
        if (!this.props.access) {
            return (<div>
                <h4>Not logged in</h4>
                <Link to="/" className="btn btn-primary">Home</Link>
            </div>)
        }

        if (!this.props.game || !this.props.game.found) {
            return (<div>
                <Alert errors={[ 'Game not found' ]}/>
                <Link to="/" className="btn btn-primary">Home</Link>
            </div>);
        }

        let component = (<Alert warnings={[`Unknown mode ${this.state.mode}`]}/>);

        if (this.state.mode === 'view' || this.props.game.readOnly) {
            if (this.props.game.training) {
                this.props.updateSocialPreview(`View training`, `Views players`);
            } else {
                this.props.updateSocialPreview(`View game against ${this.props.game.opponent}`, `View players and goals`);
            }
            component = (<ViewGame {...this.props} onGoalRemoved={this.goalRemoved} />);
        } else if (this.state.mode === 'edit') {
            if (this.props.game.training) {
                this.props.updateSocialPreview(`Edit training`, `Edit players, location and date`);
            } else {
                this.props.updateSocialPreview(`Edit game against ${this.props.game.opponent}`, `Edit players, location and date`);
            }
            component = (<EditGame {...this.props} onChanged={this.gameChanged} onDeleted={this.gameDeleted} />);
        } else if (this.state.mode === 'play') {
            if (this.props.game.training) {
                this.props.updateSocialPreview(`n/a`, `n/a`);
            } else {
                this.props.updateSocialPreview(`Record goals in the game against ${this.props.game.opponent}`, `See the score line and update the goals as they're scored`);
            }
            component = (<PlayGame {...this.props} onGoalScored={this.goalScored} />);
        } else if (this.state.mode === 'availability') {
            if (this.props.game.training) {
                this.props.updateSocialPreview(`Record availability for training on ${this.props.game.date}`, `Record the availability of players for this training session`);
            } else {
                this.props.updateSocialPreview(`Record availability for game against ${this.props.game.opponent} on ${this.props.game.date}`, `Record the availability of players for this game`);
            }
            component = (<EditAvailability {...this.props} onAvailabilityChanged={this.availabilityChanged} />);
        }

        return (<div>
            {this.renderHeading()}
            {this.renderNav()}
            <br/>
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

        const content = this.props.game.training
            ? `training at ${this.props.game.playingAtHome ? 'home' : this.props.game.opponent} on ${date.toDateString()}`
            : `${location} to ${this.props.game.opponent} on ${date.toDateString()}`

        return (<h4>
            {this.props.team.name}: {content}
            &nbsp;
            {this.props.game.training ? null : (<Score playingAtHome={this.props.game.playingAtHome} score={score}/>)}
        </h4>);
    }
}
