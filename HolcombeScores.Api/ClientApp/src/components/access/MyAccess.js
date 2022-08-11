import React, {Component} from 'react';
import {Http} from '../../api/http';
import {Settings} from '../../api/settings';
import {Access} from '../../api/access';
import {Alert} from '../Alert';
import { Link } from 'react-router-dom';

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
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.state = {
            navigating: false
        };
        this.beforeNavigate = this.beforeNavigate.bind(this);
        this.unimpersonate = this.unimpersonate.bind(this);
    }

    async unimpersonate() {
        if (!window.confirm('Are you sure you want to unimpersonate?')) {
            return;
        }

        await this.accessApi.unimpersonate();
        await this.props.reloadAll();
    }

    async beforeNavigate(event) {
        event.preventDefault();
        this.setState({
            navigating: true
        });

        const reloadTeam = false;
        const reloadPlayers = true;
        const reloadGames = true;
        await this.props.reloadTeam(this.props.access.teamId, reloadTeam, reloadPlayers, reloadGames);

        this.setState({
            navigating: false
        });

        this.props.history.push(event.target.getAttribute('href'));
    }

    // renderers
    renderAccess() {
        let team = this.props.teams.filter(t => t.id === this.props.access.teamId)[0];
        // access granted
        return (<div>
            Hello <strong>{this.props.access.name}</strong>, you have access to <strong><span><strong>{team.name}</strong> (Coach {team.coach})</span></strong>
            <br/>
            <Link onClick={this.beforeNavigate} to={`/team/${team.id}/view`} className="btn btn-primary margin-right">
                {this.state.navigating ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                View Games
            </Link>
            {this.props.isImpersonated ? (<Button className="btn btn-secondary" onClick={this.unimpersonate}>
                Unimpersonate
            </Button>) : null}
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
