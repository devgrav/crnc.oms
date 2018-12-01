import * as React from 'react';
import { Form, Button, InputOnChangeData, Segment, Grid, Image } from 'semantic-ui-react';
import AuthService from '../../services/AuthService';
import * as Logo from "../../assets/images/logo.png";

export default class Login extends React.Component<any, LoginState> {


    constructor(props: any){
        super(props)
        this.state = {
            login: "",
            password: ""
        }

        this.onLoginChange = this.onLoginChange.bind(this);
        this.onPasswordChange = this.onPasswordChange.bind(this);
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
        AuthService.signIn();
    }

    public render() {
        let {login, password} = this.state;        
        return ( 
            <Grid centered columns={3}>
                <Grid.Column>
                    <Segment attached="top"><Image centered src={Logo} size="tiny"/></Segment>
                    <Segment attached >
                        <Form>
                            <Form.Input label="Login" value={login} onChange={this.onLoginChange}/>
                            <Form.Input label="Password" value={password} onChange={this.onPasswordChange} type="password"/>
                            <Button primary type="submit" content="Sign In"/>
                        </Form>
                    </Segment>
                </Grid.Column>
            </Grid>  
        );
  }
}

interface LoginState{
    login: string;
    password: string;
}