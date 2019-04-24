import React from 'react';
import { connect } from 'react-redux';
import { Link } from 'react-router-dom';
import { Container, Nav, Navbar, NavbarBrand, NavItem, NavLink } from 'reactstrap';
import './NavMenu.css';
import { bindActionCreators } from 'redux';
import { actionCreators } from '../store/User';

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
                <Navbar className="navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3" light>
                    <Container>
                        <NavbarBrand tag={Link} to="/">index</NavbarBrand>
                        <Nav className="ml-auto" navbar>
                            <NavItem>
                                <NavLink tag={Link} className="text-dark" to="/notes">Notes</NavLink>
                            </NavItem>
                            {isLoggedIn &&
                            <React.Fragment>
                                <NavItem>
                                    <NavLink tag={Link} className="text-dark" to="/upload">Upload file</NavLink>
                                </NavItem>
                                <NavItem>
                                    <NavLink tag={Link} className="text-dark" to="/search">Find file</NavLink>
                                </NavItem>
                            </React.Fragment>}
                            <NavItem>
                                {!isLoggedIn
                                    ? <NavLink tag={Link} className="text-dark" to="/login">Login</NavLink>
                                    : <NavLink tag={Link}
                                               className="text-dark"
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