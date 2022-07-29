import React, { Component } from 'react';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Access} from "../api/access";
import {Functions} from "../functions";

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
        this.request = props.request;
        this.teams = props.teams;
        this.respondRequest = this.respondRequest.bind(this);
        this.deleteRequest = this.deleteRequest.bind(this);
        this.prepareRejectRequest = this.prepareRejectRequest.bind(this);
        this.rejectRequest = this.rejectRequest.bind(this);
        this.approveRequest = this.approveRequest.bind(this);
        this.reasonChanged = this.reasonChanged.bind(this);
    }

    //events
    requestChanged() {
        if (this.props.onRequestChanged) {
            this.props.onRequestChanged(this.request.userId);
        }
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

        const result = await this.accessApi.deleteAccessRequest(this.request.userId);
        if (result.success) {
            this.requestChanged();
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
        if (!window.confirm(`Are you sure you want to reject ${this.request.name}?`)) {
            return;
        }

        await this.respondRequest(false, this.state.reason);
    }

    render() {
        const requestedDate = new Date(this.request.requested);
        const team = this.teams[this.request.teamId];
        const btnClassName = this.state.processing
            ? 'btn-light'
            : this.state.mode === 'view'
                ? 'btn-primary'
                : 'btn-warning';
        const respondRequestButton = this.state.processing
            ? (<span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>)
            : (<button type="button" className={`btn ${btnClassName}`} onClick={this.prepareRejectRequest}>❓</button>);

        return (<div className="list-group-item list-group-item-action flex-column align-items-start">
            <span>Name: <strong>{this.request.name}</strong>, Team: {team.name}, Requested: {requestedDate.toLocaleString()}</span>
            <span className="float-end">
                {this.state.processing || !this.request.rejected ? null : (<button type="button" className={`btn ${this.state.processing ? 'btn-light' : 'btn-danger'}`} onClick={this.deleteRequest}>🗑</button>)}
                &nbsp;
                {this.request.rejected ? null : respondRequestButton}
            </span>
            {this.state.mode === 'reject' && !this.state.processing ? this.renderRejectOptions() : null}
            {this.request.rejected && this.request.reason ? (<p>Reason: <strong>{this.request.reason}</strong></p>) : null}
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
                        className={`btn ${this.state.processing || this.self ? 'btn-light' : 'btn-danger'}`}
                        onClick={this.rejectRequest}>🚷 Reject
                </button>
                &nbsp;
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

        const result = await this.accessApi.respondToAccessRequest(this.request.userId, this.request.teamId, reason, allow);
        if (result.success) {
            this.requestChanged();
        } else {
            alert(`Could not respond to request: ${Functions.getResultMessages(result)}`);
            this.setState({
                processing: false
            });
        }
    }
}