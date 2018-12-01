import * as React from 'react';
import CurrentUserContext from '../../auth/CurrentUserContext';
import { Segment, Icon, Header } from 'semantic-ui-react';
import AuthService from '../../services/AuthService';
import { Redirect } from 'react-router';

export default class UserInfo extends React.Component<any, UserInfoState> {

    constructor(props: any){
        super(props);

        this.state ={
            isAuthentificated: CurrentUserContext.isAuthentificated
        }

        this.onSignOut = this.onSignOut.bind(this)
    }

    onSignOut(){
        AuthService.signOut()

        this.setState({
            isAuthentificated: CurrentUserContext.isAuthentificated
        })
    }

    public render() {
        if(!this.state.isAuthentificated)
            return <Redirect to="/login"/>

        return (
            <Header>
                <Icon 
                    link name="sign out" 
                    title="logout" 
                    onClick={this.onSignOut}/>
                {CurrentUserContext.user.login}
            </Header>
        )
    }
}

interface UserInfoState{
    isAuthentificated: boolean
}