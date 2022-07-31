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
import {Team} from "./api/team";
import {Functions} from "./functions";
import {Game} from "./api/game";

export default class App extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.teamApi = new Team(http);
        this.gameApi = new Game(http);
        this.state = {
            loading: true,
            subProps: null,
        };
        this.reloadAccess = this.reloadAccess.bind(this);
        this.reloadTeams = this.reloadTeams.bind(this);
        this.reloadAll = this.reloadAll.bind(this);
        this.reloadGame = this.reloadGame.bind(this);
        this.reloadTeam = this.reloadTeam.bind(this);
        this.combineProps = this.combineProps.bind(this);
    }

    async componentDidMount() {
        await this.reloadAll();
    }

    async reloadGame(id) {
        const subProps = Object.assign({}, this.state.subProps);
        subProps.game = await this.gameApi.getGame(id);
        subProps.game.asAt = new Date();
        subProps.game.squad.sort(Functions.playerSortFunction);

        if (!subProps.team || subProps.team.id !== subProps.game.teamId) {
            subProps.team = subProps.teams.filter(t => t.id === subProps.game.teamId)[0];
        }

        this.setState({
            subProps: subProps
        });
    }

    async reloadTeam(id) {
        const teams = await this.reloadTeams();
        const subProps = Object.assign({}, this.state.subProps);
        subProps.team = teams.filter(t => t.id === id)[0];
        subProps.team.players.sort(Functions.playerSortFunction);
        this.setState({
            subProps: subProps
        });
    }

    async reloadTeams() {
        const subProps = Object.assign({}, this.state.subProps);
        subProps.teams = await this.teamApi.getAllTeams();
        subProps.teams.forEach(t => t.asAt = new Date());
        subProps.teams.sort(Functions.teamSortFunction);
        this.setState({
            subProps: subProps
        });
        return subProps.teams;
    }

    async reloadAccess() {
        const subProps = Object.assign({}, this.state.subProps);
        const access = await this.accessApi.getMyAccess();
        subProps.access = access.access;
        subProps.request = access.request;

        this.setState({
            subProps: subProps,
        });
    }

    async reloadAll() {
        const access = await this.accessApi.getMyAccess();
        const teams = await this.teamApi.getAllTeams();
        teams.sort(Functions.teamSortFunction);
        this.setState({
            subProps: {
                access: access.access,
                request: access.request,
                teams: teams,
                reloadAccess: this.reloadAccess,
                reloadTeams: this.reloadTeams,
                reloadGame: this.reloadGame
            },
            loading: false
        });
    }

    render() {
        if (this.state.loading) {
            return (<div className="d-flex justify-content-center">
                <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>);
        }

        return (
            <Layout {...this.combineProps({...this.props})}>
                <Route exact path='/' render={(props) => <Home {...(this.combineProps(props))} />} />
                <Route path='/home/:mode?' render={(props) => <Home {...(this.combineProps(props))} />} />
                <Route path='/team/:teamId/:mode?' render={(props) => <TeamDetails {...(this.combineProps(props))} />} />
                <Route path='/teams/:mode?' render={(props) => <Teams {...(this.combineProps(props))} />} />
                <Route path='/game/:gameId/:mode?' render={(props) => <GameDetails {...(this.combineProps(props))} />} />
                <Route path='/admin/:mode?' render={(props) => <AccessAdmin {...(this.combineProps(props))} />} />
                <Route path='/about' render={(props) => <About {...(this.combineProps(props))} />} />
            </Layout>
        );
    }

    combineProps(props) {
        return Object.assign(props, this.state.subProps);
    }
}
