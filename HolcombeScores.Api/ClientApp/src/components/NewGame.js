import React, { Component } from 'react';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Game} from "../api/game";
import {PlayerList} from "./PlayerList";
import {Alert} from "./Alert";

export class NewGame extends Component {
    dateSuffix = ":00.000Z";

    constructor(props) {
        super(props);
        this.state = {
            loading: false,
            players: {},
            opponent: "",
            playingAtHome: true,
            date: this.formatDate(new Date(new Date().setHours(0, 0, 0))),
            playersLoaded: false
        }
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.valueChanged = this.valueChanged.bind(this);
        this.createGame = this.createGame.bind(this);
        this.onPlayerChanged = this.onPlayerChanged.bind(this);
        this.onLoaded = this.onLoaded.bind(this);
        this.teamId = this.props.teamId;
    }

    formatDate(date) {
        const isoDate = date.toISOString();
        return isoDate.substring(0, isoDate.length - (this.dateSuffix.length)); // trim the Z suffix, milliseconds and seconds off the date
    }

    // event handlers
    async createGame() {
        try {
            if (!this.state.playersLoaded) {
                return;
            }

            const playerNumbers = Object.keys(this.state.players);
            const date = new Date(Date.parse(this.state.date));
            const formattedDate = this.formatDate(date) + this.dateSuffix;

            if (playerNumbers.length === 0) {
                alert('You must select some players');
                return;
            }

            if (!this.state.opponent) {
                alert('You must enter the name of the opponent');
                return;
            }

            this.setState({
                loading: true
            });
            const result = await this.gameApi.createGame(this.teamId, formattedDate, this.state.opponent, this.state.playingAtHome, playerNumbers);

            this.setState({
                loading: false,
                created: result
            });

            if (result.success && this.props.onCreated) {
                this.props.onCreated(result.outcome.id);
            }
        } catch (e) {
            this.setState({
                loading: false,
                error: e.message,
            });
        }
    }

    valueChanged(event) {
        const type = event.target.getAttribute('type');

        const value = type === 'checkbox'
            ? event.target.checked
            : event.target.value;

        let newState = {};
        newState[event.target.name] = value;
        this.setState(newState);
    }

    onPlayerChanged(teamId, playerNumber, selected) {
        let newState = selected ? this.union(this.state.players, playerNumber) : this.except(this.state.players, playerNumber);
        this.setState({
            players: newState
        });
    }

    onLoaded() {
        this.setState({
            playersLoaded: true
        });

        if (this.props.onLoaded) {
            this.props.onLoaded();
        }
    }

    // renderers
    renderError(error) {
        const back = (() => {
            this.setState({
                error: null
            });
        });

        return (<div>
            (<Alert errors={[ error ]} />)
            <hr />
            <button type="button" className="btn btn-primary" onClick={back}>Back</button>
        </div>);
    }

    renderCreated(created) {
        const back = (() => {
            this.setState({
                error: null,
                created: null,
            });
        });

        return (<div>
            <Alert messages={created.messages} warnings={created.warnings} errors={created.errors} />
            <hr />
            <button type="button" className="btn btn-light" onClick={back}>Back</button>
            <a href={`/game/${created.outcome.id}`} className="btn btn-primary">Open game</a>
        </div>);
    }

    render() {
        if (this.state.loading) {
            return (<div>Loading...</div>);
        }

        if (this.state.error) {
            return this.renderError(this.state.error);
        }

        if (this.state.created) {
            return this.renderCreated(this.state.created);
        }

        return (<div>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Opponent</span>
                </div>
                <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="opponent" value={this.state.opponent} onChange={this.valueChanged} />
            </div>
            <div className="input-group mb-3">
                <div className="form-check form-switch">
                    <input className="form-check-input" type="checkbox" id="flexSwitchCheckDefault" name="playingAtHome" checked={this.state.playingAtHome ? 'checked' : ''} onChange={this.valueChanged} />
                    <label className="form-check-label" htmlFor="flexSwitchCheckDefault">Playing at home</label>
                </div>
            </div>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Date</span>
                </div>
                <input type="datetime-local" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="date" value={this.state.date} onChange={this.valueChanged} />
            </div>
            <PlayerList teamId={this.teamId} onPlayerChanged={this.onPlayerChanged} onLoaded={this.onLoaded} />
            <hr />
            <button type="button" className={`btn ${this.state.playersLoaded ? 'btn-primary' : 'btn-light'}`} onClick={this.createGame}>Create game</button>
        </div>);
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