import React, {Component} from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Access} from '../api/access';
import {Alert} from "./Alert";
import {EditAccess} from "./EditAccess";
import {MyAccess} from "./MyAccess";
import {RequestAccess} from "./RequestAccess";
import {Functions} from "../functions";

/*
* Props:
* - access
* - request
* - teams
* - updateAccess()
* - reloadAccess()
*
* Events:
* -none-
* */
// noinspection JSUnresolvedVariable
export class Home extends Component {
    constructor(props) {
        super(props);
        this.state = {
            loading: true,
            mode: props.match.params.mode || 'access',
            recovery: {adminPassCode: ''}
        };
        this.recoverAccess = this.recoverAccess.bind(this);
        this.recoveryChanged = this.recoveryChanged.bind(this);
        this.removeError = this.removeError.bind(this);
        this.changeMode = this.changeMode.bind(this);
        this.accessDeleted = this.accessDeleted.bind(this);
        this.accessChanged = this.accessChanged.bind(this);
        this.requestCreated = this.requestCreated.bind(this);
        let http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.history = props.history;
    }

    // hooks
    async componentDidMount() {
        await this.populateAccessForRecovery();
    }

    //event handlers
    async requestCreated() {
        // noinspection JSUnresolvedFunction
        await this.props.reloadAll();
    }

    async accessDeleted() {
        // noinspection JSUnresolvedFunction
        await this.props.reloadAll();
    }

    async accessChanged(accessUpdate) {
        // noinspection JSUnresolvedFunction
        await this.props.updateAccess(accessUpdate.teamId, accessUpdate.userId, accessUpdate.name, accessUpdate.admin, accessUpdate.manager);
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

    async recoverAccess() {
        if (!this.state.recovery.adminPassCode) {
            alert('You must enter the admin pass code');
            return;
        }

        if (!this.state.recovery.recoveryId) {
            alert('You must select an account to recover');
            return;
        }

        this.setState({loading: true});
        await this.sendAccessRecovery(this.state.recovery);
    }

    recoveryChanged(event) {
        let input = event.target;
        let name = input.name;
        let stateUpdate = {recovery: this.state.recovery};
        stateUpdate.recovery[name] = input.value;
        this.setState(stateUpdate);
    }

    removeError() {
        this.setState({error: null});
    }

    // renderers
    renderNav() {
        return (<ul className="nav nav-tabs">
            <li className="nav-item">
                <a className={`nav-link${this.state.mode === 'access' ? ' active' : ''}`} href={`/home/access`}
                   onClick={this.changeMode}>Access</a>
            </li>
            {this.props.access ? null : (<li className="nav-item">
                <a className={`nav-link${this.state.mode === 'recover' ? ' active' : ''}`} href={`/home/recover`}
                   onClick={this.changeMode}>Recover</a>
            </li>)}
            {this.props.access ? (<li className="nav-item">
                <a className={`nav-link${this.state.mode === 'update' ? ' active' : ''}`} href={`/home/update`}
                   onClick={this.changeMode}>Update</a>
            </li>) : null}
        </ul>);
    }

    renderRecoveryAccounts(recoveryAccounts) {
        let setSelectedAccount = function (event) {
            let item = event.target;
            let id = item.getAttribute('data-id');
            let stateUpdate = {recovery: this.state.recovery};
            stateUpdate.recovery.recoveryId = id;
            this.setState(stateUpdate);
        }.bind(this);

        return recoveryAccounts.map(recoveryAccount => {
            let className = 'list-group-item' + (recoveryAccount.recoveryId === this.state.recovery.recoveryId ? ' active' : '');
            return (<li key={recoveryAccount.recoveryId} className={className} data-id={recoveryAccount.recoveryId}
                        onClick={setSelectedAccount}>{recoveryAccount.recoveryId} {recoveryAccount.name}</li>)
        });
    }

    renderRecoveryOptions(recoveryAccounts) {
        return (<div>
            {this.renderNav()}
            <br/>
            <p>Pick an account to recover</p>
            <div className="input-group mb-3">
                <div className="input-group-prepend">
                    <span className="input-group-text" id="basic-addon3">Admin password</span>
                </div>
                <input type="password" className="form-control" id="basic-url" aria-describedby="basic-addon3"
                       name="adminPassCode" value={this.state.recovery.adminPassCode} onChange={this.recoveryChanged}/>
            </div>

            <p>Select your account</p>
            <ul className="list-group">
                {this.renderRecoveryAccounts(recoveryAccounts)}
            </ul>
            <hr/>
            <button type="button" className="btn btn-primary" onClick={this.recoverAccess}>Recover access</button>
        </div>);
    }

    renderLoading() {
        // show a spinner?
        return (<div className="d-flex justify-content-center">
            <div className="spinner-border" role="status">
                <span className="visually-hidden">Loading...</span>
            </div>
        </div>);
    }

    renderError(error) {
        return (<div>
            <Alert errors={[error]}/>
            <hr/>
            <button type="button" className="btn btn-primary" onClick={this.removeError}>Back</button>
        </div>);
    }

    renderAccessMode() {
        if (this.props.access || this.props.request) {
            return (<div>
                {this.renderNav()}
                <br/>
                <MyAccess {...this.props} />
            </div>);
        }

        return <RequestAccess {...this.props} onRequestCreated={this.requestCreated} />
    }

    renderUpdateMode() {
        return (<div>
            {this.renderNav()}
            <br/>
            <EditAccess {...this.props} onAccessDeleted={this.accessDeleted} onAccessChanged={this.accessChanged} />
        </div>)
    }

    render() {
        try {
            if (this.state.loading) {
                return this.renderLoading();
            } else if (this.state.error) {
                return this.renderError(this.state.error);
            } else if (this.state.mode === 'access') {
                return this.renderAccessMode();
            } else if (this.state.mode === 'recover') {
                return this.renderRecoveryOptions(this.state.recoveryAccounts);
            } else if (this.state.mode === 'update') {
                return this.renderUpdateMode();
            }

            return (<div>Unset: {this.state.mode}</div>);
        } catch (e) {
            console.error(e);
            return (<Alert errors={[`Error rendering component: ${e.message}`]}/>);
        }
    }

    // api access
    async populateAccessForRecovery() {
        try {
            const recoveryAccounts = await this.accessApi.getAccessForRecovery();
            recoveryAccounts.sort(Functions.recoverySortFunction);

            this.setState({
                loading: false,
                recoveryAccounts: recoveryAccounts
            });
        } catch (e) {
            console.error(e);
            this.setState({mode: 'access', error: e.message, loading: false});
        }
    }

    async sendAccessRecovery(recovery) {
        this.setState({error: null, loading: true});
        try {
            const data = await this.accessApi.recoverAccess(recovery.recoveryId, recovery.adminPassCode);
            if (data.errors && data.errors.length > 0) {
                this.setState({error: data.errors, loading: false});
                return;
            }

            if (this.props.updateAccess) {
                this.props.updateAccess();
            }
        } catch (e) {
            console.error(e);
            this.setState({error: e.message, loading: false});
        }
    }
}
