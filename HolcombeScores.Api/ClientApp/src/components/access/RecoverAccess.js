import React, {Component} from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Access} from '../../api/access';
import {Alert} from '../Alert';
import {Functions} from '../../functions';

/*
* Props:
* -none-
*
* Events:
* - onRecoverySuccess()
* */
// noinspection JSUnresolvedVariable
export class RecoverAccess extends Component {
    constructor(props) {
        super(props);
        this.state = {
            loading: true,
            recovery: {adminPassCode: ''}
        };
        this.recoverAccess = this.recoverAccess.bind(this);
        this.recoveryChanged = this.recoveryChanged.bind(this);
        this.removeError = this.removeError.bind(this);
        let http = new Http(new Settings());
        this.accessApi = new Access(http);
    }

    // hooks
    async componentDidMount() {
        await this.populateAccessForRecovery();
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
    renderLoading() {
        // show a spinner?
        return (<div className="d-flex justify-content-center">
            <div className="spinner-border" role="status">
                <span className="visually-hidden">Loading...</span>
            </div>
        </div>);
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

    renderError(error) {
        return (<div>
            <Alert errors={[error]}/>
            <hr/>
            <button type="button" className="btn btn-primary" onClick={this.removeError}>Back</button>
        </div>);
    }

    render() {
        try {
            if (this.state.loading) {
                return this.renderLoading();
            }

            if (this.state.error) {
                return this.renderError(this.state.error);
            }

            return (<div>
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
                    {this.renderRecoveryAccounts(this.state.recoveryAccounts)}
                </ul>
                <hr/>
                <button type="button" className="btn btn-primary" onClick={this.recoverAccess}>Recover access</button>
            </div>);
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

            if (this.props.onRecoverySuccess) {
                await this.props.onRecoverySuccess();
            }
        } catch (e) {
            console.error(e);
            this.setState({error: e.message, loading: false});
        }
    }
}
