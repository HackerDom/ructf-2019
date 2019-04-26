import React from 'react';
import { connect } from 'react-redux';
import { Link } from 'react-router-dom';
import { Container, Nav, Navbar, NavbarBrand, NavItem, NavLink } from 'reactstrap';
import './NavMenu.css';
import { bindActionCreators } from 'redux';
import { actionCreators } from '../../store/User';

class NavMenu extends React.Component {
    logOut(props) {
        return async e => {
            e.preventDefault();
            await fetch('api/users/logout', {
                method: 'POST',
            }).then(_ => {
                props.history.push('/');
                props.removeUser();
            });
        };
    }

    render() {
        const isLoggedIn = !!this.props.user;

        return (
            <header>
                <Navbar className="navbar-expand-sm navbar-toggleable-sm box-shadow mb-3 bg" light>
                    <Container>
                        <NavbarBrand tag={Link} className="yellow-main" to="/"><span className='mr-3'>index</span>
                            <svg height="30pt"
                                 viewBox="-63 0 479 480"
                                 width="30pt"
                                 xmlns="http://www.w3.org/2000/svg"
                                 transform='rotate(90)'>
                                <g fill="#bcb41c">
                                    <path d="m328.5 0h-256c-13.253906 0-24 10.746094-24 24v140.6875l-45.65625 45.65625c-1.5 1.5-2.34375 3.535156-2.34375 5.65625v32c0 4.417969 3.582031 8 8 8h16v28.6875l-21.65625 21.65625c-1.5 1.5-2.34375 3.535156-2.34375 5.65625v144c0 13.253906 10.746094 24 24 24h304c13.253906 0 24-10.746094 24-24v-432c0-13.253906-10.746094-24-24-24zm8 456c0 4.417969-3.582031 8-8 8h-304c-4.417969 0-8-3.582031-8-8v-140.6875l21.65625-21.65625c1.5-1.5 2.34375-3.535156 2.34375-5.65625v-40c0-4.417969-3.582031-8-8-8h-16v-20.6875l45.65625-45.65625c1.5-1.5 2.34375-3.535156 2.34375-5.65625v-144c0-4.417969 3.582031-8 8-8h256c4.417969 0 8 3.582031 8 8zm0 0"/>
                                    <path d="m88.5 144h32c4.417969 0 8-3.582031 8-8v-80c0-4.417969-3.582031-8-8-8h-32c-4.417969 0-8 3.582031-8 8v80c0 4.417969 3.582031 8 8 8zm8-80h16v64h-16zm0 0"/>
                                    <path d="m152.5 144h32c4.417969 0 8-3.582031 8-8v-96c0-4.417969-3.582031-8-8-8h-32c-4.417969 0-8 3.582031-8 8v96c0 4.417969 3.582031 8 8 8zm8-96h16v80h-16zm0 0"/>
                                    <path d="m216.5 144h32c4.417969 0 8-3.582031 8-8v-80c0-4.417969-3.582031-8-8-8h-32c-4.417969 0-8 3.582031-8 8v80c0 4.417969 3.582031 8 8 8zm8-80h16v64h-16zm0 0"/>
                                    <path d="m312.5 32h-32c-4.417969 0-8 3.582031-8 8v96c0 4.417969 3.582031 8 8 8h32c4.417969 0 8-3.582031 8-8v-96c0-4.417969-3.582031-8-8-8zm-8 96h-16v-80h16zm0 0"/>
                                    <path d="m312.5 392h-272c-4.417969 0-8 3.582031-8 8v40c0 4.417969 3.582031 8 8 8h272c4.417969 0 8-3.582031 8-8v-40c0-4.417969-3.582031-8-8-8zm-8 40h-256v-24h256zm0 0"/>
                                    <path d="m80.5 160h240v16h-240zm0 0"/>
                                </g>
                            </svg>
                        </NavbarBrand>

                        <Nav className="ml-auto" navbar>
                            <NavItem>
                                <NavLink tag={Link} className="light-purple link" to="/notespage">Notes</NavLink>
                            </NavItem>
                            {isLoggedIn &&
                            <React.Fragment>
                                <NavItem>
                                    <NavLink tag={Link} className="light-purple link" to="/upload">Upload file</NavLink>
                                </NavItem>
                                <NavItem>
                                    <NavLink tag={Link} className="light-purple link" to="/search">Find file</NavLink>
                                </NavItem>
                            </React.Fragment>}
                            <NavItem>
                                {!isLoggedIn
                                    ? <NavLink tag={Link} className="light-purple link" to="/login">Login</NavLink>
                                    : <NavLink tag={Link}
                                               className="light-purple link"
                                               to="/logout"
                                               onClick={this.logOut(this.props)}>Logout</NavLink>
                                }
                            </NavItem>
                        </Nav>
                    </Container>
                </Navbar>
            </header>
        );
    }
}

export default connect(
    state => state.user,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(NavMenu);