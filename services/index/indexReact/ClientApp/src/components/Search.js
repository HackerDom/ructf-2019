import React from 'react';
import { Alert, Button, Col, Container, Form, FormGroup, Input, Label } from 'reactstrap';
import './NavMenu.css';

export default class Search extends React.Component {
    constructor(props) {
        super(props);
        this.state = { fileName: "", error: null, data: null };
        this.handleChange = this.handleChange.bind(this);
        this.submitForm = this.submitForm.bind(this);
    }

    submitForm(e) {
        e.preventDefault();
        this.setState({ error: null });
        fetch(`api/files?fileName=${this.state.fileName}`, {
            method: 'GET',
        }).then(async resp => {
            if (!resp.ok)
                if (resp.status === 403)
                    this.setState({ error: "Unauthorized" });
                else
                    throw await resp.json();
            this.setState({ data: await resp.json() });
        }).catch(json => this.setState({ error: json.error }));
    };

    handleChange = e => {
        const { target } = e;
        const { name } = target;
        this.setState({
            [name]: target.value,
        });
    };

    render() {
        return <Container>
            <Col sm={3} md={{ size: 8, offset: 2 }}>
                <Alert color="light">search your file by name and list containing directory</Alert>
                {this.state.error && <Col sm={5}><Alert color="danger">{this.state.error}</Alert></Col>}
                <Form onSubmit={this.submitForm} id="uploadForm">
                    <FormGroup row>
                        <Col sm={8}>
                            <Input type="text" placeholder="file name" name="fileName" id="fileName" onChange={this.handleChange} />
                        </Col>
                        <Button>find</Button>
                    </FormGroup>
                </Form>
            </Col>
        </Container>;
    }
}
