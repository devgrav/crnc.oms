import * as React from "react";
import { BrowserRouter as Router } from "react-router-dom";
import "semantic-ui-css/semantic.css";
import {Container, Header} from "semantic-ui-react";
import Layout from "./components/layout/Layout";
import Routes from "./routes";

export default class App extends React.Component<any, any> {
    
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
