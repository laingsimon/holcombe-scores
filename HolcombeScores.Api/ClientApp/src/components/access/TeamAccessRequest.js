﻿import React, {Component} from 'react';
import {Alert} from '../Alert';
import { Link } from 'react-router-dom';

/*
* Props:
* - team
* - selected
* - requested
* - approved
* - rejected
* - rejectedReason
*
* Events:
* - onSelected(teamId, selected)
* */
// noinspection JSUnresolvedVariable
export class TeamAccessRequest extends Component {
    constructor(props) {
        super(props);
        this.setSelectedTeam = this.setSelectedTeam.bind(this);
    }

    //event handlers
    setSelectedTeam(event) {
        let item = event.target;
        while (item && item.tagName !== 'LI') {
            item = item.parentElement;
        }

        if (!item) {
            return;
        }

        if (this.props.onSelected) {
            this.props.onSelected(this.props.team.id, !this.props.selected);
        }
    }

    // renderers
    render() {
        try {
            return (<li className={`list-group-item flex-column align-items-start ${this.accessColour()}`} onClick={this.setSelectedTeam}>
                <div className="d-flex justify-content-between">
                    {this.props.team.name}
                    {this.props.selected && this.props.approved ? (<Link className="btn btn-primary" to={`/team/${this.props.team.id}`}>View games...</Link>) : null}
                    <div className="d-flex align-items-center">{this.accessLabel()}</div>
                </div>
            </li>)
        } catch (e) {
            console.error(e);
            return (<Alert errors={[`Error rendering component: ${e.message}`]}/>);
        }
    }

    accessColour() {
        if (!this.props.selected) {
            return '';
        }

        if (this.props.approved) {
            return 'list-group-item-success';
        }

        if (this.props.rejected) {
            return 'list-group-item-danger';
        }

        if (this.props.requested) {
            return 'list-group-item-warning';
        }

        return 'list-group-item-primary';
    }

    accessLabel() {
        if (!this.props.selected) {
            if (this.props.approved) {
                return (<span className="badge rounded-pill bg-info">To remove</span>);
            }
            if (this.props.requested) {
                return (<span className="badge rounded-pill bg-info">To cancel request</span>);
            }

            return null;
        }

        if (this.props.approved) {
            return (<span className="badge rounded-pill bg-success">Approved</span>);
        }
        if (this.props.rejected) {
            return (<span className="badge rounded-pill bg-danger">{this.props.rejectedReason ? `Rejected: ${this.props.rejectedReason}` : 'Rejected'}</span>);
        }
        if (this.props.requested) {
            return (<span className="badge rounded-pill bg-warning">Requested</span>);
        }

        return (<span className="badge rounded-pill bg-info">To request</span>);
    }
}
