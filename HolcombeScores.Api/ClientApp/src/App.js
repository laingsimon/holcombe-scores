import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { TeamDetails } from './components/TeamDetails';
import { GameDetails } from './components/GameDetails';
import { Teams } from './components/Teams';
import { AccessAdmin } from './components/AccessAdmin';
import { About } from './components/About';

import './custom.css'

export default class App extends Component {
  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/home/:mode?' component={Home} />
        <Route path='/team/:teamId/:mode?' component={TeamDetails} />
        <Route path='/teams/:mode?' component={Teams} />
        <Route path='/game/:gameId/:mode?' component={GameDetails} />
        <Route path='/admin/:mode?' component={AccessAdmin} />
        <Route path='/about' component={About} />
      </Layout>
    );
  }
}
