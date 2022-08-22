import React, {Component} from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Availability} from '../../api/availability';
import {Functions} from "../../functions";

/*
* Props:
* - availability
*
* Events:
* - onAvailabilityChanged(gameId, playerId, available)
*/
export class EditPlayerAvailability extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.availabilityApi = new Availability(http);
        this.state = {
            saving: false
        };
        this.playerClicked = this.playerClicked.bind(this);
    }

    // event handlers
    async playerClicked() {
        try {
            this.setState({
                saving: true
            });

            const availability = this.props.availability;

            const nowAvailable = availability.id === null
                ? true // change null -> available
                : (availability.available === true ? false : null);

            if (nowAvailable === null) {
                await this.availabilityApi.delete(availability.team.id, availability.gameId, availability.player.id);
            } else {
                await this.availabilityApi.update(availability.team.id, availability.gameId, availability.player.id, nowAvailable);
            }

            if (this.props.onAvailabilityChanged) {
                await this.props.onAvailabilityChanged(availability.gameId, availability.player.id, nowAvailable);
            }
        }
        catch (e) {
            console.error(e);
            alert(`Unable to update availability: ${Functions.getResultMessages(e)}`);
        }

        this.setState({
            saving: false
        });
    }

    // renders
    render() {
        const className = this.props.availability.id
            ? (this.props.availability.available ? 'btn-success' : 'btn-warning')
            : 'btn-outline-secondary';
        const note = this.props.availability.id
            ? (this.props.availability.available ? 'is available' : 'is unavailable')
            : null;

        return (<button type="button" className={`btn ${className} btn-large-player-button margin-right`} onClick={this.playerClicked}>
            {this.state.saving ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
            {this.props.availability.player.name}
            {note != null ? <div className="small-text">{note}</div> : null}
        </button>);
    }
}
