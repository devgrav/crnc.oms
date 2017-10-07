import * as React from 'react';
import {Container, Header} from 'semantic-ui-react'
import 'semantic-ui-css/semantic.css'; 

export default class App extends React.Component {
    render() {
        return (
            <Container fluid> 
                <Header as="h1" content="Order management system"/>                                   
            </Container>           
        );
    }
}
