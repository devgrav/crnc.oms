import * as React from "react";
import { BrowserRouter as Router } from "react-router-dom";
import "semantic-ui-css/semantic.css";
import {Container, Header} from "semantic-ui-react";
import Layout from "./components/layout/Layout";
import Routes from "./routes";
import CurrentUserContext from "./auth/CurrentUserContext";
import CurrentUser from "./auth/CurrentUser";

export default class App extends React.Component<any, any> {
    
    constructor(props: any){
        super(props)

        this.tryInitCurrentUser();        
    }

    tryInitCurrentUser(){
        let userJson = sessionStorage.getItem("crnc.oms.currentUser");
        let user = userJson ? JSON.parse(userJson) as CurrentUser : null;

        if(user){
            CurrentUserContext.init(user)
        }        
    }

    public render() {        
        return (
            <Router>
                <Container fluid>
                    <Routes/>
                </Container>
            </Router>
        );
    }
}
