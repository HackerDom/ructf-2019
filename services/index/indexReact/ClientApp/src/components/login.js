import React from 'react';
import { Button, Col, Container, Form, FormGroup, Input, Label } from 'reactstrap';
import './NavMenu.css';
import { Redirect } from 'react-router-dom';

export default class Login extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            login: '',
            pwd: '',
        };

        this.handleChange = this.handleChange.bind(this);
    }

    submitForm() {
        const form = new FormData(document.getElementById('loginForm'));
        fetch('api/users/register', {
            method: 'POST',
            body: form
        }).then(_ => <Redirect to='/'/>);
        console.log(JSON.stringify(this.state))
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
        return (
            <Container>
                <Col sm={12} md={{ size: 4, offset: 4 }}>
                    <Form onSubmit={e => this.submitForm(e)} id="loginForm">
                        <FormGroup row>
                            <Label for="login" sm={3}>Login</Label>
                            <Col sm={9}>
                                <Input type="text" name="login" id="login" value={login} onChange={this.handleChange}/>
                            </Col>
                        </FormGroup>
                        <FormGroup row>
                            <Label for="pwd" sm={3}>Password</Label>
                            <Col sm={9}>
                                <Input type="password" name="pwd" id="pwd" value={pwd} onChange={this.handleChange}/>
                            </Col>
                        </FormGroup>
                        <FormGroup check row>
                            <Col sm={{ size: 20, offset: 3 }}>
                                <Button>Submit</Button>
                            </Col>
                        </FormGroup>
                    </Form>
                </Col>
            </Container>
        );
    }
}
