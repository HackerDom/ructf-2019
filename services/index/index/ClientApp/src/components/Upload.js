import React from 'react';
import { Alert, Button, Col, Form, FormGroup, Input, Label, Spinner } from 'reactstrap';

export default class Upload extends React.Component {
    constructor(props) {
        super(props);
        this.state = { file: null, error: null, fetching: false, success: false };
        this.handleChange = this.handleChange.bind(this);
        this.submitForm = this.submitForm.bind(this);
    }

    submitForm(e) {
        e.preventDefault();
        if (!this.state.file) {
            this.setState({ error: 'no file' });
            return;
        }
        this.setState({ error: null, fetching: true, success: false });
        const form = new FormData();
        form.append('file', this.state.file);
        fetch('api/files', {
            method: 'POST',
            body: form
        }).then(async resp => {
            if (!resp.ok) {
                if (resp.status === 403)
                    this.setState({ error: "Unauthorized" });
                else if (resp.status >= 500) {
                    this.setState({ error: 'internal server error' });
                } else
                    throw await resp.json();
            } else {
                this.setState({ success: true })
            }
        }).catch(json => this.setState({
            error: json.error
        })).finally(_ => this.setState({ fetching: false }));
    };

    handleChange = e => this.setState({ file: e.target.files[0] });

    render() {
        return <div className='upload-form'>
            <Form onSubmit={this.submitForm} id="uploadForm">
                <FormGroup row>

                    <Col sm={8}>
                        <Label for="file"
                               className='light-purple link upload-input-label'
                               >
                            {this.state.file ? `selected: ${this.state.file.name}` : 'select file'}
                        </Label>
                    </Col>
                    <Input className='upload-input' type="file" name="file" id="file" onChange={this.handleChange}/>
                    <Button>{this.state.fetching && <Spinner size="sm" color="dark"/>} upload</Button>
                </FormGroup>
            </Form>
            {this.state.error && <Alert color="danger">{this.state.error}</Alert>}
            {this.state.success && <Alert color="success">Uploaded</Alert>}
        </div>
    }
}
