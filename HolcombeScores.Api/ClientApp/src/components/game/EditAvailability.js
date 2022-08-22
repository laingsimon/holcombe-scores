import React, {Component} from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Availability} from '../../api/availability';
import {Alert} from '../Alert';
import {EditPlayerAvailability} from "./EditPlayerAvailability";

/*
* Props:
* - [game]
* - team
*
* Events:
* - onAvailabilityChanged(gameId, playerId, available)
*/
export class EditAvailability extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.availabilityApi = new Availability(http);
        this.state = {
            saving: false
        };
        this.availabilityChanged = this.availabilityChanged.bind(this);
    }

    // event handlers
    async availabilityChanged(gameId, playerId, available) {
        if (this.props.onAvailabilityChanged) {
            await this.props.onAvailabilityChanged(gameId, playerId, available);
        }
    }

    // renders
    render() {
        try {
            return (<div>
                <h4>Tap a name to change availability</h4>
                <div className="d-flex flex-wrap justify-content-center">
                    {this.props.gameAvailability.map(availability => (<EditPlayerAvailability key={availability.player.id} {...this.props} onAvailabilityChanged={this.availabilityChanged} availability={availability} />))}
                </div>
            </div>);
        } catch (e) {
            console.error(e);
            return (<Alert errors={[e.message]}/>);
        }
    }
}
