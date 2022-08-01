import React, { Component } from 'react';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Player} from "../api/player";
import {Alert} from "./Alert";

/*
* Props:
* - [selected]
* - players
* - [teamId]
* - [readOnly]
*
* Events
* - onPlayerSelected(teamId, playerId, selected)
* */
export class PlayerList extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.playerApi = new Player(http);
        this.playerClicked = this.playerClicked.bind(this);
    }

    // event handlers
    async playerClicked(event) {
        if (this.props.readOnly) {
            return;
        }

        const playerId = event.currentTarget.getAttribute("data-player-id");
        const currentlySelected = Object.keys(this.props.selected || {}).includes(playerId);

        await this.props.onPlayerSelected(this.props.teamId, playerId, !currentlySelected);
    }

    // renderers
    renderPlayer(player) {
        let selected = Object.keys(this.props.selected || {}).includes(player.id.toString());
        return (<li key={player.id} className={`list-group-item ${selected ? ' active' : ''}`} data-player-id={player.id} onClick={this.playerClicked}>
            {player.name} {player.number ? (<span className="badge rounded-pill bg-secondary">{player.number}</span>) : null}
        </li>);
    }

    render() {
        if (!this.props.players) {
            return (<Alert errors={[ 'props.players not supplied' ]} />);
        }

        try {
            return (<ul className="list-group" style={{height: '247px', overflow: 'auto'}}>
                {this.props.players.map(player => this.renderPlayer(player))}
            </ul>);
        } catch (e) {
            console.error(e);
            return (<Alert errors={[ e.message ]} />);
        }
    }
}
