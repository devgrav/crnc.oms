import * as React from "react";
import { Button, Table, Header, Menu, Icon } from "semantic-ui-react";
import {UserGridItem, UserService} from "../../services/UserService";
import UsersGridRow from "./UsersGridRow";

export default class UsersGrid extends React.Component<{}, UsersGridState> {
    constructor(props){
        super(props);

        this.state = {
            users: []
        };
    }

    private getUsers(): void{
        UserService.getUsersGrid()
            .then((users) => {
                this.setState({
                    users
                });
            });
    }

    public componentDidMount(): void{
        this.getUsers();
    }

    public render() {
        return (
            <Table celled selectable striped definition>
                <Table.Header fullWidth>
                    <Table.Row>
                        <Table.HeaderCell colSpan="6">
                            <Button floated="right" primary content="Add user" icon="plus"/>
                        </Table.HeaderCell>
                    </Table.Row>
                </Table.Header>
                <Table.Header fullWidth>
                    <Table.Row>
                        <Table.HeaderCell width={1}/>
                        <Table.HeaderCell>
                            Photo
                        </Table.HeaderCell>
                        <Table.HeaderCell>
                            Name
                        </Table.HeaderCell>
                        <Table.HeaderCell>
                            Login
                        </Table.HeaderCell>
                        <Table.HeaderCell>
                            Email
                        </Table.HeaderCell>
                        <Table.HeaderCell>
                            Activity
                        </Table.HeaderCell>
                    </Table.Row>
                </Table.Header>
                <Table.Body>
                    {this.state.users.map((user) =>
                        <UsersGridRow key={user.id} userItem={user}/>)}
                </Table.Body>
                <Table.Footer fullWidth>
                    <Table.Row>
                        <Table.HeaderCell colSpan="6">
                            <Menu floated="right" pagination>
                                <Menu.Item as="a" icon>
                                    <Icon name="left chevron" />
                                </Menu.Item>
                                <Menu.Item as="a">1</Menu.Item>
                                <Menu.Item as="a">2</Menu.Item>
                                <Menu.Item as="a">3</Menu.Item>
                                <Menu.Item as="a">4</Menu.Item>
                                <Menu.Item as="a" icon>
                                <Icon name="right chevron" />
                                </Menu.Item>
                            </Menu>
                        </Table.HeaderCell>
                    </Table.Row>
            </Table.Footer>
            </Table>
        );
    }
}

interface UsersGridState{
    users: UserGridItem[];
}
