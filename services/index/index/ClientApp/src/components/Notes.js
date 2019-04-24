import React from 'react';
import { connect } from 'react-redux';
import { ListGroup, ListGroupItem, Spinner } from 'reactstrap';
import { bindActionCreators } from 'redux';
import { actionCreators } from '../store/Notes';

class Notes extends React.Component {
    componentDidMount() {
        this.props.fetchNotes(this.props.private);
    }
    componentDidUpdate() {
        this.props.fetchNotes(this.props.private);
    }

    render() {
        if (this.props.fetching)
            return <Spinner color="secondary" />;

        return <ListGroup>{this.props.notes.map(n => <ListGroupItem>{n.Text}</ListGroupItem>)}</ListGroup>;
    }
}

export default connect(
    state => state.notes,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Notes);
