import React, {Component} from 'react';
import {Container} from 'reactstrap';
import {NavMenu} from './NavMenu';
import {Http} from "../api/http";
import {Access} from "../api/access";
import {Settings} from "../api/settings";

export class Layout extends Component {
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
            <div>
                <NavMenu access={this.state.access}/>
                <Container>
                    {this.props.children}
                </Container>
            </div>
        );
    }
}
