import React, {Component} from 'react';
import {Alert} from '../Alert';
import {EditAccess} from './EditAccess';
import {RequestAccess} from './RequestAccess';
import {RecoverAccess} from './RecoverAccess';

/*
* Props:
* - reloadAll()
* - reloadAccess()
* - access
* - requests
*
* Events:
* -none-
* */
// noinspection JSUnresolvedVariable
export class Home extends Component {
    constructor(props) {
        super(props);
        this.state = {
            mode: props.match.params.mode || 'access'
        };
        this.removeError = this.removeError.bind(this);
        this.changeMode = this.changeMode.bind(this);
        this.accessDeleted = this.accessDeleted.bind(this);
        this.accessChanged = this.accessChanged.bind(this);
        this.requestCreated = this.requestCreated.bind(this);
        this.accessRecovered = this.accessRecovered.bind(this);
        this.loggedOut = this.loggedOut.bind(this);
        this.history = props.history;
    }

    //event handlers
    async loggedOut() {
        // noinspection JSUnresolvedFunction
        await this.props.reloadAll();
        this.setState({
            mode: 'access'
        });
    }

    async requestCreated() {
        // noinspection JSUnresolvedFunction
        await this.props.reloadAll();
    }

    async accessDeleted() {
        // noinspection JSUnresolvedFunction
        await this.props.reloadAll();
        this.setState({
            mode: 'access'
        });
    }

    async accessChanged() {
        await this.props.reloadAccess();
    }

    async accessRecovered() {
        await this.props.reloadAccess();
        window.history.replaceState(null, 'Access', '/home/access');
        this.setState({
            mode: 'access'
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
        </ul>);
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
            let component = <Alert errors={[ 'Unset' ]} />

            if (this.state.error) {
                component = this.renderError(this.state.error);
            } else if (this.state.mode === 'access' || (this.state.mode === 'recover' && this.props.access)) {
                if (this.props.access || this.props.requests.length) {
                    component = (<EditAccess {...this.props} onAccessDeleted={this.accessDeleted} onLoggedOut={this.loggedOut} onAccessChanged={this.accessChanged} />);
                } else {
                    component = (<RequestAccess {...this.props} onRequestCreated={this.requestCreated}/>);
                }
            } else if (this.state.mode === 'recover') {
                component = (<RecoverAccess {...this.props} onRecoverySuccess={this.accessRecovered} />);
            }

            return (<div>
                {this.renderNav()}
                <br/>
                {component}
            </div>)
        } catch (e) {
            console.error(e);
            return (<Alert errors={[`Error rendering component: ${e.message}`]}/>);
        }
    }
}
