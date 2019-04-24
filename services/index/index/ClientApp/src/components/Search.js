import React from 'react';
import { Alert, Button, Col, Container, Form, FormGroup, Input, ListGroup, ListGroupItem, Spinner } from 'reactstrap';
import './NavMenu.css';

export default class Search extends React.Component {
    constructor(props) {
        super(props);
        this.state = { fileName: "", error: null, data: null, fetching: false };
        this.handleChange = this.handleChange.bind(this);
        this.submitForm = this.submitForm.bind(this);
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
        return <Container>
            <Col sm={3} md={{ size: 8, offset: 2 }}>
                <Alert color="light">search your file by name and list containing directories</Alert>
                <Form onSubmit={this.submitForm} id="uploadForm">
                    <FormGroup row>
                        <Col sm={8}>
                            <Input type="text"
                                   placeholder="file name"
                                   name="fileName"
                                   id="fileName"
                                   onChange={this.handleChange} />
                        </Col>
                        <Button>
                            {this.state.fetching && <Spinner size="sm" color="dark" />}
                            find</Button>
                    </FormGroup>
                </Form>
                {this.state.error && <Col sm={5}><Alert color="danger">{this.state.error}</Alert></Col>}
                {this.state.data &&
                <ListGroup>
                    {this.state.data.map(dir =>
                        <ListGroupItem>
                            {dir.map(d => <div>{d}</div>)}
                        </ListGroupItem>)}
                </ListGroup>}
            </Col>
        </Container>;
    }
}