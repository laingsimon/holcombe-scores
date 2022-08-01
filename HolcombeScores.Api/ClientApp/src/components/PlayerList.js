import React, { Component } from 'react';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Player} from "../api/player";
import {Functions} from '../functions'

/*
* Props:
* - selected
* - teamId
* -
*
* Events
* - onPlayerSelected(teamId, playerId, selected)
* - onLoaded()
* */
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
        const playerId = event.currentTarget.getAttribute("data-player-id");
        const currentlySelected = Object.keys(this.state.selected).includes(playerId);
        let newSelected = currentlySelected
            ? Functions.except(this.state.selected, playerId)
            : Functions.union(this.state.selected, playerId);

        this.setState({
            selected: newSelected
        });
        this.props.onPlayerSelected(this.props.teamId, playerId, !currentlySelected);
    }

    // renderers
    renderPlayer(player) {
        let selected = Object.keys(this.state.selected).includes(player.id.toString());
        return (<li key={player.id} className={`list-group-item ${selected ? ' active' : ''}`} data-player-id={player.id} onClick={this.playerClicked}>
            {player.name} {player.number ? (<span className="badge rounded-pill bg-secondary">{player.number}</span>) : null}
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
        const players = await this.playerApi.getPlayers(this.props.teamId);
        players.sort(Functions.playerSortFunction);
        this.setState({
            loading: false,
            players: players,
        });

        if (this.props.onLoaded) {
            this.props.onLoaded();
        }
    }
}
