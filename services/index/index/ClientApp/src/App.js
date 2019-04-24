import React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import Home from './components/Home';
import Login from './components/Login';
import Notes from './components/Notes';
import Search from './components/Search';
import Upload from './components/Upload';

export default (props) => (
    <Layout history={props.history}>
        <Route exact path='/' component={Home} />
        <Route path='/login' component={Login} />
        <Route path='/upload' component={Upload} />
        <Route path='/search' component={Search} />
        <Route path='/notes' component={Notes} />
    </Layout>
);
