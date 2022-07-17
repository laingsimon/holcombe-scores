import React, { Component } from 'react';

export class TeamOverview extends Component {
    constructor(props) {
        super(props);
        this.team = props.team;
        this.history = props.history;
    }

    // renderers
    render() {
        return (<a href={`/games/${this.team.id}`} className="list-group-item list-group-item-action flex-column align-items-start" onClick={this.showGames}>
            <strong>{this.team.name}</strong> coached by {this.team.coach}
        </a>);
    }
}
