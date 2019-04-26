import React from 'react';
import { Alert, Button, Col, Container, Form, FormGroup, Input, Label, Spinner } from 'reactstrap';

export default class Upload extends React.Component {
    constructor(props) {
        super(props);
        this.state = { file: null, error: null, fetching: false, success: false };
        this.handleChange = this.handleChange.bind(this);
        this.submitForm = this.submitForm.bind(this);
    }

    submitForm(e) {
        e.preventDefault();
        this.setState({ error: null, fetching: true, success: true });
        const form = new FormData();
        form.append('file', this.state.file);

        fetch('api/files', {
            method: 'POST',
            body: form
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
        })).finally(_ => this.setState({ fetching: false }));
    };

    handleChange = e => this.setState({ file: e.target.files[0] });

    render() {
        return <Container>
            <Col sm={3} md={{ size: 8, offset: 2 }}>
                <Form onSubmit={this.submitForm} id="uploadForm">
                    <FormGroup row>
                        <Label for="file" sm={2}>Zip File</Label>
                        <Col sm={7}>
                            <Input type="file" name="file" id="file" onChange={this.handleChange} />
                        </Col>
                        <Button>
                            {this.state.fetching && <Spinner size="sm" color="dark" />}
                            upload</Button>
                    </FormGroup>
                </Form>
                {this.state.error && <Col sm={5}><Alert color="danger">{this.state.error}</Alert></Col>}
                {this.state.success && <Col sm={5}><Alert color="success">Uploaded</Alert></Col>}
            </Col>
        </Container>;
    }
}
