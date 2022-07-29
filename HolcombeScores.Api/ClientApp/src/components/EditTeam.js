import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {Team} from '../api/team';
import {Access} from '../api/access';
import {Player} from '../api/player';
import {Alert} from "./Alert";
import {Functions} from '../functions'
import {EditPlayer} from "./EditPlayer";

export class EditTeam extends Component {
    constructor (props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.teamApi = new Team(http);
        this.playerApi = new Player(http);
        this.accessApi = new Access(http);
        this.state = {
            loading: true,
            team: null,
            players: null
        };
        this.valueChanged = this.valueChanged.bind(this);
        this.updateTeam = this.updateTeam.bind(this);
        this.deleteTeam = this.deleteTeam.bind(this);
        this.onPlayerChanged = this.onPlayerChanged.bind(this);
    }

    componentDidMount() {
        // noinspection JSIgnoredPromiseFromCall
        this.getTeamDetails();
    }

    // event handlers
    async onPlayerChanged() {
        this.setState({
            players: await this.getPlayers()
        })
    }

    valueChanged(event) {
        const name = event.target.name;
        const value = event.target.value;
        const newTeam = Object.assign({}, this.state.team);
        newTeam[name] = value;

        this.setState({
            team: newTeam
        });
    }

    async deleteTeam() {
        if (!window.confirm('Are you sure you want to delete this team?')) {
            return;
        }

        this.setState({
            loading: true,
            updateResult: null,
        });

        try {
            const result = await this.teamApi.deleteTeam(this.props.teamId);

            this.setState({
                loading: false,
                deleted: result.success,
                updateResult: result,
            });
        } catch (e) {
            console.error(e);
            this.setState({
                loading: false,
                error: e.message
            });
        }
    }

    async updateTeam() {
        this.setState({
            loading: true,
            updateResult: null,
        });

        try {
            const result = this.props.teamId
                ? await this.teamApi.updateTeam(this.props.teamId, this.state.team.name, this.state.team.coach)
                : await this.teamApi.createTeam(this.state.team.name, this.state.team.coach);

            this.setState({
                loading: false,
                updateResult: result,
            });

            if (result.success) {
                this.setState({
                    team: result.outcome
                });

                if (this.props.onChanged) {
                    this.props.onChanged(result.outcome.id);
                }
            }
        } catch (e) {
            console.error(e);
            this.setState({
                loading: false,
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
        if (this.state.loading) {
            return (<div className="d-flex justify-content-center">
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>);
        }

        if (this.state.error) {
            return (<Alert errors={[ this.state.error ]} />);
        }

        if (this.state.deleted) {
            return (<div>
                {this.renderUpdateResult(this.state.updateResult)}
                <hr />
                <a className="btn btn-primary" href="/teams">View teams</a>
            </div>);
        }

        const deleteTeamButton = this.state.access.admin && this.props.teamId
            ? (<button className="btn btn-danger" onClick={this.deleteTeam}>Delete team</button>)
            : null;

        return (<div>
            {this.renderUpdateResult(this.state.updateResult)}
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Name</span>
                </div>
                <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name" value={this.state.team.name} onChange={this.valueChanged} />
            </div>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Coach</span>
                </div>
                <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="coach" value={this.state.team.coach} onChange={this.valueChanged} />
            </div>
            {this.props.teamId ? <hr /> : null}
            {this.props.teamId ? this.renderEditPlayers() : null}
            <hr />
            <button type="button" className="btn btn-primary" onClick={this.updateTeam}>{this.props.teamId ? 'Update team' : 'Create team'}</button>
            &nbsp;
            {deleteTeamButton}
        </div>);
    }

    renderEditPlayers() {
        return (<div className="container">
            <h4>Players...</h4>
            {this.state.players.map(p => this.renderEditPlayer(p))}
            <EditPlayer key={'new'} teamId={this.props.teamId} newPlayer={true} onPlayerChanged={this.onPlayerChanged} />
        </div>);
    }

    renderEditPlayer(player) {
        return (<EditPlayer key={player.id} teamId={this.props.teamId} player={player} onPlayerChanged={this.onPlayerChanged} />);
    }

    // api
    async getTeamDetails() {
        const team = this.props.teamId
            ? await this.teamApi.getTeam(this.props.teamId)
            : null;
        const access = await this.accessApi.getMyAccess();

        this.setState({
            loading: false,
            players: await this.getPlayers(),
            access: access.access,
            team: team || { name: '', coach: '' }
        });
    }

    async getPlayers() {
        const players = this.props.teamId
            ? await this.playerApi.getPlayers(this.props.teamId)
            : [];

        players.sort(Functions.playerSortFunction);

        return players;
    }
}
