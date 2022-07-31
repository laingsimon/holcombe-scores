import React, {Component} from 'react';
import {Settings} from '../api/settings';
import {Http} from '../api/http';
import {Game} from '../api/game';
import {Alert} from "./Alert";
import {PlayerList} from "./PlayerList";
import {Functions} from '../functions'

/*
* Props:
* - [game]
* - team
*
* Events:
* - onLoaded()
* - onChanged(gameId, teamId)
* - onDeleted(gameId, teamId)
* - onCreated(gameId, teamId)
*/
export class EditGame extends Component {
    constructor(props) {
        super(props);
        const http = new Http(new Settings());
        this.gameApi = new Game(http);
        this.state = {
            loading: true,
            playersLoaded: false,
            deleted: false,
            proposed: this.props.game ? null : this.defaultGameDetails() // the updated game details
        };
        this.valueChanged = this.valueChanged.bind(this);
        this.updateGame = this.updateGame.bind(this);
        this.deleteGame = this.deleteGame.bind(this);
        this.onPlayerChanged = this.onPlayerChanged.bind(this);
        this.onLoaded = this.onLoaded.bind(this);
    }

    componentDidMount() {
        this.getGameDetails();
    }

    // event handlers
    valueChanged(event) {
        const name = event.target.name;
        const type = event.target.getAttribute('type');
        const value = type === 'checkbox'
            ? event.target.checked
            : event.target.value;
        const newProposed = Object.assign({}, this.state.proposed);
        newProposed[name] = value;

        this.setState({
            proposed: newProposed
        });
    }

    onPlayerChanged(teamId, playerId, selected) {
        const proposedClone = Object.assign({}, this.state.proposed);
        proposedClone.players = selected
            ? Functions.union(this.state.proposed.players, playerId)
            : Functions.except(this.state.proposed.players, playerId);

        this.setState({
            proposed: proposedClone
        });
    }

    onLoaded() {
        this.setState({
            playersLoaded: true
        });

        // noinspection JSUnresolvedVariable
        if (this.props.onLoaded) {
            this.props.onLoaded();
        }
    }

    async updateGame() {
        if (!this.state.playersLoaded) {
            return;
        }

        try {
            const playerIds = Object.keys(this.state.proposed.players);

            if (playerIds.length === 0) {
                alert('You must select some players');
                return;
            }

            if (!this.state.proposed.opponent) {
                alert('You must enter the name of the opponent');
                return;
            }

            this.setState({
                loading: true,
                apiResult: null,
            });

            await this.applyApiChanges(Functions.toUtcDateTime(new Date(this.state.proposed.date)), playerIds);
        } catch (e) {
            console.error(e);
            this.setState({
                loading: false,
                error: e.message
            });
        }
    }

    async deleteGame() {
        if (!window.confirm('Are you sure you want to delete this game?')) {
            return;
        }

        this.setState({
            loading: true
        });

        const result = await this.gameApi.deleteGame(this.props.game.id);

        if (result.success) {
            if (this.props.onDeleted) {
                this.props.onDeleted(this.props.game.id, this.props.team.id);
            }
        }

        this.setState({
            loading: false,
            deleted: true,
            apiResult: result
        });
    }

    renderGameDeleted() {
        return (<div>
            <Alert messages={this.state.apiResult.messages} warnings={this.state.apiResult.warnings}
                   errors={this.state.apiResult.errors}/>
            <hr/>
            <a href={`/team/${this.props.team.id}`} className="btn btn-primary">View games</a>
        </div>);
    }

    // renders
    renderApiResult(result) {
        if (!result) {
            return;
        }

        if (result.success) {
            if (!this.props.game.id) {
                return (<div>
                    <Alert messages={result.messages} warnings={result.warnings} errors={result.errors}/>
                    <hr/>
                    <a href={`/game/${result.outcome.id}/play`} className="btn btn-primary">Play game</a>
                </div>);
            }
        }

        return (<Alert messages={result.messages} warnings={result.warnings} errors={result.errors}/>);
    }

    renderError(error) {
        const back = (() => {
            this.setState({
                error: null
            });
        });

        return (<div>
            <Alert errors={[error]}/>
            <hr/>
            <button type="button" className="btn btn-primary" onClick={back}>Back</button>
        </div>);
    }

