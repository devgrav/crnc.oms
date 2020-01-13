import * as React from "react";
import { Link, NavLink, withRouter } from "react-router-dom";
import { Menu, MenuItemProps, Label, Icon } from "semantic-ui-react";
import * as Logo from "../../assets/images/logo.png";
import UserInfo from "./UserInfo";
import Notifications from "./Notifications";

export default class TopMenu
    extends React.Component<any,{}> {

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
            <React.Fragment>
                <Menu>
                    <Menu.Item as={Link} to="/">
                        <img src={Logo}/>
                    </Menu.Item>
                    <Menu.Item as={NavLink} to="/users" name="users" link>
                        Users
                    </Menu.Item>
                    <Menu.Item as={NavLink} isActive={this.isActiveForDefault} to="/orders" name="orders" link>
                        Orders
                    </Menu.Item>
                    <Menu.Menu position="right">
                        <Menu.Item header>
                            <Notifications/>                         
                        </Menu.Item>                    
                        <Menu.Item>
                            <UserInfo/>
                        </Menu.Item>
                    </Menu.Menu>
                </Menu>
        </React.Fragment>
        );
    }
}
