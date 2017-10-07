import * as React from "react";
import "semantic-ui-css/semantic.css";
import {Container, Header} from "semantic-ui-react";
import Layout from "./components/layout/Layout";

export default class App extends React.Component {
    public render() {
        return (
            <Container fluid>
                <Layout/>
            </Container>
        );
    }
}
