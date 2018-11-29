import * as React from "react";
import { Link, NavLink } from "react-router-dom";
import { Menu, MenuItemProps } from "semantic-ui-react";
import * as Logo from "../../assets/images/logo.png";
import UserInfo from "./UserInfo";

export default class TopMenu
    extends React.Component<{}> {

    constructor(props: any) {
        super(props);
    }

     isActiveForDefault(match: any, location: any){
        if(location.pathname ==="/")
            return true;

        if (!match) {
          return false
        }            

        return true;
    }

    public render(){
        return (
            <Menu>
                <Menu.Item as={Link} to="/">
                   <img src={Logo}/>
                </Menu.Item>
                <Menu.Item as={NavLink} isActive={this.isActiveForDefault} to="/users" name="users" link>
                    Users
                </Menu.Item>
                <Menu.Item as={NavLink} to="/estimates" name="estimates" link>
                    Estimates
                </Menu.Item>
                <Menu.Item as={NavLink} to="/jobs" name="jobs" link>
                    Jobs
                </Menu.Item>
                <Menu.Menu position="right">
                    <Menu.Item>
                        <UserInfo/>
                    </Menu.Item>
                </Menu.Menu>
            </Menu>
        );
    }
}
