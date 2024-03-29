import React, {Component} from 'react';
import {Route} from 'react-router';
import {Layout} from './components/Layout';
import {Home} from './components/access/Home';
import {TeamDetails} from './components/team/TeamDetails';
import {GameDetails} from './components/game/GameDetails';
import {Teams} from './components/team/Teams';
import {Admin} from './components/admin/Admin';
import {About} from './components/About';

import './custom.css'
import {Http} from './api/http';
import {Settings} from './api/settings';
import {Access} from './api/access';
import {Team} from './api/team';
import {Functions} from './functions';
import {Game} from './api/game';
import {Player} from './api/player';
import {Availability} from './api/availability';
import {Testing} from './api/testing';

export default class App extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.accessApi = new Access(http);
        this.teamApi = new Team(http);
        this.gameApi = new Game(http);
        this.playerApi = new Player(http);
        this.availabilityApi = new Availability(http);
        this.testingApi = new Testing(http);
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
        this.updateGame = this.updateGame.bind(this);
        this.reloadAvailability = this.reloadAvailability.bind(this);
        this.reloadTestingState = this.reloadTestingState.bind(this);
    }

    async componentDidMount() {
        await this.reloadAll();
    }

    async reloadTestingState() {
        const subProps = Object.assign({}, this.state.subProps);
        subProps.testing = await this.getTestingState();

        this.setState({
            subProps: subProps
        });
    }

    async getTestingState() {
        const testingId = await this.testingApi.getTestingId();
        return testingId;
    }

    async updateGame(game) {
        const subProps = Object.assign({}, this.state.subProps);
        subProps.testing = await this.getTestingState();

        if (subProps.game && subProps.game.id === game.id) {
            subProps.game = game;
        }
        if (subProps.team != null && subProps.team.id === game.teamId && subProps.team.games) {
            const teamGames = subProps.team.games.filter(g => g.id !== game.id);
            teamGames.push(game);
            teamGames.sort(Functions.gameSortFunction);
            subProps.team.games = teamGames;
        }

        this.setState({
            subProps: subProps
        });
    }

    async reloadAvailability(teamId, gameId) {
        const subProps = Object.assign({}, this.state.subProps);
        subProps.testing = await this.getTestingState();

        subProps.gameAvailability = await this.availabilityApi.get(teamId, gameId);
        subProps.gameAvailability.sort(Functions.playerAvailabilitySortFunction);

        this.setState({
            subProps: subProps
        });
    }

    async reloadGame(id, skipTesting, skipAvailability) {
        const subProps = Object.assign({}, this.state.subProps);
        subProps.testing = skipTesting ? subProps.testing : await this.getTestingState();
        subProps.game = await this.gameApi.getGame(id);
        if (subProps.game) {
            subProps.game.found = true;
            subProps.game.asAt = new Date();
            subProps.game.squad.sort(Functions.playerSortFunction);

            if (!subProps.team || subProps.team.id !== subProps.game.teamId) {
                subProps.team = subProps.teams.filter(t => t.id === subProps.game.teamId)[0];

                if (!subProps.team.players) {
                    subProps.team.players = await this.playerApi.getPlayers(subProps.team.id);
                    subProps.team.players.sort(Functions.playerSortFunction);
                }
            }

            subProps.gameAvailability = skipAvailability ? subProps.gameAvailability : await this.availabilityApi.get(subProps.game.teamId, id);
            subProps.gameAvailability.sort(Functions.playerAvailabilitySortFunction);
        } else {
            subProps.game = {
                id: id,
                asAt: new Date(),
                squad: [],
                found: false
            };
            subProps.team = null;
        }

        this.setState({
            subProps: subProps
        });
    }

    async reloadTeam(id, reloadTeam, reloadPlayers, reloadGames) {
        const subProps = Object.assign({}, this.state.subProps);
        subProps.testing = await this.getTestingState();
        const existingTeam = subProps.teams.filter(t => t.id === id)[0];
        subProps.team = reloadTeam || !existingTeam ? await this.teamApi.getTeam(id) : existingTeam;

        if (subProps.team) {
            subProps.team.asAt = new Date();
            subProps.team.found = true;
            if (reloadPlayers || !existingTeam || !existingTeam.players) {
                subProps.team.players = await this.playerApi.getPlayers(id);
                subProps.team.players.sort(Functions.playerSortFunction);
            } else {
                subProps.team.players = existingTeam.players;
            }

            if (reloadGames || !existingTeam || !existingTeam.games) {
                subProps.team.games = await this.gameApi.getGames(id);
                subProps.team.games.sort(Functions.gameSortFunction);
            } else {
                subProps.team.games = existingTeam.games;
            }

            const replacementTeams = subProps.teams.filter(t => t.id !== subProps.team.id);
            replacementTeams.push(subProps.team);
            replacementTeams.sort(Functions.teamSortFunction);
            subProps.teams = replacementTeams;
        } else {
            subProps.team = {
                id: id,
                players: [],
                games: [],
                found: false
            }
        }

        this.setState({
            subProps: subProps
        });
    }

    async reloadTeams() {
        const subProps = Object.assign({}, this.state.subProps);
        subProps.testing = await this.getTestingState();
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
        subProps.testing = await this.getTestingState();
        const access = await this.accessApi.getMyAccess();
        subProps.access = access.access;
        subProps.requests = access.requests;
        subProps.isImpersonated = access.access && access.access.impersonatedBy;

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
                access: access ? access.access : null,
                requests: access ? access.requests : null,
                isImpersonated: access.access && access.access.impersonatedBy,
                teams: teams,
                reloadAccess: this.reloadAccess,
                reloadTeams: this.reloadTeams,
                reloadTeam: this.reloadTeam,
                reloadGame: this.reloadGame,
                reloadAll: this.reloadAll,
                updateGame: this.updateGame,
                reloadAvailability: this.reloadAvailability,
                reloadTestingState: this.reloadTestingState,
                testing: await this.getTestingState()
            },
            loading: false
        });
    }

    render() {
        if (this.state.loading) {
            return (<div className="margin-top-50 d-flex justify-content-center">
                <div className="spinner-border spinner-football" role="status">
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
                <Route path='/admin/:mode?' render={(props) => <Admin {...(this.combineProps(props))} />} />
                <Route path='/about' render={(props) => <About {...(this.combineProps(props))} />} />
            </Layout>
        );
    }

    combineProps(props) {
        return Object.assign(props, this.state.subProps);
    }
}
