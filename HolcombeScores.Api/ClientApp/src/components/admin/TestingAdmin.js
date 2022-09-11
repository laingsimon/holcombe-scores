import React, {Component} from 'react';
import {Http} from '../../api/http';
import {Settings} from '../../api/settings';
import {Testing} from '../../api/testing';
import {Alert} from "../Alert";

/*
* Props:
* -none
*
* Events:
* -none-
*/
export class TestingAdmin extends Component {
    constructor(props) {
        super(props);
        this.testingApi = new Testing(new Http(new Settings()));
        this.startTesting = this.startTesting.bind(this);
        this.endTesting = this.endTesting.bind(this);
        this.endAllTestingContexts = this.endAllTestingContexts.bind(this);
        this.state = {
            starting: false,
            ending: false,
            endingAllContexts: false,
            loadingAllContexts: false,
            allContexts: [],
        }
    }

    async componentDidMount() {
        await this.reloadAllContexts();
    }

    async startTesting() {
        if (this.state.starting || this.state.ending || this.state.endingAllContexts) {
            return;
        }

        this.setState({
            starting: true
        });

        try {
            await this.testingApi.startTesting({
                copyExistingTables: true,
                setContextRequiredCookie: true
            });

            await this.props.reloadAll();
            await this.reloadAllContexts();

            this.setState({
                starting: false
            });
        } catch (e) {
            this.setState({
                error: e,
                starting: false
            });
        }
    }

    async endTesting() {
        if (this.state.starting || this.state.ending || this.state.endingAllContexts) {
            return;
        }

        this.setState({
            ending: true
        });

        try {
            await this.testingApi.stopTesting();

            await this.props.reloadAll();
            await this.reloadAllContexts();

            this.setState({
                ending: false
            });
        } catch (e) {
            this.setState({
                error: e,
                ending: false
            });
        }
    }

    async endAllTestingContexts() {
        if (this.state.starting || this.state.ending || this.state.endingAllContexts) {
            return;
        }

        this.setState({
            endingAllContexts: true
        });

        try {
            await this.testingApi.endAllTestingContexts();

            await this.props.reloadAll();
            await this.reloadAllContexts();

            this.setState({
                endingAllContexts: false
            });
        } catch (e) {
            this.setState({
                error: e,
                endingAllContexts: false
            });
        }
    }

    // renderers
    render() {
        return (<div>
            <p>Status: <b>{this.props.testing ? `Testing data in use` : 'Production data in use'}</b></p>
            {this.state.error ? (<Alert errors={this.state.error} />) : null}
            {this.props.testing ? this.renderContextOptions() : null}
            {this.props.testing ? null : (<button className="btn btn-warning margin-right" onClick={this.startTesting}>
                {this.state.starting ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                Start testing</button>)}
            {this.props.testing ? (<button className="btn btn-secondary margin-right" onClick={this.endTesting}>
                {this.state.ending ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                End testing</button>) : null}
            <button className="btn btn-danger margin-right" onClick={this.endAllTestingContexts}>
                {this.state.endingAllContexts ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                End all contexts</button>
            {this.renderAllContexts()}
        </div>)
    }

    renderContextOptions() {
        return (<div>
            <p>Context: <b>{this.props.testing}</b></p>
        </div>);
    }

    renderAllContexts() {
        return (<div>
            <hr />
            <h4>
                {this.state.loadingAllContexts ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                All contexts : {this.state.allContexts.length}</h4>
            <ul>
                {this.state.allContexts.map(context => (<li key={context.contextId}>{context.contextId}: {context.tables} table/s {context.self ? <i> - This testing context</i> : null}</li>))}
            </ul>
        </div>)
    }

    //api functions
    async reloadAllContexts() {
        this.setState({
            loadingAllContexts: true
        });

        try {
            const contexts = await this.testingApi.getAllTestingContexts();

            this.setState({
                loadingAllContexts: false,
                allContexts: contexts
            });
        } catch (e) {
            this.setState({
                loadingAllContexts: false,
                error: e
            });
        }
    }
}