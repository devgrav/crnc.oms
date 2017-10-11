import * as React from "react";
import { Button, Dimmer, Header, Icon, Loader, Menu, Segment, Table } from "semantic-ui-react";
import {UserGridItem, UserService} from "../../services/UserService";
import UsersGridRow from "./UsersGridRow";

export default class UsersGrid extends React.Component<{}, UsersGridState> {
    constructor(props){
        super(props);

        this.state = {
            users: [],
            isLoading: false
        };
    }

    private async getUsers(): Promise<void>{
        this.showLoading();

        const users = await UserService.getUsersGrid();

        this.setState({
            users
        });

        this.hideLoading();
    }

    public componentDidMount(): void{
        this.getUsers();
    }

    public showLoading(){
        this.setState({
            ...this.state, isLoading: true
        });
    }

    public hideLoading(){
        this.setState({
            ...this.state, isLoading: false
        });
    }

    public render() {
        return (
            <Dimmer.Dimmable dimmed={this.state.isLoading}>
                <Dimmer active={this.state.isLoading} inverted>
                    <Loader>Loading</Loader>
                </Dimmer>
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
            </Dimmer.Dimmable>
        );
    }
}

interface UsersGridState{
    users: UserGridItem[];
    isLoading: boolean;
}
