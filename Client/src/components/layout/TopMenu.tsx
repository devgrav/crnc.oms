import * as React from "react";
import { Menu, MenuItemProps } from "semantic-ui-react";
import * as Logo from "../../assets/images/logo.png";

export default class TopMenu
    extends React.Component<TopMenuProps> {

    constructor(props: TopMenuProps) {
        super(props);

        this.onMenuItemClick = this.onMenuItemClick.bind(this);
    }

    private onMenuItemClick(event: React.MouseEvent<HTMLAnchorElement>,
                            data: MenuItemProps): void{
        this.props.onChangeActivePage(data.name);
    }

    public render(){
        return (
            <Menu>
                <Menu.Item>
                   <img src={Logo}/>
                </Menu.Item>
                <Menu.Item
                    name="users"
                    onClick={this.onMenuItemClick}
                    active={this.props.activeItem === "users"}
                >
                    Users
                </Menu.Item>
                <Menu.Item
                    name="estimates"
                    onClick={this.onMenuItemClick}
                    active={this.props.activeItem === "estimates"}
                >
                    Estimates
                </Menu.Item>
                <Menu.Item
                    name="jobs"
                    onClick={this.onMenuItemClick}
                    active={this.props.activeItem === "jobs"}
                >
                    Jobs
                </Menu.Item>
            </Menu>
        );
    }
}

export interface TopMenuProps{
    activeItem: string;
    onChangeActivePage(page: string): void;
}
