import React from 'react';
import { connect } from 'react-redux';
import { Alert, Button, Col, Container, Form, FormGroup, Input, Label } from 'reactstrap';
import './NavMenu.css';
import { bindActionCreators } from 'redux';
import { actionCreators } from '../store/User';

class Login extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            login: '',
            pwd: '',
            error: null,
        };

        this.handleChange = this.handleChange.bind(this);
    }

    submitForm(props) {
        return e => {
            e.preventDefault();
            const form = new FormData(document.getElementById('loginForm'));
            this.setState({ error: null });
            fetch('api/users/register', {
                method: 'POST',
                body: form
            }).then(resp => {
                if (!resp.ok)
                    throw resp;
            }).then(_ => {
                props.history.push('/');
                props.fetchUser();
            }).catch(_ => this.setState({ error: "User already exists" })
            );
        };
    }

    handleChange = async (event) => {
        const { target } = event;
        const { name } = target;
        await this.setState({
            [name]: target.value,
        });
    };

    render() {
        const { login, pwd } = this.state;
        return <Container>
            <Col sm={12} md={{ size: 4, offset: 4 }}>
                <Alert>You also can register here, just use unique login:)</Alert>
                <Form onSubmit={this.submitForm(this.props)} id="loginForm">
                    <FormGroup row>
                        <Label for="login" sm={3}>Login</Label>
                        <Col sm={9}>
                            <Input type="text" name="login" id="login" value={login} onChange={this.handleChange} />
                        </Col>
                    </FormGroup>
                    <FormGroup row>
                        <Label for="pwd" sm={3}>Password</Label>
                        <Col sm={9}>
                            <Input type="password" name="pwd" id="pwd" value={pwd} onChange={this.handleChange} />
                        </Col>
                    </FormGroup>
                    <FormGroup check row>
                        <Col sm={{ size: 20, offset: 3 }}>
                            <Button>Submit</Button>
                        </Col>
                    </FormGroup>
                </Form>
                {this.state.error && <Alert color="danger">{this.state.error}</Alert>}
            </Col>
        </Container>;
    }
}

export default connect(
    state => state.user,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Login);
