import React, {Component} from 'react';
import {Alert} from "./Alert";
import {EditAccess} from "./EditAccess";
import {MyAccess} from "./MyAccess";
import {RequestAccess} from "./RequestAccess";
import {RecoverAccess} from "./RecoverAccess";

/*
* Props:
* - reloadAll()
* - updateAccess()
* - access
* - request
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
        this.history = props.history;
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
            } else if (this.state.mode === 'access') {
                if (this.props.access || this.props.request) {
                    component = (<MyAccess {...this.props} />);
                } else {
                    component = (<RequestAccess {...this.props} onRequestCreated={this.requestCreated}/>);
                }
            } else if (this.state.mode === 'recover') {
                component = (<RecoverAccess {...this.props} />);
            } else if (this.state.mode === 'update') {
                component = (<EditAccess {...this.props} onAccessDeleted={this.accessDeleted} onAccessChanged={this.accessChanged} />);
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
