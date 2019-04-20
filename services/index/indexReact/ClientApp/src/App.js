import React from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import Home from './components/Home';
import FetchData from './components/FetchData';
import Login from './components/Login';
import Upload from './components/Upload';

export default (props) => (
    <Layout history={props.history}>
        <Route exact path='/' component={Home} />
        <Route path='/fetch-data/:startDateIndex?' component={FetchData} />
        <Route path='/login' component={Login} />
        <Route path='/upload' component={Upload} />
    </Layout>
);