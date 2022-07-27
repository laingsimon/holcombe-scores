import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {Team} from '../api/team';
import {Access} from '../api/access';
import {Player} from '../api/player';
import {Alert} from "./Alert";
import {Functions} from '../functions'

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
            current: null, // the current team details
            proposed: null // the updated team details
        };
        this.valueChanged = this.valueChanged.bind(this);
        this.updateTeam = this.updateTeam.bind(this);
        this.deleteTeam = this.deleteTeam.bind(this);
        this.playerValueChanged = this.playerValueChanged.bind(this);
        this.deletePlayer = this.deletePlayer.bind(this);
        this.savePlayer = this.savePlayer.bind(this);
    }

    componentDidMount() {
        // noinspection JSIgnoredPromiseFromCall
        this.getTeamDetails();
    }

    // event handlers
    valueChanged(event) {
        const name = event.target.name;
        const value = event.target.value;
        const newProposed = Object.assign({}, this.state.proposed);
        newProposed[name] = value;

        this.setState({
            proposed: newProposed
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
            const proposed = this.state.proposed;
            const result = this.props.teamId
                ? await this.teamApi.updateTeam(this.props.teamId, proposed.name, proposed.coach)
                : await this.teamApi.createTeam(proposed.name, proposed.coach);

            this.setState({
                loading: false,
                updateResult: result,
            });

            if (result.success) {
                this.setState({
                    current: result.outcome
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

    async playerValueChanged(event) {
        const playerId = event.target.getAttribute('data-player-id');
        const name = event.target.name;
        const value = event.target.value;

        const proposedPlayers = this.state.proposedPlayers;
        const player = proposedPlayers.filter(p => p.id === playerId)[0];
        player[name] = value;
        player.changed = true;

        this.setState({
            proposedPlayers: proposedPlayers
        });
    }

    async deletePlayer(event) {
        const playerId = event.target.getAttribute('data-player-id');

        try {
            const proposedPlayers = this.state.proposedPlayers;
            const player = proposedPlayers.filter(p => p.id === playerId)[0];

            if (!window.confirm(`Are you sure you want to delete ${player.name}`)){
                return;
            }

            if (player.saving) {
                return;
            }

            player.saving = true;
            this.setState({
                proposedPlayers: proposedPlayers
            });

            const result = await this.playerApi.deletePlayer(playerId);

            if (result.success) {
                const proposedPlayers = await this.getPlayers();
                this.setState({
                    proposedPlayers: proposedPlayers
                })
            } else {
                let messages = [];
                result.messages.forEach(m => messages.push(m));
                result.warnings.forEach(m => messages.push('Warning: ' + m));
                result.errors.forEach(m => messages.push('Error: ' + m));

                alert(`Could not delete player: ${messages.join('\n')}`);
                console.log(result);
            }
        } catch (e) {
            console.log(e);
            this.setState({
                error: e
            });
        }
    }

    async savePlayer(event) {
        try {
            const playerId = event.target.getAttribute('data-player-id');
            const proposedPlayers = this.state.proposedPlayers;
            const player = proposedPlayers.filter(p => p.id === playerId)[0];

            if (player.saving) {
                return;
            }

            if (Number.parseInt(player.number) <= 0) {
                alert('You must enter a player number');
                return;
            }

            player.saving = true;
            this.setState({
                proposedPlayers: proposedPlayers
            });

            const result = await this.playerApi.updatePlayer(player.id, this.props.teamId, player.number, player.name);
            if (result.success) {
                const proposedPlayers = await this.getPlayers();
                this.setState({
                    proposedPlayers: proposedPlayers
                });
            } else {
                let messages = [];
                result.messages.forEach(m => messages.push(m));
                result.warnings.forEach(m => messages.push('Warning: ' + m));
                result.errors.forEach(m => messages.push('Error: ' + m));

                alert(`Could not ${player.newPlayer ? 'create' : 'update'} player: ${messages.join('\n')}`);
                console.log(result);
            }
        } catch (e) {
            console.log(e);
            this.setState({
                error: e
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
                <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name" value={this.state.proposed.name} onChange={this.valueChanged} />
            </div>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Coach</span>
                </div>
                <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="coach" value={this.state.proposed.coach} onChange={this.valueChanged} />
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
            {this.state.proposedPlayers.map(p => this.renderEditPlayer(p))}
        </div>);
    }

    renderEditPlayer(player) {
        return (<div className="row" key={player.id}>
            <div className="col">
                <div className="input-group mb-3">
                    <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name" data-player-id={player.id} value={player.name} onChange={this.playerValueChanged} />
                </div>
            </div>
            <div className="col">
                <div className="input-group mb-3">
                    <div className="input-group-prepend">
                        <span className="input-group-text" id="basic-addon3">No.</span>
                    </div>
                    <input type="number" min="1" max="50" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="number" data-player-id={player.id} value={player.number} onChange={this.playerValueChanged} />
                </div>
            </div>
            <div className="col">
                <button disabled={player.saving} className={`btn ${player.changed && !player.saving ? 'btn-success' : 'btn-light'}`} data-player-id={player.id} onClick={this.savePlayer}>{player.newPlayer ? 'Add' : 'Save'}</button>
                &nbsp;
                {player.newPlayer ? null : (<button className="btn btn-danger" data-player-id={player.id} onClick={this.deletePlayer}>&times;</button>)}
            </div>
        </div>);
    }

    // api
    async getTeamDetails() {
        const team = this.props.teamId
            ? await this.teamApi.getTeam(this.props.teamId)
            : null;
        const access = await this.accessApi.getMyAccess();
        const proposedPlayers = await this.getPlayers();

        this.setState({
            loading: false,
            current: team,
            access: access.access,
            proposedPlayers: proposedPlayers,
            proposed: team || { name: '', coach: '' }
        });
    }

    async getPlayers() {
        const players = this.props.teamId
            ? await this.playerApi.getPlayers(this.props.teamId)
            : null;
        const proposedPlayers = players || [];

        proposedPlayers.forEach(p => {
            p.changed = false;
            p.saving = false;
        });
        proposedPlayers.sort(Functions.playerSortFunction);
        proposedPlayers.push({ id: '00000000-0000-0000-0000-000000000000', number: '', name: '', newPlayer: true });

        return proposedPlayers;
    }
}
