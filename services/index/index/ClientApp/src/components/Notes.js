import React from 'react';
import { connect } from 'react-redux';
import { Alert, Button, Col, Container, Form, FormGroup, Input, ListGroup, ListGroupItem, Spinner } from 'reactstrap';
import './NavMenu.css';
import { bindActionCreators } from 'redux';
import { actionCreators } from '../store/Notes';

class Note extends React.Component {
    constructor(props) {
        super(props);
        this.state = { fileName: "", error: null, data: null, fetching: false };
        this.handleChange = this.handleChange.bind(this);
        this.submitForm = this.submitForm.bind(this);
    }

    componentDidMount() {
        this.props.fetchNotes();
    }

    submitForm(e) {
        e.preventDefault();
        this.setState({ data: null });
        if (!this.state.fileName) {
            this.setState({ error: "no file name" });
            return;
        }
        this.setState({ error: null, fetching: true });
        fetch(`api/files?fileName=${this.state.fileName}`, {
            method: 'GET',
        }).then(async resp => {
            if (!resp.ok)
                if (resp.status === 403)
                    this.setState({ error: "Unauthorized" });
                else
                    throw resp;
            this.setState({ data: await resp.json() });
        }).catch(() => this.setState({ error: `can't find ${this.state.fileName}` })).finally(_ => this.setState({ fetching: false }));
    };

    handleChange = e => {
        const { target } = e;
        const { name } = target;
        this.setState({
            [name]: target.value,
        });
    };

    render() {
        if (this.props.fetching)
            return <Spinner color="secondary" />;
        return <div>asdasdasdasd</div>;
    }
}

export default connect(
    state => state.notes,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Note);
