import * as React from 'react';
import { Form, Button, InputOnChangeData, Segment, Grid, Image } from 'semantic-ui-react';
import AuthService from '../../services/AuthService';
import * as Logo from "../../assets/images/logo.png";
import { string } from 'prop-types';
import CurrentUserContext from '../../auth/CurrentUserContext';
import { Redirect, withRouter } from 'react-router';

export default class Login extends React.Component<any, LoginState> {

    constructor(props: any){
        super(props)
        this.state = {
            login: "",
            password: "",
            redirectToReferrer: false
        }

        this.onLoginChange = this.onLoginChange.bind(this);
        this.onPasswordChange = this.onPasswordChange.bind(this);
        this.onSignIn = this.onSignIn.bind(this);
    }

    onLoginChange(event: React.SyntheticEvent<HTMLInputElement>, data: InputOnChangeData){
        this.setState({
            login: data.value
        })
    }

    onPasswordChange(event: React.SyntheticEvent<HTMLInputElement>, data: InputOnChangeData){
        this.setState({
            password: data.value
        })
    }

    onSignIn(){
        const {login, password} = this.state;
        AuthService.signIn(login, password);
        this.setState({
            redirectToReferrer: true
        })
    }

    isSignInDisabled():boolean{
        return !this.state.login || !this.state.password; 
    }

    public render() {
        let {login, password, redirectToReferrer} = this.state;            
        let { from } = this.props.location.state || { from: { pathname: "/" } };
    
        if (redirectToReferrer) return <Redirect to={from} />;

        return ( 
            <Grid style={{paddingTop: "50px"}} centered columns={3}>
                <Grid.Column>
                    <Segment attached="top"><Image centered src={Logo} size="tiny"/></Segment>
                    <Segment attached >
                        <Form onSubmit={this.onSignIn}>
                            <Form.Input label="Login" value={login} onChange={this.onLoginChange}/>
                            <Form.Input label="Password" value={password} onChange={this.onPasswordChange} type="password"/>
                            <Button primary type="submit" content="Sign In" disabled={this.isSignInDisabled()}/>
                        </Form>
                    </Segment>
                </Grid.Column>
            </Grid>  
        );
  }
}

withRouter(Login)

interface LoginState{
    login: string;
    password: string;
    redirectToReferrer: boolean;
}