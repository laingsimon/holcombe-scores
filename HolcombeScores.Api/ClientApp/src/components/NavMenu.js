import React, { Component } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';
import {Http} from "../api/http";
import {Settings} from "../api/settings";
import {Access} from "../api/access";

export class NavMenu extends Component {
  constructor (props) {
    super(props);
    const http = new Http(new Settings());
    this.accessApi = new Access(http);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {
      collapsed: true,
      access: null
    };
  }

  //event handlers
  toggleNavbar () {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  async componentDidMount() {
    const access = await this.accessApi.getMyAccess();
    this.setState({
      access: access.access
    });
  }

  // renderers
  render () {
    return (
      <header>
        <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
          <Container>
            <NavbarBrand tag={Link} to="/home/access">⚽ Holcombe Scores</NavbarBrand>
            <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
            <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
              <ul className="navbar-nav flex-grow">
                <NavItem>
                  <NavLink tag={Link} className="text-dark" to="/home/access">Home</NavLink>
                </NavItem>
                {this.state.access ? (<NavItem>
                  <NavLink tag={Link} className="text-dark" to={`/team/${this.state.access.teamId}/view`}>Team</NavLink>
                </NavItem>) : null}
                {this.state.access && (this.state.access.admin) ? (<NavItem>
                  <NavLink tag={Link} className="text-dark" to="/teams/view">Teams</NavLink>
                </NavItem>) : null}
                {this.state.access && (this.state.access.admin || this.state.access.manager) ? (<NavItem>
                  <NavLink tag={Link} className="text-dark" to="/admin/requests">Admin</NavLink>
                </NavItem>) : null}
                <NavItem>
                  <NavLink tag={Link} className="text-dark" to="/about">About</NavLink>
                </NavItem>
              </ul>
            </Collapse>
          </Container>
        </Navbar>
      </header>
    );
  }
}
