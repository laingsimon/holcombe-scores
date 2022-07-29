import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {Team} from '../api/team';
import {Access} from '../api/access';
import {Player} from '../api/player';
import {Functions} from '../functions'

export class EditPlayer extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.teamApi = new Team(http);
        this.playerApi = new Player(http);
        this.accessApi = new Access(http);
        this.state = Object.assign({
            saving: false,
            changed: false
        }, props.player || { name: '', number: '' });
        this.playerValueChanged = this.playerValueChanged.bind(this);
        this.deletePlayer = this.deletePlayer.bind(this);
        this.savePlayer = this.savePlayer.bind(this);

        this.id = props.player ? props.player.id : '00000000-0000-0000-0000-000000000000';
        this.teamId = props.teamId;
    }

    //events
    playerChanged() {
        if (this.props.onPlayerChanged) {
            this.props.onPlayerChanged(this.id);
        }
    }

    // event handlers
    async playerValueChanged(event) {
        const name = event.target.name;
        const value = event.target.value;

        const newState = Object.assign({}, this.state);
        newState.changed = true;
        newState[name] = value;

        this.setState(newState);
    }

    async deletePlayer() {
        try {
            if (this.state.saving) {
                return;
            }

            if (!window.confirm(`Are you sure you want to delete ${this.state.name}`)){
                return;
            }

            this.setState({
                saving: true
            });

            const result = await this.playerApi.deletePlayer(this.id);

            if (result.success) {
                this.setState({
                    saving: false
                });
                this.playerChanged();
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
            if (this.state.saving) {
                return;
            }

            const number = this.state.number ? Number.parseInt(this.state.number) : null;
            if (number && (number <= 0 || number === Number.NaN)) {
                alert('Invalid player number, must be a whole positive number');
                return;
            }

            this.setState({
                saving: true
            });

            const result = await this.playerApi.updatePlayer(this.id, this.teamId, number, this.state.name);
            if (result.success) {
                this.setState({
                    saving: false,
                    changed: false,
                    name: this.props.newPlayer ? '' : this.state.name,
                    number: this.props.newPlayer ? '' : this.state.number,
                });
                this.playerChanged();
            } else {
                alert(`Could not ${this.props.newPlayer ? 'create' : 'update'} player: ${Functions.getResultMessages(result)}`);
                this.setState({
                    saving: false,
                });
                console.log(result);
            }
        } catch (e) {
            console.log(e);
            this.setState({
                saving: false,
                error: e
            });
        }
    }

    // renderers
    render() {
        const saveButton = (<button className={`btn ${this.state.changed && this.state.name ? 'btn-success' : 'btn-light'}`} onClick={this.savePlayer}>{this.props.newPlayer ? '➕' : '💾'}</button>);
        const savingButton = (<button className="btn btn-light"><span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>&nbsp;</button>);

        return (<div className="row">
            <div className="col">
                <div className="input-group mb-3">
                    <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name" value={this.state.name} onChange={this.playerValueChanged} />
                </div>
            </div>
            <div className="col">
                <div className="input-group mb-3">
                    <div className="input-group-prepend">
                        <span className="input-group-text" id="basic-addon3">#️⃣</span>
                    </div>
                    <input type="number" min="1" max="50" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="number" value={this.state.number} onChange={this.playerValueChanged} />
                </div>
            </div>
            <div className="col">
                {this.state.saving ? savingButton : saveButton}
                &nbsp;
                {this.props.newPlayer ? null : (<button className={`btn ${this.state.saving ? 'btn-light' : 'btn-danger'}`} onClick={this.deletePlayer}>🗑</button>)}
            </div>
        </div>);
    }
}