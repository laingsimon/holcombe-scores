import React, { Component } from 'react';
import { Link } from "react-router-dom";

export class About extends Component {
    render() {
        return (<div>
            <p>
                The purpose of <strong>Holcombe scores</strong> is to provide a means for the players and supporters to view the scores of a game as they're played, and to look back at the history of previous games.
                This allows for players and supporters to be part of the action when they may not be able to attend the game itself.
                <i> No more need for whatsapp messages between supporters!</i>
            </p>
            <p>This service has no affiliation with <a href="https://www.holcombeyfc.com/">Holcombe Youth Football Club</a>.</p>
            <h4>How it works</h4>
            <ol>
                <li>Go to the <Link to="/">registration page</Link>, enter your name and select your team</li>
                <li>A manager or administrator will approve your request at which point you'll be able to see the games</li>
                <li>You can record the goals as they're scored from anywhere</li>
            </ol>
            <h4>Disclaimers</h4>
            <p><strong>Parental approval should be sought before entering any data into this service.</strong></p>
            <p>The service provided free by <a href="https://laingsimon.github.io/blog/">Simon Laing</a> for management by the coaches and managers of <a href="https://www.holcombeyfc.com/">Holcombe Youth Football Club</a>.</p>
            <p>This is a free service and will be maintained for so long as it can remain so. Any misuse/abuse of the service or where material costs are involved will result in the service being taken offline.</p>
            <p>I, <a href="https://github.com/laingsimon/">Simon Laing</a>, take no responsibility for the accuracy of the data entered in to this service. I will however endeavour to ensure it is always held in a secure fashion.</p>
            <h5>Links</h5>
            <ul>
                <li><a href="https://www.holcombeyfc.com/">Holcombe Youth Football Club</a></li>
                <li><a href="https://laingsimon.github.io/blog/">Simon Laing</a></li>
            </ul>
        </div>);
    }
}
