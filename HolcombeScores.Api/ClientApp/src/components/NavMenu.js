import React, { Component } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';

/*
* Props:
* - access
* - teams
*
* Events:
* -none-
* */
export class NavMenu extends Component {
  constructor (props) {
    super(props);
    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.collapseNavbar = this.collapseNavbar.bind(this);
    this.changeTeam = this.changeTeam.bind(this);
    this.state = {
      collapsed: true,
      changingTeam: null
    };
  }

  //event handlers
  toggleNavbar () {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  collapseNavbar () {
    this.setState({
      collapsed: true
    });
  }

  async changeTeam(event) {
    event.preventDefault();

    if (this.state.changingTeam) {
      return;
    }

    let target = event.target;
    while (target && !target.getAttribute('href')) {
      target = event.target.parentElement;
    }

    if (!target) {
      return;
    }

    const url = target.getAttribute('href');

    const segments = url.split('/')
    const teamId = segments[segments.length - 2];

    if (!this.props.team || teamId !== this.props.team.id) {
      this.setState({
        changingTeam: teamId
      });

      const reloadTeam = true;
      const reloadPlayers = true;
      const reloadGames = true;
      await this.props.reloadTeam(teamId, reloadTeam, reloadPlayers, reloadGames);
    }

    window.history.replaceState(null, event.target.textContent, url);

    this.setState({
      changingTeam: null,
      collapsed: true
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
                  <NavLink onClick={this.collapseNavbar} tag={Link} className="text-dark" to="/home/access">🏟️ Home</NavLink>
                </NavItem>
                {this.props.access ? this.renderTeamsNavs(this.props.access, this.props.teams) : null}
                {this.props.access && (this.props.access.admin || this.props.access.teams.length > 1) ? (<NavItem>
                  <NavLink onClick={this.collapseNavbar} tag={Link} className="text-dark" to="/teams/view">🎽 Teams</NavLink>
                </NavItem>) : null}
                {this.props.access && (this.props.access.admin || this.props.access.manager) ? (<NavItem>
                  <NavLink onClick={this.collapseNavbar} tag={Link} className="text-dark" to="/admin/requests">⚙ Admin</NavLink>
                </NavItem>) : null}
                <NavItem>
                  <NavLink onClick={this.collapseNavbar} tag={Link} className="text-dark" to="/about">⁉ About</NavLink>
                </NavItem>
              </ul>
            </Collapse>
          </Container>
        </Navbar>
      </header>
    );
  }

  renderTeamsNavs(access, teams) {
    return access.teams.map(teamId => {
      const team = teams.filter(t => t.id === teamId)[0];

      return (<NavItem key={teamId}>
        <NavLink onClick={this.changeTeam} tag={Link} className="text-dark" to={`/team/${teamId}/view`}>
          {this.state.changingTeam === teamId ? (<span className="spinner-border spinner-border-sm margin-right" role="status" aria-hidden="true"></span>) : <span className="margin-right" role="status" aria-hidden="true">⛹</span>}
          {team.name}
        </NavLink>
      </NavItem>)
    });
  }
}
