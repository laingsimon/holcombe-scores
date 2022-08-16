import React, { Component } from 'react';
import {Http} from '../../api/http';
import {Settings} from '../../api/settings';
import {Access} from '../../api/access';
import {Functions} from '../../functions';

/*
* Props:
* - request
* - teams
*
* Events:
* - onRequestChanged(userId)
* - onRequestDeleted(userId)
* */
export class RequestOverview extends Component {
    constructor (props) {
        super(props);
        const http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.state = {
            processing: false,
            mode: 'view',
            reason: ''
        }
        this.respondRequest = this.respondRequest.bind(this);
        this.deleteRequest = this.deleteRequest.bind(this);
        this.prepareRejectRequest = this.prepareRejectRequest.bind(this);
        this.rejectRequest = this.rejectRequest.bind(this);
        this.approveRequest = this.approveRequest.bind(this);
        this.reasonChanged = this.reasonChanged.bind(this);
    }

    //event handlers
    reasonChanged(event) {
        this.setState({
            reason: event.target.value
        });
    }

    prepareRejectRequest() {
        if (this.state.mode === 'reject') {
            this.setState({
                mode: 'view'
            });
            return;
        }

        this.setState({
            mode: 'reject'
        });
    }

    async deleteRequest() {
        if (!window.confirm('Are you sure you want to DELETE this request')) {
            return;
        }

        this.setState({
            processing: true
        });

        const result = await this.accessApi.deleteAccessRequest(this.props.request.userId);
        if (result.success) {
            if (this.props.onRequestDeleted) {
                await this.props.onRequestDeleted(this.props.request.userId);
            }
        } else {
            alert(`Could not delete request: ${Functions.getResultMessages(result)}`);
            this.setState({
                processing: false
            });
        }
    }

    async approveRequest() {
        await this.respondRequest(true, null);
    }

    async rejectRequest() {
        if (!window.confirm(`Are you sure you want to reject ${this.props.request.name}?`)) {
            return;
        }

        await this.respondRequest(false, this.state.reason);
    }

    render() {
        const requestedDate = new Date(this.props.request.requested);
        const team = this.props.teams[this.props.request.teamId];
        const btnClassName = this.state.processing
            ? 'btn-light'
            : this.state.mode === 'view'
                ? 'btn-primary'
                : 'btn-warning';
        const respondRequestButton = this.state.processing
            ? (<span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>)
            : (<button type="button" className={`btn ${btnClassName}`} onClick={this.prepareRejectRequest}>❓</button>);

        return (<div className="list-group-item list-group-item-action flex-column align-items-start">
            <span>Name: <strong>{this.props.request.name}</strong>, Team: {team.name}, Requested: {requestedDate.toLocaleString()}</span>
            <span className="float-end">
                {this.state.processing || !this.props.request.rejected ? null : (<button type="button" className={`btn ${this.state.processing ? 'btn-light' : 'btn-danger'} margin-right`} onClick={this.deleteRequest}>🗑</button>)}
                {this.props.request.rejected ? null : respondRequestButton}
            </span>
            {this.state.mode === 'reject' && !this.state.processing ? this.renderRejectOptions() : null}
            {this.props.request.rejected && this.props.request.reason ? (<p>Reason: <strong>{this.props.request.reason}</strong></p>) : null}
        </div>);
    }

    renderRejectOptions() {
        return (<div>
            <hr/>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Reason</span>
                </div>
                <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3"
                       name="reason" value={this.state.reason} onChange={this.reasonChanged}/>
                <button type="button"
                        className={`btn ${this.state.processing || this.self ? 'btn-light' : 'btn-danger'} margin-right`}
                        onClick={this.rejectRequest}>🚷 Reject
                </button>
                <button type="button"
                        className={`btn ${this.state.processing || this.self ? 'btn-light' : 'btn-success'}`}
                        onClick={this.approveRequest}>❤ Approve
                </button>
            </div>
        </div>);
    }

    async respondRequest(allow, reason) {
        this.setState({
            processing: true
        });

        const result = await this.accessApi.respondToAccessRequest(this.props.request.userId, this.props.request.teamId, reason, allow);
        if (result.success) {
            if (this.props.onRequestChanged) {
                await this.props.onRequestChanged(this.props.request.userId);
            }
        } else {
            alert(`Could not respond to request: ${Functions.getResultMessages(result)}`);
        }

        this.setState({
            processing: false
        });
    }
}