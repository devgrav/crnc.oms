import * as React from "react";
import {Container} from "semantic-ui-react";
import EstimatesGrid from "../estimates/EstimatesGrid";
import UsersGrid from "../users/UsersGrid";
import Content from "./Content";
import TopMenu from "./TopMenu";

class Layout extends React.Component<{}, LayoutState> {

    constructor() {
        super();

        this.state = {
            activePage: "users"
        };

        this.onActivePageChange = this.onActivePageChange.bind(this);
    }

    private onActivePageChange(page: string): void{
        this.setState({
            activePage: page
        });
    }

    private getChildrenContent(): React.ReactElement<any>{
        switch (this.state.activePage){
            case "users":
                return <UsersGrid/>;
            case "estimates":
                return <EstimatesGrid/>;
            default :
                return <div/>;
        }
    }

    public render(){
        return (
            <Container fluid>
                <TopMenu
                    activeItem={this.state.activePage}
                    onChangeActivePage={this.onActivePageChange}
                />
                <Content
                    children={this.getChildrenContent()}
                />
            </Container>
        );
    }
}

export default Layout;

interface LayoutState{
    activePage: string;
}
