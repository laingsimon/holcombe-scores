import React, { Component } from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {Team} from '../api/team';
import {Alert} from "./Alert";

export class EditTeam extends Component {
    constructor (props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.teamApi = new Team(http);
        this.state = {
            loading: true,
            current: null, // the current team details
            proposed: null // the updated team details
        };
        this.valueChanged = this.valueChanged.bind(this);
        this.updateTeam = this.updateTeam.bind(this);
    }

    componentDidMount() {
        // noinspection JSIgnoredPromiseFromCall
        this.getTeamDetails();
    }

    // event handlers
    valueChanged(event) {
        const name = event.target.name;
        const value = event.target.value;
        const newProposed = Object.assign({}, this.state.proposed);
        newProposed[name] = value;

        this.setState({
            proposed: newProposed
        });
    }

    async updateTeam() {
        this.setState({
            loading: true,
            updateResult: null,
        });

        try {
            const proposed = this.state.proposed;
            const result = this.props.teamId
                ? await this.teamApi.updateTeam(this.props.teamId, proposed.name, proposed.coach)
                : await this.teamApi.createTeam(proposed.name, proposed.coach);

            this.setState({
                loading: false,
                updateResult: result,
            });

            if (result.success) {
                this.setState({
                    current: result.outcome
                });

                if (this.props.onChanged) {
                    this.props.onChanged(result.outcome.id);
                }
            }
        } catch (e) {
            console.error(e);
            this.setState({
                loading: false,
                error: e.message
            });
        }
    }

    // renders
    renderUpdateResult(result) {
        if (!result) {
            return;
        }

        if (result.success) {
            return (<Alert messages={result.messages} />);
        }

        return (<div>
            <Alert messages={result.messages} warnings={result.warnings} errors={result.errors} />
        </div>);
    }

    render () {
        if (this.state.loading) {
            return (<div className="d-flex justify-content-center">
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>);
        }

        if (this.state.error) {
            return (<Alert errors={[ this.state.error ]} />);
        }

        return (<div>
            {this.renderUpdateResult(this.state.updateResult)}
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Name</span>
                </div>
                <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name" value={this.state.proposed.name} onChange={this.valueChanged} />
            </div>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Coach</span>
                </div>
                <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="coach" value={this.state.proposed.coach} onChange={this.valueChanged} />
            </div>
            <hr />
            <button type="button" className="btn btn-primary" onClick={this.updateTeam}>{this.props.teamId ? 'Update team' : 'Create team'}</button>
        </div>);
    }

    async getTeamDetails() {
        const team = this.props.teamId
            ? await this.teamApi.getTeam(this.props.teamId)
            : null;
        this.setState({
            loading: false,
            current: team,
            proposed: team || { name: '', coach: '' }
        });
    }
}