    render() {
        try {
            if (this.state.loading) {
                return (<div className="d-flex justify-content-center">
                    <div className="spinner-border" role="status">
                        <span className="visually-hidden">Loading...</span>
                    </div>
                </div>);
            }

            if (this.state.error) {
                return this.renderError(this.state.error);
            }

            if (this.state.deleted) {
                return this.renderGameDeleted();
            }

            if (!this.props.game && this.state.apiResult) {
                // game created
                return this.renderApiResult(this.state.apiResult);
            }

            const deleteButton = this.props.game
                ? (<button className="btn btn-danger" onClick={this.deleteGame}>Delete game</button>)
                : null;

            return (<div>
                {this.renderApiResult(this.state.apiResult)}
                <div className="input-group mb-3">
                    <div className="input-group-prepend">
                        <span className="input-group-text" id="basic-addon3">Opponent</span>
                    </div>
                    <input type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3"
                           name="opponent" value={this.state.proposed.opponent} onChange={this.valueChanged}/>
                </div>
                <div className="input-group mb-3">
                    <div className="form-check form-switch">
                        <input className="form-check-input" type="checkbox" id="flexSwitchCheckDefault"
                               name="playingAtHome" checked={this.state.proposed.playingAtHome}
                               onChange={this.valueChanged}/>
                        <label className="form-check-label" htmlFor="flexSwitchCheckDefault">Playing at home</label>
                    </div>
                </div>
                <div className="input-group mb-3">
                    <div className="input-group-prepend">
                        <span className="input-group-text" id="basic-addon3">📆 Date</span>
                    </div>
                    <input type="datetime-local" className="form-control" id="basic-url" aria-describedby="basic-addon3"
                           name="date" value={this.state.proposed.date} onChange={this.valueChanged}/>
                </div>
                <PlayerList teamId={this.props.team.id} selected={this.state.proposed.players}
                            onPlayerChanged={this.onPlayerChanged} onLoaded={this.onLoaded}/>
                <hr/>
                <button type="button" className="btn btn-primary"
                        onClick={this.updateGame}>{this.props.game ? 'Update game' : 'Create game'}</button>
                &nbsp;
                {deleteButton}
            </div>);
        } catch (e) {
            console.error(e);
            return (<Alert errors={[e.message]}/>);
        }
    }

    // apis
    getGameDetails() {
        let proposedGame = Object.assign({}, this.props.game || {});
        if (this.props.game) {
            proposedGame.date = Functions.toLocalDateTime(new Date(this.props.game.date));
            proposedGame.players = {};

            // noinspection JSUnresolvedVariable
            delete proposedGame.squad;

            // noinspection JSUnresolvedVariable
            this.props.game.squad.forEach(player => {
                proposedGame.players[player.id] = true;
            });
        }

        this.setState({
            loading: false,
            proposed: proposedGame,
        });
    }

    async applyApiChanges(utcDateTime, playerIds) {
        try {
            const game = this.props.game;
            const team = this.props.team;
            const proposed = this.state.proposed;
            const apiFunction = this.props.game
                ? async () => await this.gameApi.updateGame(game.id, team.id, utcDateTime, proposed.opponent, proposed.playingAtHome, playerIds)
                : async () => await this.gameApi.createGame(team.id, utcDateTime, proposed.opponent, proposed.playingAtHome, playerIds);

            const result = await apiFunction();

            if (result.success) {
                if (this.props.game) {
                    if (this.props.onChanged) {
                        this.props.onChanged(result.outcome.id, result.outcome.teamId);
                    }
                } else {
                    if (this.props.onCreated) {
                        this.props.onCreated(result.outcome.id, result.outcome.teamId);
                    }
                }
            }

            this.setState({
                apiResult: result,
                loading: false
            });

            this.setState({
                apiResult: result,
                loading: false
            });
        } catch (e) {
            console.error(e);
            this.setState({
                loading: false,
                error: e.message
            });
        }
    }

    // utility functions
    defaultGameDetails() {
        return {
            opponent: "",
            playingAtHome: true,
            date: new Date().toISOString().substring(0, 10) + 'T11:00'
        };
    }
}
