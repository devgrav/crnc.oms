import * as React from "react";
import { Link } from "react-router-dom";
import { Menu, MenuItemProps } from "semantic-ui-react";
import * as Logo from "../../assets/images/logo.png";

export default class TopMenu
    extends React.Component<{}> {

    constructor(props: any) {
        super(props);
    }

    public render(){
        return (
            <Menu>
                <Menu.Item as={Link} to="/">
                   <img src={Logo}/>
                </Menu.Item>
                <Menu.Item as={Link} to="/users" name="users" link>
                    Users
                </Menu.Item>
                <Menu.Item as={Link} to="/estimates" name="estimates" link>
                    Estimates
                </Menu.Item>
                <Menu.Item as={Link} to="/jobs" name="jobs" link>
                    Jobs
                </Menu.Item>
            </Menu>
        );
    }
}
