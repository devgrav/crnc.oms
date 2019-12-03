import * as React from 'react';
import { Form, Button, InputOnChangeData, Segment, Grid, Image, Message } from 'semantic-ui-react';
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
            redirectToReferrer: false,
            isLoading: false
        }

        this.onLoginChange = this.onLoginChange.bind(this);
        this.onPasswordChange = this.onPasswordChange.bind(this);
        this.onSignIn = this.onSignIn.bind(this);
    }

    showError(error: string){
        this.setState({
            errorMessage: error
        })
    }

    hideError(){
        this.setState({
            errorMessage: ""
        })
    }

    showLoading(){
        this.setState({
            isLoading: true
        })
    }

    hideLoading(){
        this.setState({
            isLoading: false
        })
    }

    onLoginChange(event: React.SyntheticEvent<HTMLInputElement>, data: InputOnChangeData){
        this.setState({
            login: data.value
        })

        this.hideError();
    }
    
    onPasswordChange(event: React.SyntheticEvent<HTMLInputElement>, data: InputOnChangeData){
        this.setState({
            password: data.value
        })

        this.hideError();
    }

    async onSignIn(){
        const {login, password} = this.state;

        try {
            this.showLoading();
            await AuthService.signIn(login, password)   
            
            if(CurrentUserContext.isAuthentificated)
                this.setState({
                    redirectToReferrer: true
                })                
        } catch (error) {
            if(error.response){
                this.showError(error.response.data);
            }            
        }
        finally{
            this.hideLoading();
        }    
    }

    isSignInDisabled():boolean{
        return !this.state.login || !this.state.password; 
    }

    public render() {
        let {login, password, redirectToReferrer, errorMessage, isLoading} = this.state;            
        let { from } = this.props.location.state || { from: { pathname: "/" } };
    
        if (redirectToReferrer) return <Redirect to={from} />;

        return ( 
            <Grid style={{paddingTop: "50px"}} centered columns={3}>
                <Grid.Column>
                    <Segment attached="top"><Image centered src={Logo} size="tiny"/></Segment>
                    <Segment attached >
                        {errorMessage && <Message error content={errorMessage}/>}
                        <Form loading={isLoading} onSubmit={this.onSignIn}>
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
    isLoading: boolean;
    redirectToReferrer: boolean;
    errorMessage?: string;
}