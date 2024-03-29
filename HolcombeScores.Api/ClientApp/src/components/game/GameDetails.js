import React, {Component} from 'react';
import {Http} from '../../api/http';
import {Settings} from '../../api/settings';
import {Game} from '../../api/game';
import {Access} from '../../api/access';
import {Team} from '../../api/team';
import {Alert} from '../Alert';
import {EditGame} from './EditGame';
import {EditAvailability} from './EditAvailability';
import {PlayGame} from './PlayGame';
import {Score} from './Score';
import {ViewGame} from './ViewGame';
import { Link } from 'react-router-dom';
import {Functions} from '../../functions';
import {QRCodeSVG} from 'qrcode.react';
import {ShareIcon} from "../ShareIcon";

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
    static REFRESH_INTERVAL = 5000;
    static AVAILABILITY_ENABLED = false;

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
            mode: props.match.params.mode || 'view',
            showSharingDetail: false,
            refreshHandle: window.setInterval(this.refresh.bind(this), GameDetails.REFRESH_INTERVAL),
            refreshEnabled: true,
            refreshing: false
        };
        this.changeMode = this.changeMode.bind(this);
        this.goalScored = this.goalScored.bind(this);
        this.goalRemoved = this.goalRemoved.bind(this);
        this.gameChanged = this.gameChanged.bind(this);
        this.gameDeleted = this.gameDeleted.bind(this);
        this.availabilityChanged = this.availabilityChanged.bind(this);
        this.toggleSharingDetail = this.toggleSharingDetail.bind(this);
        this.goalNotRecorded = this.goalNotRecorded.bind(this);
        this.stopRefresh = this.stopRefresh.bind(this);
    }

    //event handlers
    toggleSharingDetail(event) {
        event.preventDefault();

        this.setState({
            showSharingDetail: !this.state.showSharingDetail
        });
    }

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

    async goalNotRecorded(gameId, holcombeGoal, playerId) {
        if (!gameId) {
            await this.props.reloadGame(gameId, true, true); // don't set the state to loading
            return;
        }

        const repairedGoals = this.props.game.goals.filter(g => {
            const goalThatCouldNotBeRecorded = g.local && g.holcombeGoal === holcombeGoal && (g.holcombeGoal ? g.player.id === playerId : g.player === null);
            return !goalThatCouldNotBeRecorded;
        });
        this.props.game.goals = repairedGoals;

        await this.props.updateGame(this.props.game);
        await this.props.reloadGame(gameId);
    }

    async goalScored(gameId, holcombeGoal, playerId) {
        if (!gameId) {
            await this.props.reloadGame(gameId, true, true); // don't set the state to loading
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

        await this.props.updateGame(this.props.game);
    }

    changeMode(event) {
        event.preventDefault();
        const url = event.target.getAttribute('href');
        const segments = url.split('/')
        const mode = segments[segments.length - 1];
        this.setState({
            mode: mode,
            showSharingDetail: false
        });
        window.history.replaceState(null, event.target.textContent, url);
    }

    shouldRefresh() {
        if (this.props.game.training || this.props.game.postponed) {
            return false;
        }

        if (this.props.game.started) {
            if (!this.props.game.playable || this.props.game.readOnly) {
                return false;
            }
        }

        return true;
    }

    async refresh() {
        if (this.state.mode === 'edit' || !this.state.refreshEnabled || !this.props.game || this.state.refreshing) {
            return;
        }

        try {
            const now = new Date().getTime();
            const asAt = this.props.game.asAt.getTime();
            const diff = now - asAt;

            if (!this.shouldRefresh()) {
                this.setState({
                    refreshEnabled: false
                });
                return;
            }

            if (diff > (GameDetails.REFRESH_INTERVAL)) { // half the refresh interval
                this.setState({
                    refreshing: true
                });

                await this.props.reloadGame(this.gameId, true, true);
            }
        } catch (e) {
            console.error(e);
            this.setState({
                refreshEnabled: false
            });
        }
        finally {
            this.setState({
                refreshing: false
            });
        }
    }

    stopRefresh() {
        this.setState({
            refreshEnabled: false
        })
    }

    async componentDidMount() {
        if (this.props.game) {
            this.setState({
                loading: false,
                refreshEnabled: this.shouldRefresh()
            });

            return;
        }

        if (this.gameId) {
            await this.props.reloadGame(this.gameId);

            this.setState({
                loading: false,
                refreshEnabled: this.shouldRefresh()
            });
        }
    }

    componentWillUnmount() {
        this.stopRefresh();
        if (this.state.refreshHandle) {
            window.clearInterval(this.state.refreshHandle);
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
                <Link className={`nav-link${this.state.mode === 'view' ? ' active' : ''}`}
                   to={`/game/${this.gameId}/view`} onClick={this.changeMode}>Overview</Link>
            </li>)}
            {(this.props.access.admin || this.props.access.manager) && !this.props.game.readOnly ? editNav : null}
            {!this.props.game.playable || this.state.gameDeleted || this.props.game.training ? null : (<li className="nav-item">
                <Link className={`nav-link${this.state.mode === 'play' ? ' active' : ''}`}
                   to={`/game/${this.gameId}/play`} onClick={this.changeMode}>Play</Link>
            </li>)}
            {!GameDetails.AVAILABILITY_ENABLED || this.state.gameDeleted || this.props.game.started ? null : (<li className="nav-item">
                <Link className={`nav-link${this.state.mode === 'availability' ? ' active' : ''}`}
                   to={`/game/${this.gameId}/availability`} onClick={this.changeMode}>Availability</Link>
            </li>)}
            <li className="nav-item">
                <a className="nav-link" onClick={this.toggleSharingDetail}
                      href={`/game/${this.gameId}/share`}><ShareIcon /></a>
                {this.state.showSharingDetail ? this.renderSharingComponent() : null}
            </li>
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

        if (this.state.mode === 'view') {
            component = (<ViewGame {...this.props} onGoalRemoved={this.goalRemoved} />);
        } else if (this.state.mode === 'edit') {
            component = (<EditGame {...this.props} onChanged={this.gameChanged} onDeleted={this.gameDeleted} />);
        } else if (this.state.mode === 'play') {
            component = (<PlayGame {...this.props} onGoalScored={this.goalScored} onGoalNotRecorded={this.goalNotRecorded} />);
        } else if (GameDetails.AVAILABILITY_ENABLED && this.state.mode === 'availability') {
            component = (<EditAvailability {...this.props} onAvailabilityChanged={this.availabilityChanged} />);
        }

        return (<div>
            {this.renderHeading()}
            {this.renderNav()}
            <br/>
            {component}
            <hr />
            <div className="text-center">
                {this.state.refreshing ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                As at {this.props.game.asAt.toLocaleTimeString()}
                &nbsp;-&nbsp;
                {this.state.refreshEnabled && this.state.mode !== 'edit'
                    ? (<button className="btn btn-secondary" onClick={this.stopRefresh}>Stop Refresh</button>)
                    : <span>Not refreshing</span>}
            </div>
        </div>)
    }

    renderSharingComponent() {
        return (<div className="floating-drop-down">
            <QRCodeSVG includeMargin={true} value={Functions.getSharingLink()} />
        </div>);
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
            : `${location} to ${this.props.game.opponent} on ${date.toDateString()}`;

        return (<h4>
            {this.props.team.name}: {content}{this.props.game.postponed ? ' (postponed)' : null}{this.props.game.friendly ? ' (friendly)' : null}
            &nbsp;
            {this.props.game.training || this.props.game.postponed || !this.props.game.hasStarted
                ? null
                : (<Score playingAtHome={this.props.game.playingAtHome} score={score}/>)}
        </h4>);
    }
}
