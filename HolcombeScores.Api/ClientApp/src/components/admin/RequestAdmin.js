import React, { Component } from 'react';
import {RequestOverview} from './RequestOverview';

/*
* Props:
* - requests
* - teams
*
* Events:
* - onRequestChanged()
*/
export class RequestAdmin extends Component {
    constructor (props) {
        super(props);
        this.requestChanged = this.requestChanged.bind(this);
    }

    //event handlers
    async requestChanged() {
        if (this.props.onRequestChanged) {
            await this.props.onRequestChanged();
        }
    }

    // renderers
    render() {
        return (<div>
            <h5>Pending: {this.props.requests.filter(r => !r.rejected).length}</h5>
            <div className="list-group">
                {this.props.requests.filter(r => !r.rejected).map(request => this.renderRequest(request))}
            </div>
            <h5>Rejected: {this.props.requests.filter(r => r.rejected).length}</h5>
            <div className="list-group">
                {this.props.requests.filter(r => r.rejected).map(request => this.renderRequest(request))}
            </div>
        </div>);
    }

    renderRequest(request) {
        return (<RequestOverview key={request.userId + ':' + request.teamId} request={request} teams={this.props.teams} onRequestChanged={this.requestChanged} onRequestDeleted={this.requestChanged} />);
    }
}
