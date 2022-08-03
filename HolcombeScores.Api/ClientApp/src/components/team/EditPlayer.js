import React, { Component } from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Game} from '../../api/game';
import {Team} from '../../api/team';
import {Access} from '../../api/access';
import {Player} from '../../api/player';
import {Functions} from '../../functions'

/*
* Props:
* - team
* - [player]
* - [readOnly]
*
* Events:
* - onPlayerChanged(playerId, teamId)
* - onPlayerCreated(playerId, teamId)
* - onPlayerDeleted(playerId, teamId)
*/
export class EditPlayer extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.teamApi = new Team(http);
        this.playerApi = new Player(http);
        this.accessApi = new Access(http);
        this.state = {
            saving: false,
            changed: false,
            proposed: props.player ? Object.assign({}, props.player) : { name: '', number: '' }
        };
        this.playerValueChanged = this.playerValueChanged.bind(this);
        this.deletePlayer = this.deletePlayer.bind(this);
        this.savePlayer = this.savePlayer.bind(this);
    }

    // event handlers
    async playerValueChanged(event) {
        const name = event.target.name;
        const value = event.target.value;

        const newProposed = Object.assign({}, this.state.proposed);
        newProposed[name] = value;

        this.setState({
            proposed: newProposed,
            changed: this.props.player ? this.hasChanged(newProposed) : true
        });
    }

    async deletePlayer() {
        try {
            if (this.state.saving || this.props.readOnly) {
                return;
            }

            if (!window.confirm(`Are you sure you want to delete ${this.props.player.name}`)){
                return;
            }

            this.setState({
                saving: true
            });

            const result = await this.playerApi.deletePlayer(this.props.player.id);

            if (result.success) {
                this.setState({
                    saving: false
                });

                if (this.props.onPlayerDeleted) {
                    await this.props.onPlayerDeleted(this.props.player.id, this.props.team.id);
                }
            } else {
                alert(`Could not delete player: ${Functions.getResultMessages(result)}`);
                console.log(result);
            }
        } catch (e) {
            console.log(e);
            this.setState({
                error: e
            });
        }
    }

    async savePlayer() {
        try {
            if (this.state.saving || this.props.readOnly) {
                return;
            }

            const number = this.state.proposed.number ? Number.parseInt(this.state.proposed.number) : null;
            if (number && (number <= 0 || isNaN(number))) {
                alert('Invalid player number, must be a whole positive number');
                return;
            }

            this.setState({
                saving: true
            });

            const id = this.props.player ? this.props.player.id : '00000000-0000-0000-0000-000000000000';
            const result = await this.playerApi.updatePlayer(id, this.props.team.id, number, this.state.proposed.name);
            if (result.success) {
                this.setState({
                    saving: false,
                    changed: false,
                    proposed: this.props.player ? result.outcome : { name: '', number: '' },
                });

                if (this.props.player) {
                    if (this.props.onPlayerChanged) {
                        await this.props.onPlayerChanged(this.props.player.id, this.props.team.id);
                    }
                } else {
                    if (this.props.onPlayerCreated) {
                        await this.props.onPlayerCreated(result.outcome.id, this.props.team.id);
                    }
                }
            } else {
                alert(`Could not ${this.props.player ? 'update' : 'create'} player: ${Functions.getResultMessages(result)}`);
                this.setState({
                    saving: false,
                });
                console.log(result);
            }
        } catch (e) {
            console.error(e);
            this.setState({
                saving: false,
                error: e
            });
        }
    }

    // renderers
    render() {
        return (<div className="row">
            <div className="col">
                <div className="input-group mb-3">
                    <input readOnly={this.props.readOnly} type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name" value={this.state.proposed.name || ''} onChange={this.playerValueChanged} />
                </div>
            </div>
            <div className="col">
                <div className="input-group mb-3">
                    <div className="input-group-prepend">
                        <span className="input-group-text" id="basic-addon3">#️⃣</span>
                    </div>
                    <input readOnly={this.props.readOnly} type="number" min="1" max="50" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="number" value={this.state.proposed.number || ''} onChange={this.playerValueChanged} />
                </div>
            </div>
            <div className="col">
                <button className={`btn ${!this.state.saving && this.state.changed && this.state.proposed.name && !this.props.readOnly ? 'btn-success' : 'btn-light'} margin-right`} onClick={this.savePlayer}>
                    {this.state.saving
                        ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>)
                        : this.props.player ? '💾' : '➕'}
                </button>
                {this.props.player ? (<button className={`btn ${this.state.saving ? 'btn-light' : 'btn-danger'}`} onClick={this.deletePlayer}>🗑</button>) : null}
            </div>
        </div>);
    }

    hasChanged(newProposed) {
        return newProposed.name !== this.props.player.name
        || newProposed.number !== this.props.player.number;
    }
}
