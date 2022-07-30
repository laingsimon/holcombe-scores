import React, {Component} from 'react';
import {Route} from 'react-router';
import {Layout} from './components/Layout';
import {Home} from './components/Home';
import {TeamDetails} from './components/TeamDetails';
import {GameDetails} from './components/GameDetails';
import {Teams} from './components/Teams';
import {AccessAdmin} from './components/AccessAdmin';
import {About} from './components/About';

import './custom.css'
import {Http} from "./api/http";
import {Settings} from "./api/settings";
import {Access} from "./api/access";

export default class App extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.state = {
            loading: true,
            access: null,
        };
    }

    async componentDidMount() {
        const access = await this.accessApi.getMyAccess();
        this.setState({
            access: access.access,
            loading: false
        });
    }

    render() {
        if (this.loading) {
            return (<div className="d-flex justify-content-center">
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>);
        }

        return (
            <Layout>
                <Route exact path='/' render={(props) => <Home {...props} access={this.state.access} />} />
                <Route path='/home/:mode?' render={(props) => <Home {...props} access={this.state.access} />} />
                <Route path='/team/:teamId/:mode?' render={(props) => <TeamDetails {...props} access={this.state.access} />} />
                <Route path='/teams/:mode?' render={(props) => <Teams {...props} access={this.state.access} />} />
                <Route path='/game/:gameId/:mode?' render={(props) => <GameDetails {...props} access={this.state.access} />} />
                <Route path='/admin/:mode?' render={(props) => <AccessAdmin {...props} access={this.state.access} />} />
                <Route path='/about' render={(props) => <About {...props} access={this.state.access} />} />
            </Layout>
        );
    }
}
