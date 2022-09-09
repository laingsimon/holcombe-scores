import React, { Component } from 'react';
import {Http} from '../../api/http';
import {Settings} from '../../api/settings';
import {Access} from '../../api/access';
import {Alert} from '../Alert';
import {AccessAdmin} from './AccessAdmin';
import {RequestAdmin} from './RequestAdmin';
import {TestingAdmin} from './TestingAdmin';

/*
* Props:
* - access
* - teams
* - reloadAll()
*
* Events:
* -none-
*/
export class Admin extends Component {
    constructor (props) {
        super(props);
        const http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.state = {
            loading: true,
            error: null,
            mode: props.match.params.mode || 'requests',
            requests: null,
            allAccess: null,
        };
        this.changeMode = this.changeMode.bind(this);
        this.accessChanged = this.accessChanged.bind(this);
        this.requestChanged = this.requestChanged.bind(this);
        this.accessImpersonated = this.accessImpersonated.bind(this);
    }

    //event handlers
    async accessImpersonated() {
        this.setState({
            loading: true
        });

        await this.props.reloadAll();

        this.setState({
            allAccess: await this.getAllAccess(true),
            loading: false
        });
    }

    async accessChanged() {
        this.setState({
            allAccess: await this.getAllAccess(true),
        });
    }

    async requestChanged() {
        this.setState({
            requests: await this.getAccessRequests(),
            allAccess: await this.getAllAccess(),
        });
    }

    changeMode(event) {
        event.preventDefault();
        const url = event.target.getAttribute('href');
        const segments = url.split('/')
        const mode = segments[segments.length - 1];
        this.setState({
            mode: mode,
        });
        window.history.replaceState(null, event.target.textContent, url);
    }

    async componentDidMount() {
        await this.requestData();
    }

    // renderers
    renderNav() {
        return (<ul className="nav nav-tabs">
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'requests' ? ' active' : ''}`} href={`/admin/requests`} onClick={this.changeMode}>Requests</a>
            </li>
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'access' ? ' active' : ''}`} href={`/admin/access`} onClick={this.changeMode}>Access</a>
            </li>
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'testing' ? ' active' : ''}`} href={`/admin/testing`} onClick={this.changeMode}>Testing</a>
            </li>
        </ul>);
    }

    render() {
        if (this.state.loading) {
            return (<div className="d-flex justify-content-center">
                <div className="spinner-border spinner-football" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>);
        }

        if (this.state.error) {
            return (<div>
                {this.renderNav()}
                <br />
                <Alert errors={[ this.state.error ]} />
            </div>);
        }

        if (!this.props.access.admin && !this.props.access.manager) {
            return (<Alert warnings={[ 'This service is only available for managers and administrators.' ]} />);
        }

        let component = (<div>Unknown mode {this.state.mode}</div>)

        if (this.state.mode === 'requests') {
            component = (<RequestAdmin requests={this.state.requests} teams={this.props.teams} onRequestChanged={this.requestChanged} />);
        } else if (this.state.mode === 'access') {
            component = (<AccessAdmin allAccess={this.state.allAccess} myAccess={this.props.access} teams={this.props.teams} onAccessImpersonated={this.accessImpersonated} onAccessChanged={this.accessChanged} isImpersonated={this.props.isImpersonated} />);
        } else if (this.state.mode === 'testing') {
            component = (<TestingAdmin {...this.props} />);
        }

        return (<div>
            {this.renderNav()}
            <br />
            {component}
        </div>);
    }

    //api
    async getAccessRequests() {
        const requests = await this.accessApi.getAllAccessRequests();
        requests.sort((a, b) => Date.parse(b.requested) - Date.parse(a.requested));

        return requests;
    }

    async getAllAccess() {
        const allAccess = await this.accessApi.getAllAccess();
        allAccess.sort((a, b) => a.name - b.name);

        return allAccess;
    }

    async requestData() {
        try {
            this.setState({
                loading: false,
                requests: await this.getAccessRequests(),
                allAccess: await this.getAllAccess()
            });
        } catch (e) {
            console.error(e);
            this.setState({
                error: e,
                loading: false
            });
        }
    }
}
