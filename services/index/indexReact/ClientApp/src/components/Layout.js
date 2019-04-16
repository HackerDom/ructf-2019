import React, { Component } from 'react';
import { connect } from 'react-redux';
import { Container } from 'reactstrap';
import { bindActionCreators } from 'redux';
import { actionCreators } from '../store/User';
import NavMenu from './NavMenu';

class Layout extends Component {
    componentDidMount() {
        // This method is called when the component is first added to the document
        this.ensureDataFetched();
    }

    ensureDataFetched() {
        this.props.fetchUser();
    }

    render() {
        return (
            <div>
                <NavMenu fetchUser={this.props.fetchUser} removeUser={this.props.removeUser} history={this.props.history}/>
                <Container>
                    {this.props.children}
                </Container>
            </div>
        );
    }
}

export default connect(
    state => state.user,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Layout);
