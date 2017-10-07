import * as React from "react";
import {Container} from "semantic-ui-react";
import Content from "./Content";
import TopMenu from "./TopMenu";

const Layout: React.StatelessComponent = () => {
    return (
        <Container fluid>
            <TopMenu/>
            <Content/>
        </Container>
    );
};

export default Layout;
