import React from 'react';
import { connect } from 'react-redux';
import {
    Alert,
    Badge,
    Button,
    Col,
    Form,
    FormGroup,
    Input,
    Label,
    ListGroup,
    ListGroupItem,
    Spinner
} from 'reactstrap';
import './NavMenu.css';
import { bindActionCreators } from 'redux';
import * as Notes from '../store/Notes';
import * as User from '../store/User';

class Note extends React.Component {
    constructor(props) {
        super(props);
        this.state = { note: "", public: false, error: null, success: false };
        this.handleChange = this.handleChange.bind(this);
        this.submitForm = this.submitForm.bind(this);
    }

    componentDidMount() {
        this.props.fetchNotes();
    }

    submitForm(e) {
        e.preventDefault();
        this.setState({ data: null });
        if (this.state.note === '') {
            this.setState({ error: "empty note" });
            return;
        }
        this.setState({ error: null, success: false });
        fetch(`api/notes`, {
            method: 'POST',
            body: {
                Text: this.state.note,
                IsPublic: this.state.public
            }
        }).then(async resp => {
            if (!resp.ok)
                if (resp.status === 403)
                    this.setState({ error: "Unauthorized" });
                else
                    throw await resp.json();
        }).then(_ => this.setState({
            success: true
        })).catch(json => this.setState({
            error: json.error
        }));
    };

    handleChange = e => {
        const { target } = e;
        const { name } = target;
        if (target.type === 'checkbox') {
            this.setState({
                [name]: target.checked,
            });
        } else {
            this.setState({
                [name]: target.value,
            });
        }
    };

    render() {
        return <React.Fragment>
            {this.renderNotes()}
            {this.props.user && <Form onSubmit={this.submitForm} style={{ marginTop: '20px' }}>
                <FormGroup row>
                    <Col sm={8}>
                        <Input type="textarea"
                               placeholder="Write new note here"
                               name="note"
                               id="note"
                               onChange={this.handleChange} />
                    </Col>
                </FormGroup>
                <FormGroup check>
                    <Label check>
                        <Input type="checkbox" name="public" id="public" onChange={this.handleChange} />{' '}Public
                    </Label>
                </FormGroup>
                <FormGroup>
                    <Button>{this.state.fetching && <Spinner size="sm" color="dark" />} post</Button>
                </FormGroup>
                {this.state.error && <Col sm={5}><Alert color="danger">{this.state.error}</Alert></Col>}
                {this.state.success && <Col sm={5}><Alert color="success">Uploaded</Alert></Col>}
            </Form>}
        </React.Fragment>;
    }

    renderNotes() {
        if (this.props.fetching)
            return <Spinner color="secondary" />;

        return <ListGroup>
            {this.props.notes.map(
                n => <ListGroupItem>
                    {!n.IsPublic && <Badge color="light">Private</Badge>}
                    {n.Text}
                </ListGroupItem>
            )}
        </ListGroup>;
    }
}

export default connect(
    state => state.user,
    dispatch => bindActionCreators(User.actionCreators, dispatch)
)(connect(
    state => state.notes,
    dispatch => bindActionCreators(Notes.actionCreators, dispatch)
)(Note));
