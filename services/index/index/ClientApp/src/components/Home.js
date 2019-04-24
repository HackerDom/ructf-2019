import React from 'react';
import { connect } from 'react-redux';
import { Link } from 'react-router-dom';

const Home = props => (
    <div>
        <h1>welcome to index</h1>
        <p>here we collect all files about VR simulation</p>
        <br />
        <p>
            <Link to="/login">create account</Link> and after<br />
            you can <Link to="/upload">add</Link> new zip archives to index<br />
            or <Link to="/search">search</Link> and observe already indexed content
            also you can left <Link to="/notespage">notes</Link> to save some data
        </p>
    </div>
);

export default connect()(Home);
