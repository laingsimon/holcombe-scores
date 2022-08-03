import React, {Component} from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Game} from '../../api/game';
import {Team} from '../../api/team';
import {Access} from '../../api/access';
import {GameOverview} from '../game/GameOverview';
import {EditGame} from '../game/EditGame';
import {EditTeam} from './EditTeam';
import {Alert} from '../Alert';
import { Link } from 'react-router-dom';

/*
* Props:
* - access
* - team
* - reloadTeams()
* - reloadTeam(id, [reloadTeam, reloadPlayers, reloadGames])
*
* Events:
* -none-
* */
// noinspection JSUnresolvedVariable
export class TeamDetails extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.teamApi = new Team(http);
        this.accessApi = new Access(http);
        this.teamId = props.match.params.teamId;
        this.history = props.history;
        const isAdmin = this.props.access.admin;
        this.state = {
            error: !isAdmin && this.teamId !== this.props.access.teamId ? 'No access to team' : null,
            mode: props.match.params.mode || 'view',
            teamDeleted: null,
        };
        this.changeMode = this.changeMode.bind(this);
        this.teamChanged = this.teamChanged.bind(this);
        this.teamDeleted = this.teamDeleted.bind(this);
        this.gameCreated = this.gameCreated.bind(this);
    }

    async componentDidMount() {
        const reloadTeam = false;
        const reloadPlayers = false;
        const reloadGames = false;
        await this.props.reloadTeam(this.teamId, reloadTeam, reloadPlayers, reloadGames);
    }

    // event handlers
    async gameCreated() {
        const reloadTeam = false;
        const reloadPlayers = false;
        const reloadGames = true;
        await this.props.reloadTeam(this.teamId, reloadTeam, reloadPlayers, reloadGames);
    }

    async teamDeleted(teamId) {
        await this.props.reloadTeams();
        this.setState({
            teamDeleted: teamId
        });
    }

    async teamChanged() {
        const reloadTeam = true;
        const reloadPlayers = true;
        const reloadGames = false;
        await this.props.reloadTeam(this.teamId, reloadTeam, reloadPlayers, reloadGames);
    }

    changeMode(event) {
        event.preventDefault();
        const url = event.target.getAttribute('href');
        const segments = url.split('/')
        const mode = segments[segments.length - 1];
        this.setState({
            mode: mode
        });
        window.history.replaceState(null, event.target.textContent, url);
    }

    // renderers
    renderNav() {
        return (<ul className="nav nav-tabs">
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'view' ? ' active' : ''}`}
                   href={`/team/${this.teamId}/view`} onClick={this.changeMode}>View Games</a>
            </li>
            {this.props.access.admin || this.props.access.manager ? (<li className="nav-item">
                <a className={`nav-link${this.state.mode === 'new-game' ? ' active' : ''}`}
                   href={`/team/${this.teamId}/new-game`} onClick={this.changeMode}>New Game</a>
            </li>) : null}
            {this.props.access.admin || this.props.access.manager ? (<li className="nav-item">
                <a className={`nav-link${this.state.mode === 'edit' ? ' active' : ''}`}
                   href={`/team/${this.teamId}/edit`} onClick={this.changeMode}>Edit Team</a>
            </li>) : null}
        </ul>);
    }

    render() {
        try {
            if (!this.props.team) {
                return (<div className="d-flex justify-content-center">
                    <div className="spinner-border" role="status">
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

            if (this.state.teamDeleted) {
                return (<div>
                    <Alert messages={['Team deleted']}/>
                    <Link className="btn btn-primary" to="/teams">View remaining teams</Link>
                </div>);
            }

            if (!this.props.access) {
                return (<Alert warnings={["You need to login again, click on 'Home'"]}/>);
            }

            let component = (<Alert warnings={[`Unknown mode ${this.state.mode}`]}/>);

            if (this.state.mode === 'view') {
                component = this.renderGames(this.props.team.games);
            } else if (this.state.mode === 'new-game') {
                component = (<EditGame {...this.props} onCreated={this.gameCreated}/>);
            } else if (this.state.mode === 'edit') {
                if (this.props.team.found) {
                    component = (<EditTeam {...this.props} onChanged={this.teamChanged} onDeleted={this.teamDeleted}/>);
                } else {
                    component = (<div>
                        <Alert warnings={['Team not found']}/>
                        <Link className="btn btn-primary" to="/teams">View teams</Link>
                    </div>);
                }
            }

            return (<div>
                {this.props.team ? (<h3>{this.props.team.name}</h3>) : null}
                {this.renderNav()}
                <hr/>
                {component}
            </div>);
        } catch (e) {
            console.error(e);
            return (<Alert errors={[ e.message ]} />);
        }
    }

    renderGames(games) {
        if (!games) {
            return (<div>Loading</div>);
        }

        return (<div className="list-group">
            {games.map(g => (<GameOverview reloadGame={this.props.reloadGame} history={this.props.history} key={g.id} game={g} team={this.props.team} />))}
        </div>);
    }
}
