import React, {Component} from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Access} from '../api/access';
import {Alert} from "./Alert";

/*
* Props:
* - teams
*
* Events:
* - onRequestCreated()
* */
// noinspection JSUnresolvedVariable
export class RequestAccess extends Component {
    constructor(props) {
        super(props);
        this.state = {
            loading: false,
            request: {name: ''}
        };
        this.requestAccess = this.requestAccess.bind(this);
        this.requestChanged = this.requestChanged.bind(this);
        this.removeError = this.removeError.bind(this);
        let http = new Http(new Settings());
        this.accessApi = new Access(http);
    }

    // event handlers
    removeError() {
        this.setState({error: null});
    }

    async requestAccess() {
        if (!this.state.request.name) {
            alert('You must enter a name');
            return;
        }

        if (!this.state.request.teamId) {
            alert('You must select a team');
            return;
        }

        this.setState({error: null, loading: true});
        try {
            const result = await this.accessApi.createAccessRequest(this.state.request.name, this.state.request.teamId);
            if (result.errors && result.errors.length > 0) {
                this.setState({error: result.errors, loading: false});
                return;
            }

            if (this.props.onRequestCreated) {
                await this.props.onRequestCreated();
            }
        } catch (e) {
            console.error(e);
            this.setState({error: e.message, loading: false});
        }
    }

    requestChanged(event) {
        let input = event.target;
        let name = input.name;
        let stateUpdate = {request: this.state.request};
        stateUpdate.request[name] = input.value;
        this.setState(stateUpdate);
    }

    // renderers
    renderError(error) {
        return (<div>
            <Alert errors={[error]}/>
            <hr/>
            <button type="button" className="btn btn-primary" onClick={this.removeError}>Back</button>
        </div>);
    }

    renderTeams(teams) {
        let setSelectedTeam = function (event) {
            let item = event.target;
            let id = item.getAttribute('data-id');
            let stateUpdate = {request: this.state.request};
            stateUpdate.request.teamId = id;
            this.setState(stateUpdate);
        }.bind(this);

        return teams.map(team => {
            let selected = team.id === this.state.request.teamId;
            return (<li key={team.id} className={`list-group-item ${selected ? ' active' : ''}`} data-id={team.id}
                        onClick={setSelectedTeam}>
                {team.name}
            </li>)
        });
    }

    render() {
        try {
            if (this.state.error) {
                return this.renderError(this.state.error);
            }

            if (!this.props.teams.length) {
                return (<div>
                    <p>You don't currently have access. <strong>There are no teams to request access for</strong>.</p>
                </div>);
            }

            return (<div>
                <p>You don't currently have access, enter your details below to request access</p>
                <div className="input-group mb-3">
                    <div className="input-group-prepend">
                        <span className="input-group-text" id="basic-addon3">Your name</span>
                    </div>
                    <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name"
                           value={this.state.request.name} onChange={this.requestChanged}/>
                </div>

                <p>Select your team</p>
                <ul className="list-group">
                    {this.renderTeams(this.props.teams)}
                </ul>
                <hr/>
                <button type="button" className="btn btn-primary" onClick={this.requestAccess}>Request access</button>
            </div>);
        } catch (e) {
            console.error(e);
            return (<Alert errors={[`Error rendering component: ${e.message}`]}/>);
        }
    }
}
