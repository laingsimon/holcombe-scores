import React, { Component } from 'react';

/*
* Props:
* - playingAtHome
* - score
*
* Events:
* -none-
* */
export class Score extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        const score = this.props.playingAtHome
            ? `${this.props.score.holcombe}-${this.props.score.opponent}`
            : `${this.props.score.opponent}-${this.props.score.holcombe}`;
        const winning = this.props.score.holcombe > this.props.score.opponent;
        const drawing = this.props.score.holcombe === this.props.score.opponent;
        const colour = winning
            ? 'bg-success'
            : drawing
                ? 'bg-primary'
                : 'bg-danger';

        return (<span className={`badge rounded-pill ${colour}`}>{score}</span>);
    }
}