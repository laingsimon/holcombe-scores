import React, { Component } from 'react';
import {Http} from '../../api/http';
import {Settings} from '../../api/settings';
import {Access} from '../../api/access';
import {AccessOverview} from './AccessOverview';

/*
* Props:
* - myAccess
* - allAccess
* - teams
*
* Events:
* - onAccessImpersonated(impersonating)
* - onAccessChanged()
* -
*/
export class AccessAdmin extends Component {
    constructor (props) {
        super(props);
        const http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.state = {
            unimpersonating: false
        };
        this.accessChanged = this.accessChanged.bind(this);
        this.accessImpersonated = this.accessImpersonated.bind(this);
        this.unimpersonate = this.unimpersonate.bind(this);
    }

    //event handlers
    async accessImpersonated(impersonating) {
        if (this.props.onAccessImpersonated) {
            await this.props.onAccessImpersonated(impersonating);
        }
    }

    async accessChanged() {
        if (this.props.onAccessChanged) {
            await this.props.onAccessChanged();
        }
    }

    async unimpersonate() {
        if (!window.confirm('Are you sure you want to unimpersonate?')) {
            return;
        }

        this.setState({
            unimpersonating: true
        });

        try {
            await this.accessApi.unimpersonate();

            this.setState({
                unimpersonating: false
            });

            this.accessImpersonated(false);
        } catch (e) {
            this.setState({
                unimpersonating: false
            });

            alert(e);
        }
    }

    // renderers
    render() {
        return (<div>
            <h5>Active</h5>
            <div className="list-group">
                {this.props.allAccess.filter(a => !a.revoked).map(access => this.renderAccessOverview(access))}
            </div>
            <h5>Canceled</h5>
            <div className="list-group">
                {this.props.allAccess.filter(a => a.revoked).map(access => this.renderAccessOverview(access))}
            </div>
            {this.props.isImpersonated ? (<button className="btn btn-secondary" onClick={this.unimpersonate}>
                {this.state.unimpersonating ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                Unimpersonate
            </button>) : null}
        </div>);
    }

    renderAccessOverview(access) {
        return (<AccessOverview key={access.userId} onAccessChanged={this.accessChanged} onAccessRevoked={this.accessChanged} access={access} teams={this.props.teams}
                                myAccess={this.props.myAccess} onAccessImpersonated={this.accessImpersonated} />);
    }
}
