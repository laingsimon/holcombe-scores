import React, {Component} from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Access} from '../../api/access';
import {Alert} from '../Alert';
import { Link } from 'react-router-dom';

/*
* Props:
* - access
* - updateAccess()
*
* Events:
* - onAccessDeleted(userId)
* - onAccessChanged(access)
* - onAccessRequested()
* */
// noinspection JSUnresolvedVariable
export class EditAccess extends Component {
    constructor(props) {
        super(props);
        this.state = {
            loading: false,
            proposed: Object.assign({ name: '' }, this.props.access, { teams: this.getProposedTeamIds(this.props.access, this.props.requests) })
        };
        this.updateAccess = this.updateAccess.bind(this);
        this.accessChanged = this.accessChanged.bind(this);
        this.removeAccess = this.removeAccess.bind(this);
        this.logout = this.logout.bind(this);
        let http = new Http(new Settings());
        this.accessApi = new Access(http);
    }

    getProposedTeamIds(access, requests) {
        const teamIds = access ? access.teams.filter(_ => true) : [];

        if (requests) {
            for (const request of requests) {
                teamIds.push(request.teamId);
            }
        }

        return teamIds;
    }

    //event handlers
    async logout() {
        if (!window.confirm('Are you sure you want to logout?')) {
            return;
        }

        this.setState({loggingOut: true});

        const result = await this.accessApi.logout();

        if (result.success) {
            if (this.props.onLoggedOut) {
                await this.props.onLoggedOut(this.props.access.userId);
            }
        } else {
            this.setState({loggingOut: false});
            alert('Could not logout');
        }
    }

    async removeAccess() {
        if (!window.confirm('Are you sure you want to remove your access')) {
            return;
        }

        this.setState({deleting: true});

        const result = await this.accessApi.deleteAccess(this.props.access.userId);

        if (result.success) {
            if (this.props.onAccessDeleted) {
                await this.props.onAccessDeleted(this.props.access.userId);
            }
        } else {
            this.setState({deleting: false});
            alert('Could not delete your details');
        }
    }

    async updateAccess() {
        if (!this.state.proposed.name) {
            alert('You need to enter a name');
            return;
        }

        const currentAccessCopy = Object.assign({}, this.props.access);
        const accessUpdate = Object.assign(currentAccessCopy, this.state.proposed);

        this.setState({updating: true});

        if (this.props.access) {
            // update access
            const result = await this.accessApi.updateAccess(accessUpdate.teamId, accessUpdate.userId, accessUpdate.name, accessUpdate.admin, accessUpdate.manager);

            if (result.success) {
                const success = await this.updateAccessRequests();

                if (success) {
                    if (this.props.onAccessChanged) {
                        this.props.onAccessChanged(accessUpdate);
                    }
                } else {
                    alert('Error updating access requests for one or more teams');
                }

                this.setState({updating: false});
            } else {
                this.setState({updating: false});
                alert('Could not update your access');
            }
        } else {
            const success = await this.createAccessRequests();

            if (success) {
                if (this.props.onAccessRequested) {
                    await this.props.onAccessRequested();
                }
            } else {
                alert('Error requesting access for one or more teams');
            }

            this.setState({updating: false});
        }
    }

    async updateAccessRequests() {
        let success = true;

        for (const team of this.props.teams) {
            const selected = this.state.proposed.teams.filter(tid => tid === team.id).length > 0;
            const matchingRequests = this.props.requests && this.props.requests.filter(r => r.teamId === team.id);
            const wasApproved = this.props.access.teams.filter(id => id === team.id).length;
            const wasRequested = matchingRequests.length;

            try {
                if (selected) {
                    if (!wasRequested && !wasApproved) {
                        // create request access
                        await this.accessApi.createAccessRequest(this.state.proposed.name, team.id);
                    }
                } else {
                    if (wasApproved) {
                        // delete access
                        await this.accessApi.deleteAccess(this.props.access.userId, team.id);
                    }
                    if (wasRequested) {
                        // delete access request
                        await this.accessApi.deleteAccessRequest(this.props.access.userId, team.id);
                    }
                }
            } catch (e) {
                console.error(`Error updating access request: ${team.id}/${team.name}`, e);
                success = false;
            }
        }

        return success;
    }

    async createAccessRequests() {
        let success = true;

        for (const team of this.props.teams) {
            const selected = this.state.proposed.teams.filter(tid => tid === team.id).length > 0;
            if (selected) {
                try {
                    await this.accessApi.createAccessRequest(this.state.proposed.name, team.id);
                } catch (e) {
                    console.error(`Error creating access request: ${team.id}/${team.name}`, e);
                    success = false;
                }
            }
        }

        return success;
    }

