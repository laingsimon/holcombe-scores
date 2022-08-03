import React, {Component} from 'react';
import {Settings} from '../../api/settings';
import {Http} from '../../api/http';
import {Game} from '../../api/game';
import {Alert} from '../Alert';
import {PlayerList} from './PlayerList';
import {Functions} from '../../functions'
import { Link } from 'react-router-dom';

/*
* Props:
* - [game]
* - team
* - teamPlayers
*
* Events:
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
            saving: false,
            deleting: false,
            deleted: false,
            readOnly: this.props.game ? this.props.game.readOnly : false,
            proposed: this.props.game ? this.getGameDetails(this.props.game) : this.defaultGameDetails(),
            loadingGame: false
        };
        this.valueChanged = this.valueChanged.bind(this);
        this.updateGame = this.updateGame.bind(this);
        this.deleteGame = this.deleteGame.bind(this);
        this.onPlayerSelected = this.onPlayerSelected.bind(this);
        this.beforePlayNewGame = this.beforePlayNewGame.bind(this);
        this.reset = this.reset.bind(this);
    }

    // event handlers
    reset() {
        this.setState({
            proposed: this.defaultGameDetails(),
            apiResult: null,
        });
    }

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

    onPlayerSelected(teamId, playerId, selected) {
        const proposedClone = Object.assign({}, this.state.proposed);
        proposedClone.players = selected
            ? Functions.union(this.state.proposed.players, playerId)
            : Functions.except(this.state.proposed.players, playerId);

        this.setState({
            proposed: proposedClone
        });
    }

    async updateGame() {
        try {
            if (this.state.deleting || this.state.saving) {
                return;
            }

            const playerIds = this.state.proposed.players
                ? Object.keys(this.state.proposed.players)
                : [];

            if (playerIds.length === 0) {
                alert('You must select some players');
                return;
            }

            if (!this.state.proposed.opponent) {
                alert('You must enter the name of the opponent');
                return;
            }

            this.setState({
                saving: true,
                apiResult: null,
            });

            await this.applyApiChanges(Functions.toUtcDateTime(new Date(this.state.proposed.date)), playerIds);
        } catch (e) {
            console.error(e);
            this.setState({
                saving: false,
                error: e.message
            });
        }
    }

    async deleteGame() {
        if (this.state.deleting || this.state.saving) {
            return;
        }

        if (!window.confirm('Are you sure you want to delete this game?')) {
            return;
        }

        this.setState({
            deleting: true
        });

        const result = await this.gameApi.deleteGame(this.props.game.id);

        if (result.success) {
            if (this.props.onDeleted) {
                await this.props.onDeleted(this.props.game.id, this.props.team.id);
            }
        }

        this.setState({
            deleting: false,
            deleted: true,
            apiResult: result
        });
    }

    async beforePlayNewGame(event) {
        event.preventDefault();
        this.setState({
            loadingGame: true
        });

        await this.props.reloadGame(this.state.apiResult.outcome.id);

        this.setState({
            loadingGame: false
        });

        this.props.history.push(event.target.getAttribute('href'));
    }

    // renders
    renderGameDeleted() {
        return (<div>
            <Alert messages={this.state.apiResult.messages} warnings={this.state.apiResult.warnings}
                   errors={this.state.apiResult.errors}/>
            <hr/>
            <Link to={`/team/${this.props.team.id}`} className="btn btn-primary">View games</Link>
        </div>);
    }

    renderApiResult(result) {
        if (!result) {
            return;
        }

        if (result.success) {
            if (!this.props.game) {
                return (<div>
                    <Alert messages={result.messages} warnings={result.warnings} errors={result.errors}/>
                    <hr/>
                    <Link to={`/game/${result.outcome.id}/play`} onClick={this.beforePlayNewGame} className="btn btn-primary margin-right">
                        {this.state.loadingGame ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                        {this.state.loadingGame ? 'Loading game...' : 'Play game'}
                    </Link>
                    <button onClick={this.reset} className="btn btn-primary">
                        Create another
                    </button>
                </div>);
            }
        }

        return (<div>
            <hr />
            <Alert messages={result.messages} warnings={result.warnings} errors={result.errors}/>
        </div>);
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
                ? (<button className="btn btn-danger" onClick={this.deleteGame}>
                    {this.state.deleting ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                    Delete game
                   </button>)
                : null;

            return (<div>
                <div className="input-group mb-3">
                    <div className="input-group-prepend">
                        <span className="input-group-text" id="basic-addon3">Opponent</span>
                    </div>
                    <input readOnly={this.state.readOnly || this.state.saving || this.state.deleting} type="text" className="form-control" id="basic-url" aria-describedby="basic-addon3"
                           name="opponent" value={this.state.proposed.opponent} onChange={this.valueChanged}/>
                </div>
                <div className="input-group mb-3">
                    <div className="form-check form-switch">
                        <input disabled={this.state.readOnly || this.state.saving || this.state.deleting} className="form-check-input" type="checkbox" id="flexSwitchCheckDefault"
                               name="playingAtHome" checked={this.state.proposed.playingAtHome}  onChange={this.valueChanged}/>
                        <label className="form-check-label" htmlFor="flexSwitchCheckDefault">Playing at home</label>
                    </div>
                </div>
                <div className="input-group mb-3">
                    <div className="input-group-prepend">
                        <span className="input-group-text" id="basic-addon3">📆 Date</span>
                    </div>
                    <input readOnly={this.state.readOnly || this.state.saving || this.state.deleting} type="datetime-local" className="form-control" id="basic-url" aria-describedby="basic-addon3"
                           name="date" value={this.state.proposed.date} onChange={this.valueChanged}/>
                </div>
                <PlayerList players={this.props.team.players} selected={this.state.proposed.players}
                            onPlayerSelected={this.onPlayerSelected} readOnly={this.state.readOnly || this.state.saving || this.state.deleting} />
                <hr/>
                {this.state.readOnly ? null : (<button type="button" className="btn btn-primary margin-right" onClick={this.updateGame}>
                    {this.state.saving ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : null}
                    {this.props.game ? 'Update game' : 'Create game'}
                </button>)}
                {deleteButton}
                {this.renderApiResult(this.state.apiResult)}
            </div>);
        } catch (e) {
            console.error(e);
            return (<Alert errors={[e.message]}/>);
        }
    }

    // apis
    getGameDetails(game) {
        let proposedGame = Object.assign({}, game);
        proposedGame.date = Functions.toLocalDateTime(new Date(game.date));
        proposedGame.players = {};

        delete proposedGame.squad; //remove squad to prevent confusion between players (dict) and squad (array)

        game.squad.forEach(player => {
            proposedGame.players[player.id] = true;
        });

        return proposedGame;
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
                        await this.props.onChanged(result.outcome.id, result.outcome.teamId);
                    }
                } else {
                    if (this.props.onCreated) {
                        await this.props.onCreated(result.outcome.id, result.outcome.teamId);
                    }
                }
            }

            this.setState({
                proposed: this.props.game ? proposed : this.defaultGameDetails(),
                apiResult: result,
                saving: false
            });

            this.setState({
                apiResult: result,
                saving: false
            });
        } catch (e) {
            console.error(e);
            this.setState({
                saving: false,
                error: e.message
            });
        }
    }

    // utility functions
    defaultGameDetails() {
        return {
            opponent: '',
            playingAtHome: true,
            date: new Date().toISOString().substring(0, 10) + 'T11:00'
        };
    }
}