import React from 'react';
import { connect } from 'react-redux';
import { Alert, Button, Col, Form, FormGroup, Input, Label, Nav, NavItem, NavLink, Spinner } from 'reactstrap';
import './NavMenu.css';
import { bindActionCreators } from 'redux';
import * as User from '../store/User';
import Notes from './Notes';

class NotesPage extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            note: "",
            public: false,
            error: null,
            success: false,
            tab: 1,
        };

        this.handleFormChange = this.handleFormChange.bind(this);
        this.handleTabChange = this.handleTabChange.bind(this);
        this.submitForm = this.submitForm.bind(this);
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
            body: JSON.stringify({
                Note: this.state.note,
                IsPublic: this.state.public
            }),
            headers: {
                'Content-Type': 'application/json',
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

    handleFormChange = e => {
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

    handleTabChange = n => _ => {
        this.setState({ tab: n });
    };

    render() {
        return <React.Fragment>
            <Nav tabs>
                <NavItem>
                    <NavLink href="#"
                             active={this.state.tab === 1}
                             onClick={this.handleTabChange(1)}>Public</NavLink>
                </NavItem>
                <NavItem>
                    <NavLink href="#"
                             active={this.state.tab === 2}
                             onClick={this.handleTabChange(2)}>Private</NavLink>
                </NavItem>
                <NavItem>
                    <NavLink href="#"
                             active={this.state.tab === 3}
                             onClick={this.handleTabChange(3)}>Add note</NavLink>
                </NavItem>
            </Nav>

            {this.renderContent()}
        </React.Fragment>;
    }

    renderForm() {
        return this.user
            ? <Form onSubmit={this.submitForm} style={{ marginTop: '20px' }}>
                <FormGroup row>
                    <Col sm={8}>
                        <Input type="textarea"
                               placeholder="Write new note here"
                               name="note"
                               id="note"
                               onChange={this.handleFormChange} />
                    </Col>
                </FormGroup>
                <FormGroup check>
                    <Label check>
                        <Input type="checkbox" name="public" id="public" onChange={this.handleFormChange} />{' '}Public
                    </Label>
                </FormGroup>
                <FormGroup>
                    <Button>{this.state.fetching && <Spinner size="sm" color="dark" />} post</Button>
                </FormGroup>
                {this.state.error && <Col sm={5}><Alert color="danger">{this.state.error}</Alert></Col>}
                {this.state.success && <Col sm={5}><Alert color="success">Uploaded</Alert></Col>}
            </Form>
            : <Alert color="warning">You need to login</Alert> ;
    }

    renderContent() {
        switch (this.state.tab) {
            case 1:
                return <Notes key="pub" isPublic={true} />;
            case 2:
                return <Notes key="priv" isPublic={false} />;
            case 3:
                return this.renderForm();
            default:
                return null;
        }
    }
}

export default connect(
    state => state.user,
    dispatch => bindActionCreators(User.actionCreators, dispatch)
)(NotesPage);


