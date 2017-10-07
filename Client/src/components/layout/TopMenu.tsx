import * as React from "react";
import { Menu, MenuItemProps } from "semantic-ui-react";
import * as Logo from "../../assets/images/logo.png";

export default class TopMenu
    extends React.Component<{}, TopMenuState> {

    constructor() {
        super();
        this.state = {
            activeItem: "users"
        };

        this.handleMenuItemClick = this.handleMenuItemClick.bind(this);
    }

    private handleMenuItemClick(event: React.MouseEvent<HTMLAnchorElement>,
                                data: MenuItemProps): void{
        this.setState({
            activeItem: data.name
        });
    }

    public render(){
        return (
            <Menu>
                <Menu.Item>
                   <img src={Logo}/>
                </Menu.Item>
                <Menu.Item onClick={this.handleMenuItemClick} active={this.state.activeItem === "users"}>
                    users
                </Menu.Item>
                <Menu.Item onClick={this.handleMenuItemClick} active={this.state.activeItem === "estimates"}>
                    estimates
                </Menu.Item>
                <Menu.Item onClick={this.handleMenuItemClick} active={this.state.activeItem === "jobs"}>
                    jobs
                </Menu.Item>
            </Menu>
        );
    }
}

export interface TopMenuState{
    activeItem: string;
}
