import * as React from "react";
import { Header, Segment, Container } from "semantic-ui-react";

const Forbidden: React.StatelessComponent = () => {
    return (
        <Container>
            <Segment color="blue">
                <Header as="h1" content="You have not access to this page"/>
            </Segment>  
        </Container>      
    );
};

export default Forbidden;
