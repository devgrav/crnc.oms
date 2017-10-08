import * as React from "react";
import {Table} from "semantic-ui-react";
import {UserGridItem, UserService} from "../../services/UserService";

export default class UsersGrid extends React.Component<{}, UsersGridState> {
    constructor(props){
        super(props);

        this.state = {
            users: []
        };
    }

    public render() {
        return (
            <Table celled>
                <Table.Header>
                    <Table.Row>
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
            </Table>
        );
    }
}

interface UsersGridState{
    users: UserGridItem[];
}
