import React, { Component } from 'react';

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
        return (<a href={`/team/${this.props.team.id}`} className="list-group-item list-group-item-action flex-column align-items-start">
            <strong>{this.props.team.name}</strong> coached by {this.props.team.coach}
        </a>);
    }
}
