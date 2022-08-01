import React, {Component} from 'react';
import {Alert} from "./Alert";
import { Link } from "react-router-dom";

/*
* Props:
* - access
* - request
* - teams
*
* Events:
* -none-
* */
// noinspection JSUnresolvedVariable
export class MyAccess extends Component {
    // renderers
    renderAccess() {
        let team = this.props.teams.filter(t => t.id === this.props.access.teamId)[0];
        // access granted
        return (<div>
            Hello <strong>{this.props.access.name}</strong>, you have access to <strong><span><strong>{team.name}</strong> (Coach {team.coach})</span></strong>
            <br/>
            <Link to={`/team/${team.id}/view`} className="btn btn-primary">View Games</Link>
        </div>);
    }

    renderAccessRejected() {
        return (<div>
            <h1>🚫 Hi {this.props.request.name}</h1>
            <p>Sorry, your access request was rejected.</p>
            <p>Reason: <b>{this.props.request.reason ? this.props.request.reason : 'No reason given'}</b></p>
        </div>);
    }

    renderAccessPending() {
        return (<div>
            <h1>⏳ Hi {this.props.request.name}</h1>
            <p>Your access request hasn't been approved, yet...</p>
        </div>);
    }

    render() {
        try {
            if (this.props.access) {
                return this.renderAccess();
            }

            if (this.props.request) {
                if (this.props.request.rejected) {
                    return this.renderAccessRejected();
                }

                return this.renderAccessPending();
            }

            return (<Alert errors={[ 'Component shouldn\'t have been loaded, user need to request access' ]} />);
        } catch (e) {
            console.error(e);
            return (<Alert errors={[`Error rendering component: ${e.message}`]}/>);
        }
    }
}