    accessChanged(event) {
        if (this.state.updating) {
            return;
        }

        const name = event.target.getAttribute('name');
        const value = event.target.value;
        const proposed = Object.assign({}, this.state.proposed);
        proposed[name] = value;

        this.setState({
            proposed: proposed
        });
    }

    render() {
        try {
            return (<div>
                <div className="input-group mb-3">
                    <div className="input-group-prepend">
                        <span className="input-group-text" id="basic-addon3">Your name</span>
                    </div>
                    <input readOnly={this.state.updating || this.state.deleting || this.state.loggingOut} type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3" name="name"
                           value={this.state.proposed.name} onChange={this.accessChanged}/>
                </div>
                <h4>Requested teams</h4>
                <ul className="list-group">
                    {this.renderTeams(this.props.teams)}
                </ul>
                <hr />
                <button type="button" className="btn btn-primary margin-right" onClick={this.updateAccess}>
                    {this.state.updating ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                    {this.props.access ? 'Update details' : 'Request access'}
                </button>
                {this.props.access ? (<button type="button" className="btn btn-danger margin-right" onClick={this.removeAccess}>
                    {this.state.deleting ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                    Remove access
                </button>) : null}
                {this.props.access ? (<button type="button" className="btn btn-warning" onClick={this.logout}>
                    {this.state.loggingOut ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                    Logout
                </button>) : null}
            </div>)
        } catch (e) {
            console.error(e);
            return (<Alert errors={[`Error rendering component: ${e.message}`]}/>);
        }
    }

    renderTeams(teams) {
        const setSelectedTeam = function (event) {
            if (this.state.updating) {
                return;
            }

            let item = event.target;
            while (item && item.tagName !== 'LI') {
                item = item.parentElement;
            }

            if (!item) {
                return;
            }

            let id = item.getAttribute('data-id');
            let wasSelected = this.state.proposed.teams.filter(tid => tid === id).length > 0;
            let newTeams = this.state.proposed.teams.filter(tid => tid !== id);
            if (!wasSelected) {
                newTeams.push(id);
            }

            const proposed = Object.assign({}, this.state.proposed);
            proposed.teams = newTeams;

            this.setState({
                proposed: proposed
            });
        }.bind(this);

        return teams.map(team => {
            const selected = this.state.proposed.teams.filter(tid => tid === team.id).length > 0;
            return (<li key={team.id} className={`list-group-item flex-column align-items-start ${this.accessColour(team.id, selected)} : ''}`} data-id={team.id}
                        onClick={setSelectedTeam}>
                <div className="d-flex justify-content-between">
                    {team.name}
                    {selected && this.props.access && this.props.access.teams.filter(tid => tid === team.id).length > 0 ? (<Link className="btn btn-primary" to={`/team/${team.id}`}>View games...</Link>) : null}
                    <div className="d-flex align-items-center">{this.accessLabel(team.id, selected)}</div>
                </div>
            </li>)
        });
    }

    accessColour(teamId, selected) {
        if (!selected) {
            return '';
        }

        if (this.props.access && this.props.access.teams.filter(tid => tid === teamId).length > 0) {
            return 'list-group-item-success';
        }

        if (this.props.requests) {
            const matchingRequests = this.props.requests.filter(r => r.teamId === teamId);
            const request = matchingRequests.length ? matchingRequests[0] : null;
            if (request != null) {
                if (request.rejected) {
                    return 'list-group-item-danger';
                }

                return 'list-group-item-warning';
            }
        }

        return 'list-group-item-primary';
    }

    accessLabel(teamId, selected) {
        if (!selected) {
            if (this.props.access && this.props.access.teams.filter(tid => tid === teamId).length > 0) {
                return (<span className="badge rounded-pill bg-info">To remove</span>);
            }
            if (this.props.requests && this.props.requests.filter(r => r.teamId === teamId).length > 0) {
                return (<span className="badge rounded-pill bg-info">To cancel request</span>);
            }

            return null;
        }

        if (this.props.access && this.props.access.teams.filter(tid => tid === teamId).length > 0) {
            return (<span className="badge rounded-pill bg-success">Approved</span>);
        }

        if (this.props.requests) {
            const matchingRequests = this.props.requests.filter(r => r.teamId === teamId);
            const request = matchingRequests.length ? matchingRequests[0] : null;
            if (request != null) {
                if (request.rejected) {
                    return (<span className="badge rounded-pill bg-danger">{request.reason ? `Rejected: ${request.reason}` : 'Rejected'}</span>);
                }
                return (<span className="badge rounded-pill bg-warning">Requested</span>);
            }
        }

        return (<span className="badge rounded-pill bg-info">To request</span>);
    }
}
