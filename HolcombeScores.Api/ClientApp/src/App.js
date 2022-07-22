import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { TeamDetails } from './components/TeamDetails';
import { GameDetails } from './components/GameDetails';
import { Teams } from './components/Teams';

import './custom.css'

export default class App extends Component {
  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/team/:teamId/:mode?' component={TeamDetails} />
        <Route path='/teams/:mode?' component={Teams} />
        <Route path='/game/:gameId/:mode?' component={GameDetails} />
      </Layout>
    );
  }
}
