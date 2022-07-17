import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Games } from './components/Games';
import { GameComponent } from './components/GameComponent';
import { Teams } from './components/Teams';

import './custom.css'

export default class App extends Component {
  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/games/:teamId?' component={Games} />
        <Route path='/teams' component={Teams} />
        <Route path='/game/:gameId' component={GameComponent} />
      </Layout>
    );
  }
}
