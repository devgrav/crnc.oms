import * as React from "react";
import { Header, Segment, Container } from "semantic-ui-react";

const NotFound: React.StatelessComponent = () => {
    return (
        <Container>
            <Segment color="blue">
                <Header as="h1" content="Page not found"/>
            </Segment>  
        </Container>  
    );
};

export default NotFound;
