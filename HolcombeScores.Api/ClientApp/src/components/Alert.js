import React, { Component } from 'react';

/*
* Props:
* - [messages]
* - [warnings]
* - [errors]
*
* Events:
* -none-
* */
export class Alert extends Component {
    render() {
        return (<div>
            {this.renderMessages(this.props.messages, "alert-success")}
            {this.renderMessages(this.props.warnings, "alert-warning")}
            {this.renderMessages(this.props.errors, "alert-danger")}
        </div>)
    }

    renderMessages(lines, className) {
        if (!lines || lines.length === 0) {
            return;
        }

        return (<div className={`alert ${className}`} role="alert">
            {lines.reduce((current, msg) => current + "\n" + msg, "")}
        </div>);
    }
}