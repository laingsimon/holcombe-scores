import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {Team} from '../api/team';
import {Alert} from "./Alert";
import {PlayerList} from "./PlayerList";

export class EditGame extends Component {
    constructor (props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.teamApi = new Team(http);
        this.state = {
            loading: true,
            current: null, // the current game details
            proposed: null // the updated game details
        };
        this.valueChanged = this.valueChanged.bind(this);
        this.updateGame = this.updateGame.bind(this);
        this.deleteGame = this.deleteGame.bind(this);
        this.onPlayerChanged = this.onPlayerChanged.bind(this);
        this.onLoaded = this.onLoaded.bind(this);
    }

    componentDidMount() {
        // noinspection JSIgnoredPromiseFromCall
        this.getGameDetails();
    }

    // event handlers
    valueChanged(event) {
        const name = event.target.name;
        const type = event.target.getAttribute('type');
        const value = type === 'checkbox'
            ? event.target.checked
            : event.target.value;
        const newProposed = Object.assign({}, this.state.proposed);
        newProposed[name] = value;

        this.setState({
            proposed: newProposed
        });
    }

    onPlayerChanged(teamId, playerNumber, selected) {
        const proposedClone = Object.assign({}, this.state.proposed);
        proposedClone.players = selected ? this.union(this.state.proposed.players, playerNumber) : this.except(this.state.proposed.players, playerNumber);

        this.setState({
            proposed: proposedClone
        });
    }

    onLoaded() {
        this.setState({
            playersLoaded: true
        });

        // noinspection JSUnresolvedVariable
        if (this.props.onLoaded) {
            this.props.onLoaded();
        }
    }

    toUtcDateTime(date) {
        const pad = (num) => {
            return num.toString().padStart(2, '0');
        }

        const dateStr = `${date.getUTCFullYear()}-${pad(date.getUTCMonth() + 1)}-${pad(date.getUTCDate())}`;
        const timeStr = `${pad(date.getUTCHours())}:${pad(date.getUTCMinutes())}:00.000Z`;
        return `${dateStr}T${timeStr}`;
    }

    async updateGame() {
        this.setState({
            loading: true,
            updateResult: null,
        });

        try {
            const proposed = this.state.proposed;
            const date = new Date(Date.parse(proposed.date));
            const utcDateTime = this.toUtcDateTime(date);
            const playerNumbers = Object.keys(this.state.proposed.players);
            const result = await this.gameApi.updateGame(this.props.gameId, this.props.teamId, utcDateTime, proposed.opponent, proposed.playingAtHome, playerNumbers);
            this.setState({
                loading: false,
                updateResult: result,
            });

            if (result.success) {
                this.setState({
                    current: result.outcome
                });

                if (this.props.onChanged) {
                    this.props.onChanged(this.props.gameId, this.props.teamId);
                }
            }
        } catch (e) {
            this.setState({
                loading: false,
                error: e.message
            });
        }
    }

    async deleteGame() {
        if (!window.confirm('Are you sure you want to delete this game?')) {
            return;
        }

        this.setState({
            loading: true
        });

        const result = await this.gameApi.deleteGame(this.gameId);
        if (result.success) {
            document.location.href = `/team/${this.state.team.id}`;
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

        return (<div>
            {this.renderUpdateResult(this.state.updateResult)}
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Opponent</span>
                </div>
                <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="opponent" value={this.state.proposed.opponent} onChange={this.valueChanged} />
            </div>
            <div className="input-group mb-3">
                <div className="form-check form-switch">
                    <input className="form-check-input" type="checkbox" id="flexSwitchCheckDefault" name="playingAtHome" checked={this.state.proposed.playingAtHome} onChange={this.valueChanged} />
                    <label className="form-check-label" htmlFor="flexSwitchCheckDefault">Playing at home</label>
                </div>
            </div>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Date</span>
                </div>
                <input type="datetime-local" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="date" value={this.state.proposed.date} onChange={this.valueChanged} />
            </div>
            <PlayerList teamId={this.props.teamId} selected={this.state.proposed.players} onPlayerChanged={this.onPlayerChanged} onLoaded={this.onLoaded} />
            <hr />
            <button type="button" className="btn btn-primary" onClick={this.updateGame}>Update details</button>
            <button className="btn btn-danger" onClick={this.deleteGame}>Delete game</button>
        </div>);
    }

    toLocalDateTime(date) {
        const pad = (num) => {
            return num.toString().padStart(2, '0');
        }

        const dateStr = `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
        const timeStr = `${pad(date.getHours())}:${pad(date.getMinutes())}`;
        return `${dateStr}T${timeStr}`;
    }

    async getGameDetails() {
        const team = await this.teamApi.getTeam(this.props.teamId);
        const game = await this.gameApi.getGame(this.props.gameId);
        const proposedGame = Object.assign({}, game);
        proposedGame.date = this.toLocalDateTime(new Date(game.date));
        proposedGame.players = {};

        // noinspection JSUnresolvedVariable
        delete proposedGame.squad;

        // noinspection JSUnresolvedVariable
        game.squad.forEach(player => {
           proposedGame.players[player.number] = true;
        });

        this.setState({
            loading: false,
            current: game,
            proposed: proposedGame,
            team: team
        });
    }

    except(selected, remove) {
        let copy = Object.assign({}, selected);
        delete copy[remove];
        return copy;
    }

    union(selected, add) {
        let copy = Object.assign({}, selected);
        copy[add] = true;
        return copy;
    }
}
