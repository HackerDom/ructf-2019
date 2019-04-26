import React from 'react';
import { connect } from 'react-redux';
import { ListGroup, ListGroupItem, Spinner, Alert } from 'reactstrap';
import { bindActionCreators } from 'redux';
import { actionCreators } from '../store/Notes';

class Notes extends React.Component {
    componentDidMount() {
        this.props.fetchNotes(this.props.isPublic);
    }

    render() {
        if (this.props.fetching)
            return <Spinner color="secondary"/>;

        return this.props.notes.length !== 0
            ? <ListGroup>
                {this.props.notes.map(n => <ListGroupItem className='lg light-purple'>{n}</ListGroupItem>)}
            </ListGroup>
            : <Alert color="info">no notes yes</Alert>;
    }
}

export default connect(
    state => state.notes,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Notes);
