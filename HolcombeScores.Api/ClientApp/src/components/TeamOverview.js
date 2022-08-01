import React, { Component } from 'react';
import { Link } from "react-router-dom";

/*
* Props:
* - team
*
* Events:
* -none-
* */
export class TeamOverview extends Component {
    // renderers
    render() {
        return (<Link to={`/team/${this.props.team.id}`} className="list-group-item list-group-item-action flex-column align-items-start">
            <strong>{this.props.team.name}</strong> coached by {this.props.team.coach}
        </Link>);
    }
}
