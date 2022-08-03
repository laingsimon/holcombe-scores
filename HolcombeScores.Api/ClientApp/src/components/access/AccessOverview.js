import React, { Component } from 'react';
import {Http} from '../../api/http';
import {Settings} from '../../api/settings';
import {Access} from '../../api/access';
import {Team} from '../../api/team';
import {Functions} from '../../functions';

/*
* Props:
* - teams
* - myAccess
* - access
*
* Events:
* - onAccessChanged(userId)
* - onAccessRevoked(userId)
*/
export class AccessOverview extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.teamApi = new Team(http);
        this.state = {
            processing: false,
            access: this.props.access,
            mode: 'view',
            reason: ''
        };

        this.teams = this.props.teams;
        this.myAccess = this.props.myAccess;
        this.userId = this.props.access.userId;
        this.self = this.userId === this.props.myAccess.userId;

        this.cancelAccess = this.cancelAccess.bind(this);
        this.adminChanged = this.adminChanged.bind(this);
        this.managerChanged = this.managerChanged.bind(this);
        this.changeTeam = this.changeTeam.bind(this);
        this.prepareCancelAccess = this.prepareCancelAccess.bind(this);
        this.reasonChanged = this.reasonChanged.bind(this);
    }

    // events
    async accessChanged() {
        if (this.props.onAccessChanged) {
            await this.props.onAccessChanged(this.userId);
        }
    }

    // event handlers
    reasonChanged(event) {
        this.setState({
            reason: event.target.value
        });
    }

    async prepareCancelAccess() {
        if (this.state.mode === 'cancel') {
            this.setState({
                mode: 'view'
            });
            return;
        }

        if (this.self) {
            alert('You cannot cancel your own access');
            return;
        }

        if (this.props.access.admin && !this.myAccess.admin) {
            alert('Only admins can delete other admin acceses');
            return;
        }

        if (this.props.access.revoked)
        {
            if (!window.confirm('Are you sure you want to DELETE this access')) {
                return;
            }

            this.setState({
                processing: true
            });

            const result = await this.accessApi.deleteAccess(this.userId);
            if (result.success) {
                this.setState({
                    processing: false
                });

                if (this.props.onAccessRevoked) {
                    await this.props.onAccessRevoked(this.userId);
                }
            } else {
                alert(`Could not revoke access: ${Functions.getResultMessages(result)}`);
                this.setState({
                    processing: false
                });
            }
        }

        this.setState({
            mode: 'cancel'
        });
    }

    async cancelAccess() {
        if (!window.confirm('Are you sure you want to REVOKE this access')) {
            return;
        }

        this.setState({
            processing: true
        });

        const result = await this.accessApi.revokeAccess(this.userId, this.state.access.teamId, this.state.reason);
        if (result.success) {
            this.setState({
                processing: false
            });

            if (this.props.onAccessRevoked) {
                await this.props.onAccessRevoked(this.userId);
            }
        } else {
            alert(`Could not revoke access: ${Functions.getResultMessages(result)}`);
            this.setState({
                processing: false
            });
        }
    }

    async adminChanged(event) {
        const shouldBeAdmin = event.target.checked;

        if (this.self) {
            alert('You cannot change your own admin access');
            return;
        }

        const access = Object.assign({}, this.state.access);
        access.admin = shouldBeAdmin;
        this.setState({
            processing: true,
            access: access
        });

        const result = await this.accessApi.updateAccess(this.state.access.teamId, this.userId, this.state.access.name, shouldBeAdmin, this.state.access.manager);
        if (result.success) {
            this.setState({
                processing: false
            });

            await this.accessChanged();
        } else {
            alert(`Could not change admin status: ${Functions.getResultMessages(result)}`);
            this.setState({
                processing: false
            });
        }
    }

    async managerChanged(event) {
        const shouldBeManager = event.target.checked;

        if (this.self) {
            alert('You cannot change your own manager access');
            return;
        }

        const access = Object.assign({}, this.state.access);
        access.manager = shouldBeManager;
        this.setState({
            processing: true,
            access: access
        });

        const result = await this.accessApi.updateAccess(this.state.access.teamId, this.userId, this.state.access.name, this.state.access.admin, shouldBeManager);
        if (result.success) {
            this.setState({
                processing: false
            });

            await this.accessChanged();
        } else {
            alert(`Could not change manager status: ${Functions.getResultMessages(result)}`);
            this.setState({
                processing: false
            });
        }
    }

    async changeTeam(event) {
        const teamId = event.target.value;

        const access = Object.assign({}, this.state.access);
        access.teamId = teamId;
        this.setState({
            processing: true,
            access: access
        });

        const result = await this.accessApi.updateAccess(teamId, this.userId, this.state.access.name, this.state.access.admin, this.state.access.manager);
        if (result.success) {
            this.setState({
                processing: false
            });

            await this.accessChanged();
        } else {
            alert(`Could not change team: ${Functions.getResultMessages(result)}`);
            this.setState({
                processing: false
            });
        }
    }

    // renderers
    render() {
        const btnClassName = this.state.processing || this.self
            ? 'btn-light'
            : this.state.mode === 'view'
                ? 'btn-danger'
                : 'btn-warning';

        return (<div key={this.userId} className="list-group-item list-group-item-action flex-column align-items-start">
            <span>
                <strong>{this.state.access.name}</strong>,
                Team: {this.props.access.revoked ? this.teams[this.props.access.teamId].name : (<select value={this.state.access.teamId} onChange={this.changeTeam}>
                {Object.keys(this.teams).map(teamId => <option value={teamId} key={teamId}>{this.teams[teamId].name}</option>)}
                </select>)}
            </span>
            <span className="float-end">
                {this.myAccess.admin && !this.props.access.revoked ? (<span className="form-check form-switch form-check-inline margin-right">
                    <input className="form-check-input" type="checkbox" id="flexSwitchCheckDefault"
                           checked={this.state.access.admin} onChange={this.adminChanged}/>
                    <label className="form-check-label" htmlFor="flexSwitchCheckDefault">Admin</label>
                </span>) : null}
                {!this.props.access.revoked ? (<span className="form-check form-switch form-check-inline margin-right">
                    <input className="form-check-input" type="checkbox" id="flexSwitchCheckDefault"
                           checked={this.state.access.manager} onChange={this.managerChanged}/>
                    <label className="form-check-label" htmlFor="flexSwitchCheckDefault">Manager</label>
                </span>): null}
                <button type="button"
                        className={`btn ${btnClassName}`}
                        onClick={this.prepareCancelAccess}>{this.state.mode === 'view' ? '🗑' : '🔙'}</button>
            </span>
            {this.state.mode === 'cancel' && !this.state.processing ? this.renderCancelOptions() : null}
        </div>);
    }

    renderCancelOptions() {
        return (<div>
            <hr />
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Reason</span>
                </div>
                <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3"
                       name="reason" value={this.state.reason} onChange={this.reasonChanged}/>
                <button type="button"
                        className={`btn ${this.state.processing || this.self ? 'btn-light' : 'btn-danger'}`}
                        onClick={this.cancelAccess}>Cancel Access</button>
            </div>
        </div>);
    }
}