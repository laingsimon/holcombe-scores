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
        this.state = {
            loading: false
        }
    }

    async startTesting() {
        if (this.state.loading) {
            return;
        }

        this.setState({
            loading: true
        });

        try {
            await this.testingApi.startTesting({
                copyExistingTables: true,
                setContextRequiredCookie: true
            });

            await this.props.reloadAll();

            this.setState({
                loading: false
            });
        } catch (e) {
            this.setState({
                error: e,
                loading: false
            });
        }
    }

    async endTesting() {
        if (this.state.loading) {
            return;
        }

        this.setState({
            loading: true
        });

        try {
            await this.testingApi.stopTesting();

            await this.props.reloadAll();

            this.setState({
                loading: false
            });
        } catch (e) {
            this.setState({
                error: e,
                loading: false
            });
        }
    }

    render() {
        return (<div>
            <p>Status: <b>{this.props.testing ? `Testing data in use` : 'Production data in use'}</b></p>
            {this.state.error ? (<Alert errors={this.state.error} />) : null}
            {this.props.testing ? this.renderContextOptions() : null}
            <div>
                {this.props.testing ? null : (<button className="btn btn-warning margin-right" onClick={this.startTesting}>
                    {this.state.loading ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                    Start testing</button>)}
            </div>
        </div>)
    }

    renderContextOptions() {
        return (<div>
            <p>Context: <b>{this.props.testing}</b></p>
            <button className="btn btn-secondary" onClick={this.endTesting}>
                {this.state.loading ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                End testing</button>
        </div>);
    }
}