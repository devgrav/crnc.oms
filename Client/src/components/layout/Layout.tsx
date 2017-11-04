import * as React from "react";
import {Container} from "semantic-ui-react";
import Content from "./Content";
import TopMenu from "./TopMenu";

class Layout extends React.Component<{}> {

    constructor(props: any) {
        super(props);
    }

    public render(){
        return (
            <Container fluid>
                <TopMenu/>
                <Content>
                    {this.props.children}
                </Content>
            </Container>
        );
    }
}

export default Layout;
