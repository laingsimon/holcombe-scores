import React, { Component } from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Game} from '../../api/game';
import {Team} from '../../api/team';
import {Access} from '../../api/access';
import {Player} from '../../api/player';
import {Alert} from '../Alert';
import {EditPlayer} from './EditPlayer';
import { Link } from 'react-router-dom';

/*
* Props:
* - [team]
* - access
* - reloadTeam(teamId, [reloadTeam, reloadPlayers, reloadGames])
* - reloadTeams()
*
* Events:
* - onChanged(teamId)
* - onDeleted(teamId)
* - onCreated(teamId)
*/
export class EditTeam extends Component {
    constructor (props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.teamApi = new Team(http);
        this.playerApi = new Player(http);
        this.accessApi = new Access(http);
        this.state = {
            saving: false,
            deleting: false,
            proposed: Object.assign({}, this.props.team || { name: '', coach: '' }),
            players: null
        };
        this.valueChanged = this.valueChanged.bind(this);
        this.updateTeam = this.updateTeam.bind(this);
        this.deleteTeam = this.deleteTeam.bind(this);
        this.onPlayerChanged = this.onPlayerChanged.bind(this);
    }

    // event handlers
    async onPlayerChanged() {
        const reloadTeam = false;
        const reloadPlayers = true;
        const reloadGames = false;
        await this.props.reloadTeam(this.props.team.id, reloadTeam, reloadPlayers, reloadGames);
    }

    valueChanged(event) {
        const name = event.target.name;
        const value = event.target.value;
        const newTeam = Object.assign({}, this.state.proposed);
        newTeam[name] = value;

        this.setState({
            proposed: newTeam
        });
    }

    async deleteTeam() {
        if (this.state.deleting || this.state.saving) {
            return;
        }

        if (!window.confirm('Are you sure you want to delete this team?')) {
            return;
        }

        this.setState({
            deleting: true,
            updateResult: null,
        });

        try {
            const result = await this.teamApi.deleteTeam(this.props.team.id);

            if (result.success) {
                await this.props.reloadTeams();

                if (this.props.onDeleted) {
                    await this.props.onDeleted(this.props.team.id);
                }
            }

            this.setState({
                deleting: false,
                deleted: result.success,
                updateResult: result,
            });
        } catch (e) {
            console.error(e);
            this.setState({
                deleting: false,
                error: e.message
            });
        }
    }

    async updateTeam() {
        if (this.state.deleting || this.state.saving) {
            return;
        }

        this.setState({
            saving: true,
            updateResult: null,
        });

        try {
            const result = this.props.team
                ? await this.teamApi.updateTeam(this.props.team.id, this.state.proposed.name, this.state.proposed.coach)
                : await this.teamApi.createTeam(this.state.proposed.name, this.state.proposed.coach);

            if (result.success) {
                if (this.props.team) {
                    if (this.props.onChanged) {
                        await this.props.onChanged(result.outcome.id);
                    }
                } else {
                    if (this.props.onCreated) {
                        await this.props.onCreated(result.outcome.id);
                    }
                }
            }

            this.setState({
                saving: false,
                updateResult: result,
            });
        } catch (e) {
            console.error(e);
            this.setState({
                saving: false,
                error: e.message
            });
        }
    }

    // renders
    renderUpdateResult(result) {
        if (!result) {
            return;
        }

        if (result.success) {
            return (<Alert messages={result.messages} />);
        }

        return (<div>
            <Alert messages={result.messages} warnings={result.warnings} errors={result.errors} />
        </div>);
    }

    render () {
        if (this.state.error) {
            return (<Alert errors={[ this.state.error ]} />);
        }

        if (this.state.deleted) {
            return (<div>
                {this.renderUpdateResult(this.state.updateResult)}
                <hr />
                <Link className="btn btn-primary" to="/teams">View teams</Link>
            </div>);
        }

        return (<div>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Name</span>
                </div>
                <input readOnly={this.state.deleting || this.state.saving} type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name" value={this.state.proposed.name} onChange={this.valueChanged} />
            </div>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Coach</span>
                </div>
                <input readOnly={this.state.deleting || this.state.saving} type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="coach" value={this.state.proposed.coach} onChange={this.valueChanged} />
            </div>
            <button type="button" className="btn btn-primary margin-right" onClick={this.updateTeam}>
                {this.state.saving ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                {this.props.team ? 'Update team' : 'Create team'}
            </button>
            {this.props.access.admin && this.props.team
                ? (<button className="btn btn-danger" onClick={this.deleteTeam}>
                    {this.state.deleting ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                    Delete team
                </button>)
                : null}
            {this.props.team ? <hr /> : null}
            {this.renderUpdateResult(this.state.updateResult)}
            {this.props.team ? this.renderEditPlayers() : null}
        </div>);
    }

    renderEditPlayers() {
        return (<div className="container">
            <h4>Players...</h4>
            {this.props.team.players.map(p => this.renderEditPlayer(p))}
            {this.state.deleting || this.state.saving ? null : (<EditPlayer key={'new'} team={this.props.team} newPlayer={true} onPlayerCreated={this.onPlayerChanged} />)}
        </div>);
    }

    renderEditPlayer(player) {
        return (<EditPlayer readOnly={this.state.saving || this.state.deleting} key={player.id} team={this.props.team} player={player} onPlayerDeleted={this.onPlayerChanged} onPlayerChanged={this.onPlayerChanged} />);
    }
}
