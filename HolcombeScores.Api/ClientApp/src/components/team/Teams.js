import React, {Component} from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Access} from '../../api/access';
import {TeamOverview} from './TeamOverview';
import {Alert} from '../Alert';
import {EditTeam} from './EditTeam';
import { Link } from 'react-router-dom';
import {Functions} from '../../functions';

/*
* Props:
* - teams
* - access
* - reloadTeams()
*
* Events:
* -none-
* */
export class Teams extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.history = props.history;
        this.state = {
            error: null,
            mode: props.match.params.mode || 'view',
            teamCreated: null
        };
        this.changeMode = this.changeMode.bind(this);
        this.onTeamCreated = this.onTeamCreated.bind(this);
    }

    //event handlers
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

    async onTeamCreated(teamId) {
        await this.props.reloadTeams();
        this.setState({
            teamCreated: teamId
        });
    }

    componentDidMount() {
        try {
            if (!this.props.access) {
                this.setState({ error: 'You need to request access first' });
            }
        } catch (e) {
            console.error(e);
            this.setState({ error: e.message });
        }
    }

    // renderers
    renderNav() {
        return (<ul className="nav nav-tabs">
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'view' ? ' active' : ''}`} href={`/teams/view`}
                   onClick={this.changeMode}>View Teams</a>
            </li>
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'new' ? ' active' : ''}`} href={`/teams/new`}
                   onClick={this.changeMode}>New Team</a>
            </li>
        </ul>);
    }

    render() {
        if (this.state.error) {
            return (<Alert errors={[this.state.error]}/>);
        }

        if (this.state.teamCreated) {
            return (<div>
                <Alert messages={[ 'Team created' ]}/>
                <Link className="btn btn-primary" to={`/team/${this.state.teamCreated}/edit`}>Edit players</Link>
            </div>);
        }

        if (this.state.mode === 'view') {
            return this.renderTeams(this.props.teams);
        }
        if (this.state.mode === 'new') {
            return this.renderNewTeam();
        }
    }

    renderNewTeam() {
        return (<div>
            {this.renderNav()}
            <br/>
            <EditTeam {...Functions.except(this.props, 'team')} onCreated={this.onTeamCreated} />
        </div>);
    }

    renderTeams(teams) {
        return (<div>
            {this.renderNav()}
            <br/>
            <div className="list-group">
                {teams.map(team => <TeamOverview reloadTeam={this.props.reloadTeam} history={this.props.history} key={team.id} team={team} />)}
            </div>
        </div>);
    }
}
