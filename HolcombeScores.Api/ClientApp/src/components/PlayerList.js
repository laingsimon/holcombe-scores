import React, { Component } from 'react';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Player} from "../api/player";

export class PlayerList extends Component {
    constructor(props) {
        super(props);
        this.state = {
            loading: true,
            players: [],
            selected: props.selected || {},
        };
        const http = new Http(new Settings());
        this.playerApi = new Player(http);
        this.playerClicked = this.playerClicked.bind(this);
    }

    componentDidMount() {
        // noinspection JSIgnoredPromiseFromCall
        this.getPlayers(this.props.teamId);
    }

    // event handlers
    playerClicked(event) {
        const playerNumber = event.currentTarget.getAttribute("data-player-number");
        const currentlySelected = Object.keys(this.state.selected).includes(playerNumber.toString());
        let newSelected = currentlySelected
            ? this.except(this.state.selected, playerNumber)
            : this.union(this.state.selected, playerNumber);

        this.setState({
            selected: newSelected
        });
        this.props.onPlayerChanged(this.props.teamId, playerNumber, !currentlySelected);
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

    // renderers
    renderPlayer(player) {
        let selected = Object.keys(this.state.selected).includes(player.number.toString());
        return (<li key={player.number} className={`list-group-item ${selected ? ' active' : ''}`} data-player-number={player.number} onClick={this.playerClicked}>
            {player.name} <span className="badge rounded-pill bg-secondary">{player.number}</span>
        </li>);
    }

    render() {
        if (this.state.loading) {
            return (<div className="d-flex justify-content-center" style={{ height: '247px' }}>
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>);
        }

        return (<ul className="list-group" style={{ height: '247px', overflow: 'auto' }}>
            {this.state.players.map(player => this.renderPlayer(player))}
        </ul>);
    }

    // api interaction
    async getPlayers() {
        const players = await this.playerApi.getAllPlayers();
        players.sort(this.nameSortFunction);
        this.setState({
            loading: false,
            players: players,
        });

        if (this.props.onLoaded) {
            this.props.onLoaded();
        }
    }

    nameSortFunction(playerA, playerB) {
        if (playerA.name.toLowerCase() === playerB.name.toLowerCase()) {
            return 0;
        }

        return (playerA.name.toLowerCase() > playerB.name.toLowerCase())
            ? 1
            : -1;
    }
}
