import React from 'react';
import { Alert, Button, Col, Container, Form, FormGroup, Input, Label } from 'reactstrap';
import './NavMenu.css';

export default class Upload extends React.Component {
    constructor(props) {
        super(props);
        this.state = { file: null, error: null };
        this.handleChange = this.handleChange.bind(this);
        this.submitForm = this.submitForm.bind(this);
    }

    submitForm(e) {
        e.preventDefault();
        this.setState({ error: null });
        const form = new FormData();
        form.append('file', this.state.file);

        fetch('api/zip', {
            method: 'POST',
            body: form
        }).then(async resp => {
            if (!resp.ok)
                if (resp.status === 403)
                    this.setState({ error: "Unauthorized" });
                else
                    throw await resp.json();
        }).catch(json => this.setState({ error: json.error }));
    };


    handleChange = e => this.setState({ file: e.target.files[0] });

    render() {
        return <Container>
            <Col sm={3} md={{ size: 8, offset: 4 }}>
                {this.state.error && <Col sm={5}><Alert color="danger">{this.state.error}</Alert></Col>}
                <Form onSubmit={this.submitForm} id="uploadForm">
                    <FormGroup row>
                        <Label for="file" sm={2}>Zip File</Label>
                        <Col sm={9}>
                            <Input type="file" name="file" id="file" onChange={this.handleChange} />
                        </Col>
                    </FormGroup>
                    <FormGroup check row>
                        <Col sm={{ size: 20, offset: 3 }}>
                            <Button>upload</Button>
                        </Col>
                    </FormGroup>
                </Form>
            </Col>
        </Container>;
    }
}
