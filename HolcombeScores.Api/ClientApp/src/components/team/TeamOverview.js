import React, { Component } from 'react';
import { Link } from 'react-router-dom';

/*
* Props:
* - team
* - reloadTeam(id, reloadTeam, reloadPlayers, reloadGames)
*
* Events:
* -none-
* */
export class TeamOverview extends Component {
    constructor(props) {
        super(props);
        this.state = {
            navigating: false
        }
        this.beforeNavigate = this.beforeNavigate.bind(this);
    }

    async beforeNavigate(event) {
        event.preventDefault();
        this.setState({
            navigating: true
        });

        const reloadTeam = true;
        const reloadPlayers = true;
        const reloadGames = true;
        await this.props.reloadTeam(this.props.team.id, reloadTeam, reloadPlayers, reloadGames);

        this.setState({
            navigating: false
        });

        this.props.history.push(event.target.getAttribute('href'));
    }

    // renderers
    render() {
        return (<Link to={`/team/${this.props.team.id}`} onClick={this.beforeNavigate} className="list-group-item list-group-item-action flex-column align-items-start">
            <strong>{this.props.team.name}</strong> coached by {this.props.team.coach}
            {this.state.navigating ? (<span className="float-end spinner-border spinner-border-sm text-success" role="status" aria-hidden="true"></span>) : null}
        </Link>);
    }
}
